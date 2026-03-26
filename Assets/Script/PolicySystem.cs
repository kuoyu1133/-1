using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolicySystem : MonoBehaviour
{
    private const int MaxPopulation = 100000000;
    // 記錄執行次數 Key: CountryName_PolicyType
    private Dictionary<string, int> policyCount = new Dictionary<string, int>();
    // 記錄冷卻天數 Key: CountryName_PolicyType
    private Dictionary<string, int> policyCooldown = new Dictionary<string, int>();

    private const int CooldownDays = 5;

    public bool IsPolicyReady(string countryName, string policyType)
    {
        string key = countryName + "_" + policyType;
        if (!policyCooldown.ContainsKey(key)) return true;
        return policyCooldown[key] <= 0; // 冷卻為 0 或以下代表可用
    }
    public int ApplyPopulationPolicy(Country country)
    {
        string key = country.CountryName + "_Pop";

        float efficiency = GetEfficiency(country.CountryName, "Pop");
        int baseIncrease = country.City * 200;
        int popIncrease = Mathf.RoundToInt(baseIncrease * efficiency);

        country.Population += popIncrease;
        policyCooldown[key] = CooldownDays;

        //Debug.Log($"{country.CountryName} 執行生育政策，效率: {efficiency * 100}%，進入 {CooldownDays} 天冷卻");
        return popIncrease;
    }

    public int ApplyMilitaryPolicy(Country country)
    {
        string key = country.CountryName + "_Mil";

        float efficiency = GetEfficiency(country.CountryName, "Mil");
        float oldMil = country.MilPower;
        float gains = (country.Population * 0.001f) * efficiency;

        country.MilPower += gains;
        country.Population = Mathf.CeilToInt(country.Population * 0.99f);
        country.morale.ModifyMorale(-5);
        policyCooldown[key] = CooldownDays;

        //Debug.Log($"<color=cyan>[軍事政策]</color> {country.CountryName} 效率: {efficiency * 100}%，軍力 +{gains:F1}，進入 {CooldownDays} 天冷卻");
        return 0;
    }
    public void UpdateCooldowns(string countryName)
    {
        List<string> keys = new List<string>(policyCooldown.Keys);
        foreach (var key in keys)
        {
            if (key.StartsWith(countryName))
            {
                if (policyCooldown[key] > 0)
                {
                    policyCooldown[key]--;

                    // 當冷卻剛好歸零時，減少 policyCount 以恢復效率 (維持你原本的設計)
                    if (policyCooldown[key] <= 0)
                    {
                        if (policyCount.ContainsKey(key) && policyCount[key] > 0)
                        {
                            policyCount[key]--;
                            // 🚩 直接丟給全域管理器排隊，它會自動幫你錯開時間
                            string readableKey = key.Replace("_Pop", " [人口政策]").Replace("_Mil", " [軍事政策]");
                            LogManager.Instance.EnqueueLog($"<color=#00FF00>[政策恢復]</color> <b>{readableKey}</b> 冷卻結束。");
                        }
                    }
                }
            }
        }
    }

    private float GetEfficiency(string countryName, string policyType)
    {
        string key = countryName + "_" + policyType;

        if (!policyCount.ContainsKey(key)) policyCount[key] = 0;

        int count = policyCount[key];
        policyCount[key]++; // 執行次數增加

        // 邊際效應倍率
        if (count == 0) return 1.0f; // 100%
        if (count == 1) return 0.8f; // 80%
        if (count == 2) return 0.5f; // 50%
        return 0.2f; // 最低 20%
    }
}