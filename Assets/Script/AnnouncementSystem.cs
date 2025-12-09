using UnityEngine;

public class AnnouncementSystem : MonoBehaviour
{
    public bool isDisaster;   // 天災
    public bool isWar;        // 戰爭
    public bool isDisease;    // 疾病

    public CountryStateManager countryA;
    public CountryStateManager countryB;
    public CountryStateManager player;

    public enum SupportChoice { None, SupportA, SupportB }
    public SupportChoice playerChoice = SupportChoice.None;

    private void Start()
    {
        if (isDisaster && countryA != null) HandleDisaster(countryA,player);
        if (isDisease && countryA != null) HandleDisease(countryA,player);
        if (isWar && countryA != null && countryB != null)
        {
            HandleWar(countryA, countryB);
            HandleWarSupport(playerChoice);
        }
    }
    
    public void HandleDisaster(CountryStateManager target,CountryStateManager player)
    {
        Country DisasterCountry = target.resource.countries.Find(c => c.CountryName == target.CountryName); // 找受影響國的資料

        Debug.Log($"{DisasterCountry.CountryName} 發生天災");

        // 天災效果
        DisasterCountry.DailyFoodProd -= Mathf.RoundToInt(target.Food * 0.7f);
        DisasterCountry.MilPower -= Mathf.RoundToInt(target.MilPower * 0.5f);

        AnnounceDisaster(target);
        ProcessSupport(target,player, true, true, 30, 15, -5); // 糧食30% 信賴+15 / 人力 信賴+10 / 拒絕-5
    }

    public void HandleDisease(CountryStateManager target, CountryStateManager player)
    {
        Country DiseaseCountry = target.resource.countries.Find(c => c.CountryName == target.CountryName); // 找受影響國的資料

        Debug.Log($"{DiseaseCountry.CountryName} 發生疾病");

        DiseaseCountry.DailyFoodProd = Mathf.RoundToInt(target.Food * 0.5f); // 糧食減半
        DiseaseCountry.MilPower = Mathf.RoundToInt(target.Military * 0.75f); // 軍力減少25%

        AnnounceDisease(target);
        ProcessSupport(target,player, true, true, 15, 10, -3); // 糧食15% 信賴+10 / 人力 信賴+5 / 拒絕-3
    }

    public void HandleWar(CountryStateManager a, CountryStateManager b)
    {
        Country WarA = a.resource.countries.Find(c => c.CountryName == a.CountryName); // 找戰爭國的資料
        Country WarB = b.resource.countries.Find(c => c.CountryName == b.CountryName);
        Debug.Log($"{WarA.CountryName} 與 {WarB.CountryName} 開戰");

        // 互相降低信賴值
        a.trust.ModifyTrust(WarB.CountryName, -30); // A 對 B -30
        b.trust.ModifyTrust(WarA.CountryName, -30); // B 對 A -30

        AnnounceWar(a, b);
    }

    private void ProcessSupport(CountryStateManager targetA, CountryStateManager targetB, bool isSupported, bool isFood, int foodPercent, int trustGainFood, int trustGainOtherOrReject)
    {
        Country SupportCountryA = targetA.resource.countries.Find(c => c.CountryName == targetA.CountryName); // 找支援國A的資料
        Country SupportCountryB = targetB.resource.countries.Find(c => c.CountryName == targetB.CountryName); // 找支援國B的資料
        if (isSupported)
        {
            if (isFood)
            {
                int foodToSend = Mathf.RoundToInt(player.Food * foodPercent / 100f);
                SupportCountryB.Food -= foodToSend;
                SupportCountryA.Food += foodToSend;

                targetA.trust.ModifyTrust(SupportCountryB.CountryName, trustGainFood); // 對玩家信賴增加
                targetB.morale.ModifyMorale(-5); // 支援消耗民心
                Debug.Log($"{SupportCountryB.CountryName} 支援 {SupportCountryA.CountryName} 糧食 {foodPercent}%，對方信賴+{trustGainFood}，自己民心-5");
            }
            else
            {
                targetA.trust.ModifyTrust(SupportCountryB.CountryName, trustGainOtherOrReject);
                targetB.morale.ModifyMorale(-5);
                Debug.Log($"{SupportCountryB.CountryName} 支援 {SupportCountryA.CountryName} 人力，對方信賴+{trustGainOtherOrReject}，自己民心-5");
            }
        }
        else
        {
            targetA.trust.ModifyTrust(SupportCountryB.CountryName, -trustGainOtherOrReject);
            Debug.Log($"{SupportCountryB.CountryName} 拒絕支援 {SupportCountryA.CountryName}，對方信賴-{trustGainOtherOrReject}");
        }
    }

    private void AnnounceDisaster(CountryStateManager target)
    {
        Country AnnouncedDisasterCountry = target.resource.countries.Find(c => c.CountryName == target.CountryName); // 找受影響國的資料
        Debug.Log($"{AnnouncedDisasterCountry.CountryName} 發生天災，是否支援糧食(30% & 信賴+15) 或 人力(信賴+10)，拒絕-5信賴");
    }

    private void AnnounceDisease(CountryStateManager target)
    {
        Country AnnouncedDiseaseCountry = target.resource.countries.Find(c => c.CountryName == target.CountryName); // 找受影響國的資料
        Debug.Log($"{AnnouncedDiseaseCountry.CountryName} 發生疾病，是否支援糧食(15% & 信賴+10) 或 人力(信賴+5)，拒絕-3信賴");
    }

    private void AnnounceWar(CountryStateManager a, CountryStateManager b)
    {
        Country AnnouncedWarCountryA = a.resource.countries.Find(c => c.CountryName == a.CountryName); // 找戰爭國的資料
        Country AnnouncedWarCountryB = b.resource.countries.Find(c => c.CountryName == b.CountryName);
        Debug.Log($"{AnnouncedWarCountryA.CountryName} 與 {AnnouncedWarCountryB.CountryName} 開戰！");
        Debug.Log($"是否支援 {AnnouncedWarCountryA.CountryName}（+20 信賴，自己 -10 民心）？");
        Debug.Log($"是否支援 {AnnouncedWarCountryB.CountryName}（+20 信賴，自己 -10 民心）？");
        Debug.Log($"選擇不支援（雙方 -10 信賴）");
    }

    private void HandleWarSupport(SupportChoice choice)
    {
        Country playerCountry = player.resource.countries.Find(c => c.CountryName == player.CountryName);
        Country SupportCountryA = countryA.resource.countries.Find(c => c.CountryName == countryA.CountryName);
        Country SupportCountryB = countryB.resource.countries.Find(c => c.CountryName == countryB.CountryName);
        switch (choice)
        {
            case SupportChoice.SupportA:
                countryA.trust.ModifyTrust(playerCountry.CountryName, 20);
                player.morale.ModifyMorale(-10);
                Debug.Log($"玩家支援了 {SupportCountryA.CountryName}，信賴+20，自己民心-10");
                break;

            case SupportChoice.SupportB:
                countryB.trust.ModifyTrust(playerCountry.CountryName, 20);
                player.morale.ModifyMorale(-10);
                Debug.Log($"玩家支援了 {SupportCountryB.CountryName}，信賴+20，自己民心-10");
                break;

            case SupportChoice.None:
                countryA.trust.ModifyTrust(playerCountry.CountryName, -10);
                countryB.trust.ModifyTrust(playerCountry.CountryName, -10);
                Debug.Log($"玩家選擇不支援，{SupportCountryA.CountryName} 與 {SupportCountryB.CountryName} 對玩家信賴-10");
                break;
        }
    }
}