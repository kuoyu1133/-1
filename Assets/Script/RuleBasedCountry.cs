using System.Collections;
using UnityEngine;

public class RuleBasedCountry : MonoBehaviour
{
    public CountryStateManager countryState;
    public CountryStateManager target;
    public TradeSystem tradeSystem;
    public AnnouncementSystem announcementSystem;
    public BattleSystem battleSystem;
    public PolicySystem policySystem;
    public TrustSystem trustSystem;
    public OccupationSystem occupationSystem;

    /// <summary>
    /// 由 GameManager 呼叫。確保 AI 在 AP 耗盡前持續決策。
    /// </summary>
    public void TakeTurn(CountryStateManager self, CountryStateManager opponent)
    {
        Country selfData = self.resource.countries.Find(c => c.CountryName == self.CountryName);
        if (selfData == null) return;

        Debug.Log($"<color=white>[RBC]</color> {self.CountryName} 開始行動，當前 AP: {selfData.AP}");

        // 使用 while 確保 AI 會用完所有想用的 AP，而不是每回合只做一件事
        int safetyNet = 0; // 防止無限迴圈
        while (selfData.AP > 0 && safetyNet < 10)
        {
            safetyNet++;
            if (!DecideAndExecuteAction(selfData, opponent))
                break; // 如果沒有合適動作可做，跳出循環（休息）
        }
    }

    private bool DecideAndExecuteAction(Country selfData, CountryStateManager opponent)
    {
        Country opponentData = opponent.resource.countries.Find(c => c.CountryName == opponent.CountryName);

        // 1. 優先處理生存資源 (交易) - 消耗 1 AP
        string[] resources = { "Food", "Iron", "Wood" };
        foreach (string res in resources)
        {
            int deficit = tradeSystem.GetResourceNeed(countryState, res);
            // 如果缺資源，或者食物低於安全線 (1天份量)
            if (deficit > 0 || (res == "Food" && selfData.Food < (selfData.Population / 20 * 1)))
            {
                tradeSystem.DailyTrade(countryState);
                selfData.AP--;
                LogManager.Instance.EnqueueActionLog(selfData.CountryName, $"購買了 {res}", true);
                return true;
            }
        }

        // 2. 處理外交支援 (響應公告系統) - 消耗 1 AP
        // 這裡修改邏輯：AI 不主動觸發天災，而是檢查是否有未處理的事件需要支援
        if (selfData.AP >= 1 && selfData.Food > selfData.Population * 0.5f)
        {
            if (announcementSystem.isDisaster || announcementSystem.isDisease)
            {
                // 這裡假設您的 AnnouncementSystem 有對應的 Response 函式
                // 簡單示範：消耗資源幫助 target
                LogManager.Instance.EnqueueActionLog(selfData.CountryName, "響應了國際救援並提供物資支援", true);
                selfData.AP--;
                return true;
            }
        }

        // 3. 發展政策 - 消耗 1 AP (檢查 PolicySystem 冷卻)
        if (selfData.AP >= 1)
        {
            // 優先考慮軍事，如果軍力落後
            if (selfData.MilPower < opponentData.MilPower + 20 && policySystem.IsPolicyReady(selfData.CountryName, "Mil"))
            {
                policySystem.ApplyMilitaryPolicy(selfData);
                selfData.AP--;
                LogManager.Instance.EnqueueActionLog(selfData.CountryName, "執行軍事擴張政策", true);
                return true;
            }
            // 否則考慮人口
            else if (selfData.Food > 1500 && policySystem.IsPolicyReady(selfData.CountryName, "Pop"))
            {
                policySystem.ApplyPopulationPolicy(selfData);
                selfData.AP--;
                LogManager.Instance.EnqueueActionLog(selfData.CountryName, "執行人口增長政策", true);
                return true;
            }
        }

        // 4. 戰鬥 - 消耗 3 AP
        if (selfData.AP >= 3)
        {
            float myPower = selfData.MilPower * (1.0f + countryState.morale.MoraleValue / 100f);
            float opPower = opponentData.MilPower * (1.1f + opponent.morale.MoraleValue / 100f);

            if (myPower > opPower * 1.25f && trustSystem.GetTrust(opponent.CountryName) < 40)
            {
                LogManager.Instance.EnqueueActionLog(selfData.CountryName, "發動了軍事攻勢！", true);
                battleSystem.DoBattle(countryState, opponent);
                selfData.AP -= 3;
                return true;
            }
        }

        // 5. 佔領 - 消耗 2 AP
        if (selfData.AP >= 2 && (opponentData.Population < 2000 || opponentData.morale.MoraleValue < 20))
        {
            occupationSystem.Occupy(countryState, opponent, true);
            selfData.AP -= 2;
            LogManager.Instance.EnqueueActionLog(selfData.CountryName, $"完成了對 {opponent.CountryName} 的佔領", true);
            return true;
        }

        return false; // 沒有適合的動作了
    }
}