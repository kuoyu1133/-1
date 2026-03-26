using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class CountryAIAgent : Agent
{
    // [核心新增] 鎖定狀態，防止在協程執行期間重複觸發動作
    private bool isProcessing = false;

    public CountryStateManager country;
    public CountryStateManager target;

    public TradeSystem tradeSystem;
    public AnnouncementSystem announcement;
    public BattleSystem battleSystem;
    public PolicySystem policySystem;
    public OccupationSystem occupationSystem;
    public CSVTrainingLogger logger;

    protected override void OnEnable()
    {
        base.OnEnable();
        // 確保醒來時，處理鎖是開著的
        isProcessing = false;
        if (LogManager.Instance != null && country != null)
        {
            LogManager.Instance.EnqueueActionLog(country.CountryName, "AI 系統已喚醒並接管控制。", true);
        }
        else
        {
            // 如果 LogManager 還沒準備好，先用 Debug.Log 墊著，避免當機
            Debug.Log("<color=cyan>[系統]</color> AI Agent 已啟動");
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable(); // 🚩 這行非常重要！執行 ML-Agents 原有的關閉邏輯

        StopAllCoroutines();
        isProcessing = false;
    }

    public override void OnEpisodeBegin()
    {
        if (country != null && country.resource != null)
        {
            country.resource.ResetAllCountries();
        }
        if (logger != null) logger.OnNewEpisode();

        RuleBasedCountry rbc = Object.FindFirstObjectByType<RuleBasedCountry>();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        Country c = country.resource.countries.Find(x => x.CountryName == country.CountryName);
        Country t = target.resource.countries.Find(x => x.CountryName == target.CountryName);

        // ✅ 解決 Fewer observations 警告：若數據尚未準備好，填充 14 個 0
        if (c == null || t == null)
        {
            for (int i = 0; i < 14; i++) sensor.AddObservation(0f);
            return;
        }

        // 觀察值 (共 14 個)+2觀察政策冷卻時間  (26/03/11)
        sensor.AddObservation((float)c.Food / 10000f);
        sensor.AddObservation((float)c.Iron / 10000f);
        sensor.AddObservation((float)c.Wood / 10000f);
        sensor.AddObservation(c.MilPower / 100f);
        sensor.AddObservation((float)c.morale.MoraleValue / 100f);
        sensor.AddObservation((float)c.Population / 50000f);
        sensor.AddObservation((float)c.AP / 50f);
        sensor.AddObservation((float)country.trust.GetTrust(target.CountryName));

        sensor.AddObservation((float)t.Food / 10000f);
        sensor.AddObservation((float)t.Iron / 10000f);
        sensor.AddObservation((float)t.Wood / 10000f);
        sensor.AddObservation(t.MilPower / 100f);
        sensor.AddObservation((float)t.morale.MoraleValue / 100f);
        sensor.AddObservation((float)t.AP / 50f);
        sensor.AddObservation(policySystem.IsPolicyReady(country.CountryName, "Pop") ? 1f : 0f);
        sensor.AddObservation(policySystem.IsPolicyReady(country.CountryName, "Mil") ? 1f : 0f);
        /*// 觀察 RBC 是否有餘糧可以賣給我 (1 代表有，0 代表沒)
        int rbcFood = t.Food;
        sensor.AddObservation(rbcFood > (t.Population / 20 * 5) ? 1.0f : 0.0f);*/
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 如果協程還在跑，直接跳過這幀的決策請求
        if (isProcessing) return;

        float startDelay = (country.CountryName.Contains("B")) ? 0.5f : 0f;

        int act = actions.DiscreteActions[0];
        Country agentData = country.resource.countries.Find(x => x.CountryName == country.CountryName);
        Country targetData = target.resource.countries.Find(x => x.CountryName == target.CountryName);
        if (agentData == null) return;

        // 改用協程來控制動作順序與時間延遲
        StartCoroutine(ProcessActionStep(act, agentData, targetData, startDelay));

        // 2. 驅動遊戲進入下一天 (同步 RBC 與 資源扣除)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterAgentAction();
        }

        //AddReward(0.1f);
        CheckEpisodeEnd(agentData);
        GameManager.Instance.RegisterAgentAction();
    }

    // [核心協程] 控制「執行動作 -> 停頓 -> 更新遊戲 -> 結束」的流程
    IEnumerator ProcessActionStep(int act, Country agentData, Country targetData, float startDelay)
    {
        isProcessing = true; // 上鎖

        if (startDelay > 0) yield return new WaitForSeconds(startDelay);

        string actionName = GetActionName(act);
        LogManager.Instance.EnqueueLog($"<color=green>[AI]</color> {country.CountryName} 選擇了：<b>{actionName}</b> | 當前 AP: {agentData.AP}");

        // 1. 執行具體動作與輸出 Log
        ExecuteAction(act, agentData, targetData);

        // 2. 停頓 2 秒（受 Time.timeScale 影響）
        float waitTime = (country.CountryName.Contains("A")) ? 1.5f : 1.0f;
        yield return new WaitForSeconds(waitTime);

        // 3. 動作結束後，驅動遊戲邏輯進入下一天
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterAgentAction();
        }

        // 4. 短暫停頓後檢查勝負
        yield return new WaitForSeconds(0.5f);
        CheckEpisodeEnd(agentData);

        isProcessing = false; // 解鎖，允許進行下一次 Decision
    }

    private string GetActionName(int act)
    {
        return act switch
        {
            0 => "休息 (Rest)",
            1 => "交易 (Trade)",
            2 => "生育政策 (Pop Policy)",
            3 => "軍事政策 (Mil Policy)",
            4 => "打贏佔領 (Occupy)",
            5 => "打贏不佔領 (Battle)",
            _ => "休息 (Rest)"
        };
    }

    private void ExecuteAction(int act, Country agentData, Country targetData)
    {
        //Debug.Log($"<color=green>[AI]</color> {country.CountryName}選擇了：<b>{actionName}</b> | 當前 AP: {agentData.AP}");
        switch (act)
        {
            case 0: // 不行動 (休息)
                    // 如果 AP 已經很多卻還不動，扣一點小分；AP 低時休息是好事
                    //AddReward(agentData.AP > 20 ? -0.01f : 0.005f);

                break;

            case 1: // 交易 (消耗 1 AP)
                    // 定義安全線：支撐 5 天的食量
                int dailyConsume = agentData.Population / 20;
                int safetyLine = dailyConsume * 5;

                bool isHungry = agentData.Food < safetyLine;
                bool isOverstock = agentData.Food > (safetyLine * 3); // 囤積超過 15 天份量

                int foodBefore = agentData.Food;
                tradeSystem.DailyTrade(country);
                int foodAfter = agentData.Food;

                if (foodAfter > foodBefore)
                {
                    // 情況 A：真的很餓的時候買
                    if (isHungry)
                    {
                        AddReward(2.0f); // 生存獎勵
                        Debug.Log($"<color=green>[AI決策]</color> 緊急補給成功");
                    }
                    // 情況 B：資源已經太多了還買
                    else if (isOverstock)
                    {
                        //AddReward(-0.05f); // 微量扣分，教它不要浪費 AP 囤貨
                        Debug.Log($"<color=orange>[AI決策]</color> 資源過剩仍進行交易，小幅扣分");
                    }
                    else
                    {
                        //AddReward(0.1f); // 正常的戰略備貨
                    }
                }
                else
                {
                    //AddReward(-0.1f); // 嘗試交易但失敗（可能沒錢或市場沒貨）
                }
                agentData.AP -= 1;

                break;

            /*case 2: // 民心
                if (agentData.AP >= 1)
                {
                    agentData.morale.ModifyMorale(10); // 效果可以設顯著一點
                    agentData.AP -= 1;

                    // 獎勵邏輯：民心越低，修復它的獎勵越高
                    float moraleReward = (100 - agentData.morale.MoraleValue) / 100f * 0.5f;
                    AddReward(0.1f + moraleReward);
                    Debug.Log($"<color=yellow>[AI決策]</color> {country.CountryName} 正在安撫民眾，民心回升。");
                }
                break;*/

            case 2: // 生育政策
                if (agentData.AP >= 1)
                {
                    int increasedPop = policySystem.ApplyPopulationPolicy(agentData);
                    //Debug.Log($"📈 [政策日誌] {country.CountryName} 執行生育政策，人口變動: +{increasedPop}");
                    // 只有在食物充足（例如 > 1000）時生小孩才給分
                    if (agentData.Population < 5000 && agentData.Food > 1000)
                    {
                        //AddReward(0.05f); // 從 0.5 降到 0.05
                    }
                    else
                    {
                        //AddReward(-0.2f); // 食物不夠還生，重罰
                    }
                    agentData.AP--;
                }
                //else AddReward(-0.01f);

                break;

            case 3: // 軍事政策
                if (agentData.AP >= 1)
                {
                    int popChange = policySystem.ApplyMilitaryPolicy(agentData);
                    //Debug.Log($"⚔️ [政策日誌] {country.CountryName} 執行軍事政策，人口變動: {popChange}");
                    if (agentData.MilPower < targetData.MilPower) //AddReward(0.7f);
                                                                  //else AddReward(0.1f);
                        agentData.AP--;
                }
                //else AddReward(-0.01f);

                break;

            case 4: // 打贏 & 佔領
                if (agentData.AP >= 5)
                {
                    var result = battleSystem.DoBattle(country, target);
                    agentData.AP -= 5;
                    if (result != null && result.AttackerWon)
                    {
                        AddReward(5.0f);
                        occupationSystem.Occupy(country, target, true);
                    }
                    //else AddReward(-0.1f);
                }
                //else AddReward(-0.5f);

                break;

            case 5: // 打贏 & 不佔領
                if (agentData.AP >= 3)
                {
                    var result = battleSystem.DoBattle(country, target);
                    agentData.AP -= 3;
                    if (result != null && result.AttackerWon)
                    {
                        AddReward(2.5f);
                        occupationSystem.Occupy(country, target, false);
                    }
                    //else AddReward(-0.1f);
                }
                //else AddReward(-0.5f);

                break;
        }
    }
    
    private void CheckEpisodeEnd(Country agentData)
    {
        Country targetData = target.resource.countries.Find(x => x.CountryName == target.CountryName);
        var stats = Academy.Instance.StatsRecorder; // 取得紀錄器

        // [新增] 基礎生存檢查
        if (agentData.Food <= 0 || agentData.Iron <= 0 || agentData.Wood <= 0)
        {
            //AddReward(-0.01f); // 輕微扣分，讓它感到不舒服
        }

        // 判定 A: 自己滅亡 (失敗) -> ELO 下降
        if (agentData.morale.MoraleValue <= 0 || agentData.City <= 0)
        {
            stats.Add("Custom/WinRate", 0);
            Debug.Log($"{country.CountryName} 滅亡。");
            SetReward(-100.0f); // 統一勝負權重，建議改為 -10 ~ -100
            if (logger != null) logger.LogGameResult(country.CountryName, "LOSS", StepCount, -100.0f); // 【新增日誌】
            EndEpisode();
            return;
        }

        // 判定 B: 對手滅亡 (勝利) -> ELO 上升
        if (targetData != null && (targetData.morale.MoraleValue <= 0 || targetData.City <= 0))
        {
            stats.Add("Custom/WinRate", 1);
            // 基礎分 100，步數越少額外加越多 (假設 MaxStep 是 300)
            float speedRatio = Mathf.Clamp01((300f - StepCount) / 300f);
            float speedBonus = speedRatio * 30f;
            SetReward(70.0f + speedBonus);
            if (logger != null) logger.LogGameResult(country.CountryName, "WIN", StepCount, 100.0f + speedBonus); // 【新增日誌】
            Debug.Log($"勝利！步數：{StepCount}，獲得加速獎勵：{speedBonus}");
            EndEpisode();
            return;
        }
        /*if (StepCount >= 300)
        {
            float myScore = agentData.Population + (agentData.MilPower * 10) + agentData.City * 10000 + agentData.Food * 5 + agentData.Iron * 2 + agentData.Wood * 3; // 這裡自定義你的「國力」公式
            float targetScore = targetData.Population + (targetData.MilPower * 10) + targetData.City * 10000 + targetData.Food * 5 + targetData.Iron * 2 + targetData.Wood * 3;

            float reward;
            string resultLabel;

            if (myScore > targetScore * 1.1f)
            {
                stats.Add("Custom/WinRate", 1);
                reward = 10.0f;
                resultLabel = "TIMEOUT_WIN";
            }
            else
            {
                stats.Add("Custom/WinRate", 0);
                reward = -20.0f;
                resultLabel = "TIMEOUT_LOSS";
            }

            // 【新增日誌】
            if (logger != null) logger.LogGameResult(country.CountryName, resultLabel, StepCount, reward);

            SetReward(reward);
            EndEpisode();
        }*/

        // [核心新增] 判定 C: 強制超時結算 (驅動 Self-Play)
        // 當 StepCount 達到 Behavior Parameters 設定的 Max Step 時 (假設設為 5000)
        /*if (StepCount >= MaxStep && MaxStep > 0)
        {
            /*float myScore = agentData.Population + (agentData.MilPower * 10) + agentData.City * 10000 + agentData.Food * 5 + agentData.Iron * 2 + agentData.Wood * 3; // 這裡自定義你的「國力」公式
            float targetScore = targetData.Population + (targetData.MilPower * 10) + targetData.City * 10000 + targetData.Food * 5 + targetData.Iron * 2 + targetData.Wood * 3;

            if (myScore > targetScore)
            {
                AddReward(1.0f); // 點數領先勝
                Debug.Log("<color=cyan>[超時結算]</color> 我方國力領先，判定微勝");
            }
            else
            {
                AddReward(-1.0f); // 點數落後敗
                Debug.Log("<color=red>[超時結算]</color> 我方國力落後，判定微敗");
            }

            EndEpisode(); // 必須手動呼叫，確保數據寫入 TensorBoard
        }*/
    }
    public override void WriteDiscreteActionMask(IDiscreteActionMask maskCollector)
    {
        Country agentData = country.resource.countries.Find(x => x.CountryName == country.CountryName);
        if (agentData == null) return;
        bool popReady = policySystem.IsPolicyReady(country.CountryName, "Pop") && agentData.AP >= 1;
        bool milReady = policySystem.IsPolicyReady(country.CountryName, "Mil") && agentData.AP >= 1;
        // 基礎索引說明：0:休息, 1:交易, 3:戰鬥, 4:生育, 5:軍事, 6:佔領 (根據你的 ExecuteAction)
        // 1. AP 不足的遮罩
        maskCollector.SetActionEnabled(0, 1, agentData.AP >= 1); // 交易需 1 AP
        maskCollector.SetActionEnabled(0, 2, agentData.AP >= 1 && popReady); // 2: 生育政策 (1 AP + Ready)
        maskCollector.SetActionEnabled(0, 3, agentData.AP >= 1 && milReady); // 3: 軍事政策 (1 AP + Ready)
        maskCollector.SetActionEnabled(0, 4, agentData.AP >= 5); // 佔領需 5 AP
        maskCollector.SetActionEnabled(0, 5, agentData.AP >= 3); // 不佔領需 3 AP

    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // 預設不行動
    }
}