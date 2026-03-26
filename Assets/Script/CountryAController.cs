using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CountryAController: MonoBehaviour
{
    [Header("基礎組件")]
    public CountryStateManager countryA;
    public CountryAIAgent aiAgent;

    [Header("系統引用")]
    public TradeSystem tradeSystem;
    public BattleSystem battleSystem;
    public PolicySystem policySystem;
    public AnnouncementSystem announcement;

    [Header("UI 介面")]
    public GameObject mainActionPanel; // 主操作介面
    public Button popPolicyBtn;
    public Button milPolicyBtn;
    //public GameObject actionDetailPanel; // 子視窗 (交易/戰鬥詳情)
    //public GameObject toolTipPanel; // 行動點提示小窗

    public string mode;
    public int cnt = 0;
    private bool isWaitingForOtherCountries = false;
    private Country data => countryA.resource.countries.Find(c => c.CountryName == countryA.CountryName);

    /*void Start()
    {
        UpdateUI();
    }*/

    // 由 ControlSwitcher 調用
    public void ToggleControlMode()
    {
        if (cnt % 2 == 0) mode = "AI 自動控制";
        else mode = "玩家控制";
        cnt++;
        Debug.Log($"<color=yellow>[系統]</color> 已切換至：{mode}");
    }

    void Update()
    {
        if (mode == "AI 自動控制" || isWaitingForOtherCountries) return;

        //UpdateUI();

        // 自動檢查：若 AP 歸零，強制進入下一天
        if (data.AP <= 0)
        {
            EndPlayerTurn();
        }
    }

    /*private void UpdateUI()
    {
        //apText.text = $"AP: {data.AP}";

        // 政策冷卻顏色檢查
        popPolicyBtn.image.color = policySystem.IsPolicyReady(countryA.CountryName, "Pop") ? Color.white : Color.red;
        milPolicyBtn.image.color = policySystem.IsPolicyReady(countryA.CountryName, "Mil") ? Color.white : Color.red;
    }*/

    #region 玩家動作指令

    public void OnRestClick()
    {
        LogManager.Instance.EnqueueActionLog(data.CountryName, "休息", false);
        EndPlayerTurn();
    }

    public void OnPopPolicyClick()
    {
        if (data.AP >= 1 && policySystem.IsPolicyReady(countryA.CountryName, "Pop"))
        {
            policySystem.ApplyPopulationPolicy(data);
            data.AP--;
            LogManager.Instance.EnqueueActionLog(data.CountryName, "執行了生育政策", false);
        }
        else if (data.AP < 1)
        {
            Debug.Log("行動點不足");
            //EndPlayerTurn();
        }
        else
        {
            Debug.Log("政策冷卻中");
            //EndPlayerTurn();
        }
    }

    public void OnMilPolicyClick()
    {
        if (data.AP >= 1 && policySystem.IsPolicyReady(countryA.CountryName, "Mil"))
        {
            policySystem.ApplyMilitaryPolicy(data);
            data.AP--;
            LogManager.Instance.EnqueueActionLog(countryA.CountryName, "強化了軍事力量！", false);
        }
        else if (data.AP < 1)
        {
            Debug.Log("行動點不足");
            //EndPlayerTurn();
        }
        else
        {
            Debug.Log("政策冷卻中");
            //EndPlayerTurn();
        }
    }

    public void HandleIncomingTradeRequest(TradeSystem.TradeRequest request)
    {
        if (mode == "玩家控制")
        {
            Debug.Log($"<color=cyan>[交易]</color> 收到來自 {request.requester.CountryName} 的 {request.resourceType} 請求 (數量: {request.amount})。玩家模式下默認接受。");

            // 設定請求為已接受
            request.accepted = true;

            // 執行 TradeSystem 中的結算邏輯
            tradeSystem.ExecuteTrade(request);
        }
    }

    // 結束回合：通知 GameManager 輪到其他 AI
    private void EndPlayerTurn()
    {
        isWaitingForOtherCountries = true;
        //mainActionPanel.SetActive(false);

        Debug.Log("<color=green>[回合]</color> 玩家動作結束，等待 AI 執行...");
        if (GameManager.Instance != null)
        {
            // 這裡假設 GameManager 有一個處理回合切換的函式
            GameManager.Instance.StartAICountryTurns();
        }
    }

    // 當所有 AI 動完後，由 GameManager 呼叫此函式
    public void StartNewDay()
    {
        isWaitingForOtherCountries = false;
        if (mode == "玩家控制") mainActionPanel.SetActive(true);
    }
    #endregion

    #region 行動點提示
    //public void OnAPPointerEnter() { toolTipPanel.SetActive(true); }
    //public void OnAPPointerExit() { toolTipPanel.SetActive(false); }
    #endregion
}