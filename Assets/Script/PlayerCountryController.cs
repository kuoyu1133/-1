using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerCountryController : MonoBehaviour
{
    [Header("基礎組件")]
    public CountryStateManager selfState;
    public CountryAIAgent backupAI;
    public bool isAutoMode = false;

    [Header("UI 面板引用")]
    public GameObject mainActionPanel; // 建議拖入 PolicyGroup
    public GameObject birthPolicyPopup; // 執行政策後的確認彈窗
    public GameObject milPolicyPopup;

    /*[Header("數據顯示")]
    public Text apText; // 如果有 UI 顯示 AP 可以拖入*/

    // 獲取當前國家數據的快捷屬性
    private Country SelfData => selfState.resource.countries.Find(c => c.CountryName == selfState.CountryName);

    /*void Update()
    {
        // 即時更新 AP 文字顯示（選擇性）
        if (apText != null && SelfData != null)
        {
            apText.text = $"AP: {SelfData.AP}";
        }
    }*/

    // 當輪到該國家行動時由 GameManager 呼叫
    public void OnPlayerTurnStart()
    {
        // 確保 AI 腳本狀態與模式同步
        if (backupAI != null) backupAI.enabled = isAutoMode;

        if (isAutoMode)
        {
            Debug.Log("<color=cyan>[系統]</color> AI 模式：自動決策中...");
            backupAI.RequestDecision();
        }
        else
        {
            if (mainActionPanel != null) mainActionPanel.SetActive(true);
            Debug.Log("<color=green>[系統]</color> 玩家模式：等待操作。");
        }
    }

    // --- 核心政策功能 ---

    // 1. 生育政策：扣除 AP 並呼叫 PolicySystem
    public void TriggerPopPolicy()
    {
        // 檢查 AP 是否足夠 (假設消耗 1) 且 政策是否冷卻結束
        if (SelfData.AP >= 1 && selfState.policy.IsPolicyReady(selfState.CountryName, "Pop"))
        {
            UseAP(1);
            // 呼叫 PolicySystem 內有的對應功能
            selfState.policy.ApplyPopulationPolicy(SelfData);

            Debug.Log($"<color=white>[玩家行動]</color> {selfState.CountryName} 執行了生育政策。");

            // 執行完後自動關閉彈窗
            if (birthPolicyPopup != null) birthPolicyPopup.SetActive(false);
        }
        else
        {
            Debug.LogWarning("AP 不足或政策冷卻中！");
        }
    }

    // 2. 軍事政策：扣除 AP 並呼叫 PolicySystem
    public void TriggerMilPolicy()
    {
        if (SelfData.AP >= 1 && selfState.policy.IsPolicyReady(selfState.CountryName, "Mil"))
        {
            UseAP(1);
            // 呼叫 PolicySystem 內有的對應功能
            selfState.policy.ApplyMilitaryPolicy(SelfData);

            Debug.Log($"<color=white>[玩家行動]</color> {selfState.CountryName} 執行了軍事政策。");

            // 執行完後自動關閉彈窗
            if (milPolicyPopup != null) milPolicyPopup.SetActive(false);
        }
    }

    // --- 通用邏輯 ---

    public void UseAP(int amount)
    {
        if (SelfData == null) return;
        SelfData.AP -= amount;

        // 如果 AP 用完，自動結束回合
        if (SelfData.AP <= 0)
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        if (mainActionPanel != null) mainActionPanel.SetActive(false);

        // 呼叫 GameManager 驅動下一天
        /*if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerActionFinished();
        }*/
    }

    public void ToggleControlMode()
    {
        isAutoMode = !isAutoMode;

        // 物理關閉/開啟 AI 腳本以防止玩家模式下 AI 偷動
        if (backupAI != null)
        {
            backupAI.enabled = isAutoMode;
            // 停掉 ML-Agents 的決策組件
            var req = backupAI.GetComponent<Unity.MLAgents.DecisionRequester>();
            if (req != null) req.enabled = isAutoMode;
        }

        if (isAutoMode) EndTurn();
        else if (mainActionPanel != null) mainActionPanel.SetActive(true);
    }
}