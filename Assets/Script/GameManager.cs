using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public CountryStateManager game; // 指向資源系統的主管理中心
        //private int agentsActedCount = 0;
        //private const int TotalAgents = 1; //設定對戰Agent國家數量
        private const int TotalAgents = 2; //設定對戰Agent國家數量
        [Header("回合狀態")]
        public bool isPlayerTurn = true;
        private List<RuleBasedCountry> rbcCountries = new List<RuleBasedCountry>();
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (game != null && game.resource != null)
            {
                foreach (var country in game.resource.countries)
                {
                    game.trust.InitializeTrusts(game.resource.countries, country.CountryName);
                }
            }
        rbcCountries = FindObjectsByType<RuleBasedCountry>(FindObjectsSortMode.None).ToList();

        // 遊戲開始：從玩家回合開始
        StartPlayerTurn();
    }
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        var pc = FindFirstObjectByType<CountryAController>();
        /*if (pc != null)
        {
            pc.StartNewDay(); // 內部會 SetActive(true) 顯示面板
            // 🚩 如果是 AI 模式，由 GameManager 主動要求它動「一次」
            if (!pc.isPlayerControl)
            {
                Debug.Log("<color=cyan>[系統]</color> A國目前為 AI 模式，執行自動決策...");
                pc.aiAgent.RequestDecision();
                // 確保 aiAgent 動完後會回傳訊號給 RegisterAgentAction
            }
        }*/
        LogManager.Instance.EnqueueActionLog("系統", "新的一天開始：現在是玩家回合", true);
    }

    // 2. 玩家動完後，由 PlayerCountryController 呼叫此處
    public void StartAICountryTurns()
    {
        LogManager.Instance.canPrint = true;
        isPlayerTurn = false;
        LogManager.Instance.EnqueueActionLog("系統", "玩家結束行動，開始 AI 國家順序行動...", true);
        StartCoroutine(ExecuteAISequentially());
    }

    // 3. 協程：讓 AI 國家一個接一個動，增加遊戲感與穩定性
    private IEnumerator ExecuteAISequentially()
    {
        // 遍歷所有由 RBC 控制的國家 (B-F)
        foreach (var rbc in rbcCountries)
        {
            Debug.Log($"<color=white>[回合]</color> 國家 {rbc.countryState.CountryName} 正在思考...");

            // 執行該國家的 RBC 邏輯 (這會觸發 LogManager.EnqueueLog)
            rbc.TakeTurn(rbc.countryState, rbc.target);

            // 🚩 等待 LogManager 把這一個國家產生的所有 Log 噴完
            // 這樣玩家才能看清該國具體做了什麼
            yield return new WaitUntil(() => !LogManager.Instance.IsBusy);

            // 額外停頓一下，讓節奏更舒服
            yield return new WaitForSeconds(1.0f);
        }

        // 4. 所有 AI 動完後，執行全球結算
        ProcessEndOfDay();
    }

    // 4. 每日結算邏輯
    private void ProcessEndOfDay()
    {
        Debug.Log("-------------------- 執行每日結算 --------------------");

        // 統一更新所有國家狀態 (資源消耗、人口增長、AP 恢復)
        CountryStateManager[] allStates = FindObjectsByType<CountryStateManager>(FindObjectsSortMode.None);
        foreach (var state in allStates)
        {
            state.DailyUpdate();
            if (state.policy != null)
            {
                state.policy.UpdateCooldowns(state.CountryName);
            }
        }

        LogWorldStatus();

        // 重新回到玩家回合
        StartPlayerTurn();
    }

    // --- [原有的工具 Function] ---

    private void LogWorldStatus()
    {
        // 這裡可以根據你現有的國家 A-F 擴展 Log
        Country a = game.resource.countries.Find(c => c.CountryName == "Country A");
        if (a != null)
        {
            Debug.Log($"📊 [昨日統計] {a.CountryName} | 人口: {a.Population} | 食物: {a.Food} | AP 已恢復");
        }
        Country b = game.resource.countries.Find(c => c.CountryName == "Country B");
        if (b != null)
        {
            Debug.Log($"📊 [昨日統計] {b.CountryName} | 人口: {b.Population} | 食物: {b.Food} | AP 已恢復");
        }
    }

    public void RegisterAgentAction()
    {
        var pc = FindFirstObjectByType<CountryAController>();

        if (pc != null && pc.mode == "AI 自動控制")
        {
            LogManager.Instance.EnqueueActionLog("系統", "A 國行動結束，開始各國 RBC 決策。", true);
            StartAICountryTurns();
        }
    }
}