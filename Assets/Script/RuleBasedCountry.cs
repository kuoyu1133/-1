using Unity.Properties;
using System.Collections;
using UnityEngine;
//4國固定邏輯控制
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

    private int foodNeed;
    private int ironNeed;
    private int woodNeed;

    private int populationPolicyTurnsLeft = 0;
    private int militaryPolicyTurnsLeft = 0;

    private void Trade(Country myCountry)
    {
        Debug.Log("[RuleBasedCountry] 進入 Trade()");
        //交易
        if (foodNeed > myCountry.Food && (myCountry.AP - 1) > 0)
        {
            tradeSystem.FindProvider(countryState, "Food");
            tradeSystem.DailyTrade(countryState);
            myCountry.AP -= 1;
        }
        if (ironNeed > myCountry.Iron && (myCountry.AP - 1) > 0)
        {
            tradeSystem.FindProvider(countryState, "Iron");
            tradeSystem.DailyTrade(countryState);
            myCountry.AP -= 1;
        }
        if (woodNeed > myCountry.Wood && (myCountry.AP - 1) > 0)
        {
            tradeSystem.FindProvider(countryState, "Wood");
            tradeSystem.DailyTrade(countryState);
            myCountry.AP -= 1;
        }
    }

    private void Announce(Country myCountry)
    {
        Debug.Log("[RuleBasedCountry] 進入 Announce()");
        //公告(支援)
        if ((myCountry.Food * 0.7) >= myCountry.Population && announcementSystem.isDisaster && (myCountry.AP - 1) > 0)
        {
            announcementSystem.HandleDisaster(target, countryState);
            Debug.Log($"{countryState.CountryName} 注意：發生天災，進行對應應對");
            myCountry.AP -= 1;
        }

        else if ((myCountry.Food * 0.85) >= myCountry.Population && announcementSystem.isDisease && (myCountry.AP - 1) > 0)
        {
            announcementSystem.HandleDisease(target, countryState);
            Debug.Log($"{countryState.CountryName} 注意：發生疾病，進行對應應對");
            myCountry.AP -= 1;
        }

        else if (myCountry.morale.MoraleValue > 30 && announcementSystem.isWar && (myCountry.AP - 1) > 0)
        {
            announcementSystem.HandleWar(target, countryState);
            Debug.Log($"{countryState.CountryName} 注意：發生戰爭，評估支援策略");
            myCountry.AP -= 1;
        }
    }

    private void Battle(Country myCountry)
    {
        Debug.Log("[RuleBasedCountry] 進入 Battle()");
        //戰鬥
        if (trustSystem.GetTrust(target.CountryName) < 30 && myCountry.MilPower > target.MilPower * 0.8f && (myCountry.AP - 3) != 0)
        {
            CountryStateManager defender = announcementSystem.player;
            Debug.Log($"countryState={countryState}, target={target}");
            Debug.Log($"countryState.name={(countryState ? countryState.name : "NULL")}");
            Debug.Log($"target.name={(target ? target.name : "NULL")}");
            var result = battleSystem.DoBattle(countryState, target);
            if (result != null)
            {
                if (result.AttackerWon)
                    Debug.Log($"{result.Winner.CountryName} 勝利，擊敗 {result.Loser.CountryName}");
                else
                    Debug.Log($"{result.Loser.CountryName} 防守成功，{result.Winner.CountryName} 失敗");
            }
            myCountry.AP -= 3;
        }
    }

    private void Policy(Country myCountry)
    {
        Debug.Log("[RuleBasedCountry] 進入 Policy()");
        //政策
        if (myCountry.Population < 8683 && myCountry.AP != 0 && populationPolicyTurnsLeft <= 0)
        {
            policySystem.ApplyPopulationPolicy(myCountry);
            populationPolicyTurnsLeft = 5;
            Debug.Log($"{myCountry.CountryName} 開始生育政策，持續5回合");
        }
        if (myCountry.MilPower < 100 && myCountry.morale.MoraleValue > 30 && myCountry.AP != 0)
        {
            policySystem.ApplyMilitaryPolicy(myCountry);
            militaryPolicyTurnsLeft = 5;
            Debug.Log($"{myCountry.CountryName} 開始軍事政策，持續5回合");
        }
    }

    private void Occupy(Country myCountry)
    {
        Debug.Log("[RuleBasedCountry] 進入 Occupy()");
        //佔領
        if (myCountry.AP != 0)
        {
            occupationSystem.Occupy(countryState, target, true);
            myCountry.AP -= 2;
            Debug.Log($"{countryState.CountryName} 佔領了 {target.CountryName} 的領土");
        }
    }

    private void UpdatePoliciesPerTurn(Country myCountry)
    {
        // 生育政策
        if (populationPolicyTurnsLeft > 0)
        {
            myCountry.AP = Mathf.Max(0, myCountry.AP - 1); // 扣 AP
            populationPolicyTurnsLeft--;
            Debug.Log($"{myCountry.CountryName} 生育政策回合倒數，剩餘回合 {populationPolicyTurnsLeft}");
        }

        // 軍事政策
        if (militaryPolicyTurnsLeft > 0)
        {
            myCountry.AP = Mathf.Max(0, myCountry.AP - 1);
            militaryPolicyTurnsLeft--;
            Debug.Log($"{myCountry.CountryName} 軍事政策回合倒數，剩餘回合 {militaryPolicyTurnsLeft}");
        }
    }

    /*void ExtremeBattleAI(Country myCountry) //極端戰鬥AI
    {
        while (myCountry.AP >= 3)
        {
            CountryStateManager defender = announcementSystem.player;
            battleSystem.DoBattle(countryState, defender);
            myCountry.AP -= 3;
        }

        if (myCountry.AP >= 2)
        {
            occupationSystem.Occupy(countryState, target, true);
            myCountry.AP -= 2;
        }
    }*/

    /*void ExtremeTradeAI(Country myCountry) //極端交易AI
    {
        while (myCountry.AP > 0)
        {
            if (foodNeed > myCountry.Food) { tradeSystem.FindProvider(countryState, "Food"); myCountry.AP--; }
            else if (ironNeed > myCountry.Iron) { tradeSystem.FindProvider(countryState, "Iron"); myCountry.AP--; }
            else if (woodNeed > myCountry.Wood) { tradeSystem.FindProvider(countryState, "Wood"); myCountry.AP--; }
            else break;
        }
    }*/

    /*void ExtremeAnnounceAI(Country myCountry) //極端公告AI
    {
        if (announcementSystem.isDisaster) { announcementSystem.HandleDisaster(target, countryState); myCountry.AP--; }
        if (announcementSystem.isDisease) { announcementSystem.HandleDisease(target, countryState); myCountry.AP--; }
        if (announcementSystem.isWar) { announcementSystem.HandleWar(target, countryState); myCountry.AP--; }
    }*/

    /*void ExtremePolicyAI(Country myCountry)  //極端政策AI
    {
        while (myCountry.AP > 0)
        {
            if (myCountry.Population < 10000) policySystem.ApplyPopulationPolicy(myCountry);
            if (myCountry.MilPower < 100) policySystem.ApplyMilitaryPolicy(myCountry);
            myCountry.AP--;
        }
    }*/

    void Start()
    {
        Debug.Log($"[RuleBasedCountry] {countryState.CountryName} 開始自動行動邏輯");
        if (countryState == null) Debug.LogError("countryState 未設定！");
        if (target == null) Debug.LogError("target 未設定！");
        if (tradeSystem == null) Debug.LogError("tradeSystem 未設定！");
        if (countryState == null || countryState.resource == null)
        {
            Debug.LogError("countryState 或 resource 未設置");
            return;
        }

        foodNeed = tradeSystem.GetResourceNeed(countryState, "Food");
        ironNeed = tradeSystem.GetResourceNeed(countryState, "Iron");
        woodNeed = tradeSystem.GetResourceNeed(countryState, "Wood");

        Country myCountry = countryState.resource.countries.Find(c => c.CountryName == countryState.CountryName);
        StartCoroutine(RunDailyRoutine());
    }
    private IEnumerator RunDailyRoutine()
    {
        while (!countryState.morale.IsDefeated && !target.morale.IsDefeated)
        {
            Country myCountry = countryState.resource.countries.Find(c => c.CountryName == countryState.CountryName);
            int temp = myCountry.AP;

            // 行動順序
            Trade(myCountry);
            yield return null; // 等一個 frame，避免卡住

            Announce(myCountry);
            yield return null;

            Policy(myCountry);
            yield return null;

            Battle(myCountry);
            yield return null;

            Occupy(myCountry);
            yield return null;

            // 下一回合資源與士氣更新
            countryState.DailyUpdate();
            target.DailyUpdate();

            myCountry.AP = temp; //恢復行動值
            UpdatePoliciesPerTurn(myCountry);

            yield return new WaitForSeconds(0.5f); // 暫停 0.5 秒再下一回合（可調整）
        }
    }
}
