using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Country
{
    public MoraleSystem morale;
    public string CountryName;
    public int AP; // 行動點
    public int Population;
    public int Iron;
    public int Food;
    public int Wood;
    public float MilPower;
    public int Defense;
    public int City;
    public float PopulationGrowthRate;

    public int DailyFoodProd;
    public int DailyIronProd;
    public int DailyWoodProd;

    public Country(string name, int ap, int population, int iron, int food, int wood, int milPower, int defense, float growthRate, int foodProd, int ironProd, int woodProd, int city)
    {
        CountryName = name;
        Population = population;
        Iron = iron;
        Food = food;
        Wood = wood;
        MilPower = milPower;
        Defense = defense;
        PopulationGrowthRate = growthRate;
        DailyFoodProd = foodProd;
        DailyIronProd = ironProd;
        DailyWoodProd = woodProd;
        City = city;
        AP = ap;
        morale = new MoraleSystem();
    }
}
public class ResourceSystem : MonoBehaviour
{
    public List<Country> countries;

    void Awake()
    {
        countries = new List<Country>()
        {
            /*new Country("鐵之國", 5, 0, 8000, 1500, 800, 1000, 130, 100, 0.1f, 800, 450, 300, 3),
            new Country("糧之國", 5, 0, 10000, 400, 1800, 1000, 100, 100, 0.3f, 1300, 100, 200, 4),
            new Country("木材國", 5, 0, 8500, 700, 900, 1600, 100, 130, 0.15f, 900, 200, 450, 3),
            new Country("貿易國", 5, 0, 7500, 500, 600, 600, 100, 100, 0.1f, 700, 180, 200, 2),
            new Country("奢侈品國", 5, 0, 9000, 800, 1000, 900, 100, 100, 0.1f, 900, 220, 300, 3),
            new Country("科技國", 5, 0, 8500, 900, 900, 900, 110, 110, 0.1f, 950, 250, 300, 3)*/
            new Country("Country A", 5, 8000, 1000, 1000, 1000, 120, 100, 10f, 800, 400, 300, 3),
            new Country("Country B", 5, 9000, 900, 1100, 900, 110, 100, 10f, 700, 350, 250, 3)
        };
        foreach (var c in countries)
            Debug.Log($"初始化 {c.CountryName}");
    }
    public void UpdateDay(Country country)
    {
        // 每日消耗
        int foodConsume = Mathf.CeilToInt(country.Population * 0.1f);
        int ironConsume = Mathf.CeilToInt(country.Population * 0.02f);
        int woodConsume = Mathf.CeilToInt(country.Population * 0.03f);

        country.Food -= foodConsume;
        country.Iron -= ironConsume;
        country.Wood -= woodConsume;

        // 每日產量
        country.Food += Mathf.CeilToInt(country.DailyFoodProd);
        country.Iron += Mathf.CeilToInt(country.DailyIronProd);
        country.Wood += Mathf.CeilToInt(country.DailyWoodProd);

        // 人口增長
        country.Population += Mathf.CeilToInt(country.Population * country.PopulationGrowthRate);
    }

    public bool IsResourceShortage(Country country)
    {
        float foodSupport = country.Food / Mathf.Max(1, country.Population * 0.1f);
        float ironSupport = country.Iron / Mathf.Max(1, country.Population * 0.02f);
        float woodSupport = country.Wood / Mathf.Max(1, country.Population * 0.03f);

        float minSupport = Mathf.Min(foodSupport, ironSupport, woodSupport);
        return minSupport < country.Population * 0.5f;
    }

}