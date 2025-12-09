using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

// AI 國家 ML Agent
public class CountryAIAgent : Agent
{
    /*  Trade Announce Battle Policy Occupy
    收集觀察（Observation） → 把國家狀況提供給模型
    接收動作（Action） → 模型告訴你該做什麼
    執行系統（Battle / Trade / Occupy ...）
    設計 Reward（獎勵） → 讓 AI 學變強    */
    public CountryStateManager country;
    public CountryStateManager target;

    public TradeSystem tradeSystem;
    public AnnouncementSystem announcement;
    public BattleSystem battleSystem;
    public PolicySystem policySystem;

    public DecisionRequester decisionRequester;

    public void Start()
    {
        //decisionRequester.DecisionPeriod = 1; // 每回合都做決策
    }
    public override void Initialize()
    {
        // 可以使用你的 CountryStateManager 做初始化
        Debug.Log("Agent Initialize() called: " + gameObject.name);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 加入你最重要的遊戲指標
        Country c = country.resource.countries.Find(c => c.CountryName == country.CountryName);
        Country t = target.resource.countries.Find(c => c.CountryName == target.CountryName);
        
        /*
         Food -> AI會觀察食物不足 → 選 Trade、Policy（提升人口）、或避免Occupy
         Iron -> 鐵不足可能導致 Military 生產下降，AI可避免戰鬥
         Wood -> Trade / Policy
         MilPower -> 決定是否能安全發動戰爭或佔領
         Morale -> 士氣在你的戰鬥計算中直接影響戰力 → AI必須知道自身士氣值
         Population -> Policy 決定是否採用人口政策，例如增加人口、避免人口過低、影響產能（未來可擴展）
         AP -> AP 不足時不能行動，因此 AI 必須知道剩餘 AP 來規劃行為
         Trust -> AI可以透過信任值判斷是否要：攻擊（低信任 → 易開戰）宣布災害/疾病（支援對手獲得信任）
         敵國Food/Iron/Wood -> 了解敵國資源狀況，決定是否發動戰爭或佔領(獲取資源)
         敵國MilPower/Morale -> 預測攻擊勝率
         */
        // 資源
        sensor.AddObservation(c.Food);
        sensor.AddObservation(c.Iron);
        sensor.AddObservation(c.Wood);

        // 軍力與民心
        sensor.AddObservation(c.MilPower);
        sensor.AddObservation(c.morale.MoraleValue);

        // 經濟
        sensor.AddObservation(c.Population);

        // 行動點 AP
        sensor.AddObservation(c.AP);

        // 信任值
        sensor.AddObservation(country.trust.GetTrust(target.CountryName));

        // 對手國狀態
        sensor.AddObservation(t.Food);
        sensor.AddObservation(t.Iron);
        sensor.AddObservation(t.Wood);
        sensor.AddObservation(t.MilPower);
        sensor.AddObservation(t.morale.MoraleValue);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int act = actions.DiscreteActions[0];
        Country c = country.resource.countries.Find(c => c.CountryName == country.CountryName);
        int tempAP = c.AP;

        switch (act)
        {
            case 0:
                // 不行動
                AddReward(-1);
                break;

            case 1:
                // Trade
                if (tempAP > 0)
                {
                    int foodNeed = tradeSystem.GetResourceNeed(country, "Food");
                    if (foodNeed > 0)
                    {
                        tradeSystem.FindProvider(country, "Food");
                        AddReward(5);
                    }
                    tempAP--;
                }
                break;

            case 2:
                // Announce
                if (tempAP > 0 && (announcement.isDisaster || announcement.isDisease || announcement.isWar))
                {
                    if (announcement.isDisaster)
                    {
                        announcement.HandleDisaster(target, country);
                        
                    } 
                    else if (announcement.isDisease)
                    {
                        announcement.HandleDisease(target, country);
                        AddReward(-3);
                    }
                    /*else if (announcement.isWar)
                        announcement.HandleWar(target, country);*/

                    tempAP--;
                }
                break;

            case 3:
                // Battle
                if (tempAP >= 3)
                {
                    var result = battleSystem.DoBattle(country, target);
                    if (result != null && result.AttackerWon)
                        AddReward(10);
                    else
                        AddReward(-10);

                    tempAP -= 3;
                }
                break;

            case 4:
                // Policy - AI 選生育政策
                if (tempAP > 0)
                {
                    policySystem.ApplyPopulationPolicy(c);
                    AddReward(3);
                    tempAP--;
                }
                break;

            case 5:
                // Policy - AI 選軍事政策
                if (tempAP > 0)
                {
                    policySystem.ApplyMilitaryPolicy(c);
                    AddReward(3);
                    tempAP--;
                }
                break;

            case 6:
                // Occupy
                if (tempAP >= 2)
                {
                    var result = battleSystem.DoBattle(country, target);
                    if (result != null && result.AttackerWon)
                    {
                        AddReward(10);
                        // 給資源獎勵
                        Country attackerCountry = country.resource.countries.Find(x => x.CountryName == country.CountryName);
                        Country defenderCountry = target.resource.countries.Find(x => x.CountryName == target.CountryName);

                        attackerCountry.Food += Mathf.RoundToInt(defenderCountry.Food * 0.5f);
                        attackerCountry.Iron += Mathf.RoundToInt(defenderCountry.Iron * 0.5f);
                        attackerCountry.Wood += Mathf.RoundToInt(defenderCountry.Wood * 0.5f);
                    }
                    else
                    {
                        AddReward(-10);
                    }

                    tempAP -= 2;
                }
                break;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 測試用手動控制
        actionsOut.DiscreteActions.Array[0] = 0;
    }
}