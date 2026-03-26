using UnityEngine;
//國家狀態管理
//!!!!!!!!havent done yet!!!!!!!!!!
public class CountryStateManager : MonoBehaviour
{
    public string CountryName;


    public AnnouncementSystem announcement;
    public TrustSystem trust;
    public MoraleSystem morale;
    public PolicySystem policy;
    public ResourceSystem resource;
    public OccupationSystem occupation;
    public MoraleSystem moraleSystem;

    public int Population;
    public float MilPower;
    public int Defense;


    public int Iron;
    public int Food;
    public int Wood;
    public float PopulationGrowthRate;//人口成長率

    public void DailyUpdate()
    {

        Country selfData = resource.countries.Find(c => c.CountryName == CountryName);

        if (selfData == null) return;

        resource.UpdateDay(selfData);

        if (selfData.DailyFoodProd < 500)
        {
            selfData.DailyFoodProd += 50;
            if (selfData.DailyFoodProd > 500) selfData.DailyFoodProd = 500;
        }

        if (selfData.Iron < 0)
            selfData.morale.ModifyMorale(-1);
        else
            selfData.morale.ModifyMorale(2);

        if (selfData.Wood < 0)
            selfData.morale.ModifyMorale(-2);
        else
            selfData.morale.ModifyMorale(4);

        if (selfData.Food < 0)
            selfData.morale.ModifyMorale(-3);
        else
            selfData.morale.ModifyMorale(5);
        if (selfData.Population < 0)    
            selfData.morale.SetMorale(0);
        if (policy != null) 
            policy.UpdateCooldowns(selfData.CountryName);
        CheckRandomEvents(selfData);
        if (selfData.morale.CheckDefeated(selfData))
        {
            Debug.Log($"{CountryName} 已滅亡（因民心歸零）！");
        }
        if (selfData.City == 0)
        {
            Debug.Log($"{CountryName} 已滅亡（因城市均被佔領）！");
        }
    }
    public CountryStateManager GetCountryByName(string name)
    {
        CountryStateManager[] allManagers = FindObjectsByType<CountryStateManager>(FindObjectsSortMode.None);
        foreach (var manager in allManagers)
        {
            if (manager.CountryName == name)
            {
                return manager;
            }
        }
        return null;
    }
    public void CheckRandomEvents(Country targetCountry)
    {
        float eventRoll = Random.value;

        if (eventRoll < 0.05f) // 5% 機率天災
        {
            announcement.isDisaster = true;
            announcement.HandleDisaster(this, announcement.player);
            announcement.isDisaster = false;
        }
        else if (eventRoll < 0.1f) // 另外 5% 機率疾病
        {
            announcement.isDisease = true;
            announcement.HandleDisease(this, announcement.player);
            announcement.isDisease = false;
        }
    }
    public bool ScouterPopulationDecision(CountryStateManager target)
    {
        Country selfData = resource.countries.Find(c => c.CountryName == CountryName);
        if (selfData.AP >= 1)
        {
            selfData.AP -= 1;
            Country targetData = target.resource.countries.Find(c => c.CountryName == target.CountryName);

            bool shouldConsiderWar = selfData.Population > targetData.Population;
            Debug.Log($"<color=white>[偵察報告]</color> {CountryName} 偵察了 {target.CountryName} 的人口。");
            Debug.Log($"結果：己方 {selfData.Population} vs 目標 {targetData.Population}。建議考慮戰爭：{shouldConsiderWar}");

            return shouldConsiderWar;
        }
        Debug.Log("AP 不足，無法執行人口偵察");
        return false;
    }
    public bool ScouterMilitaryDecision(CountryStateManager target)
    {
        Country selfData = resource.countries.Find(c => c.CountryName == CountryName);
        if (selfData.AP >= 2)
        {
            selfData.AP -= 2;
            Country targetData = target.resource.countries.Find(c => c.CountryName == target.CountryName);

            bool mustWar = selfData.MilPower > targetData.MilPower;
            Debug.Log($"<color=yellow>[深度情報]</color> {CountryName} 獲得了 {target.CountryName} 的軍隊確切數量。");
            Debug.Log($"結果：己方軍力 {selfData.MilPower:F1} vs 目標 {targetData.MilPower:F1}。判定必定開戰：{mustWar}");

            return mustWar;
        }
        Debug.Log("AP 不足，無法執行軍事偵察");
        return false;
    }
}