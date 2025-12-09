using UnityEngine;

public class PolicySystem : MonoBehaviour
{
    // 生育政策：只負責當下啟動時的效果（例如立即出生）
    public void ApplyPopulationPolicy(Country country)
    {
        float rand = Random.Range(country.PopulationGrowthRate, 2 * country.PopulationGrowthRate);
        int popIncrease = Mathf.CeilToInt(country.Population * rand / 100f);

        country.Population += popIncrease;

        Debug.Log($"{country.CountryName} 生育政策啟動，本回合人口增加 {popIncrease}");
    }

    // 軍事政策：只負責啟動瞬間效果
    public void ApplyMilitaryPolicy(Country country)
    {
        country.Population += Mathf.CeilToInt(country.Population * (country.PopulationGrowthRate - 0.15f));
        country.MilPower += country.Population * 0.015f;
        country.Population = (int)(country.Population * 0.985f);
        country.morale.ModifyMorale(-50);

        Debug.Log($"{country.CountryName} 軍事政策啟動，士兵戰力與民心值變化");
    }
}
