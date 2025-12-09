using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CountryStateManager game;

    private void Awake() //避免該instance重複執行
    {
        if (Instance == null)
        {
            Instance = this;
            game = gameObject.AddComponent<CountryStateManager>();

            /*foreach (var country in game.resource.countries)
            {
                if (country.morale == null)
                    country.morale = gameObject.AddComponent<MoraleSystem>();
            }*/
        }
        else Destroy(gameObject);
    }
    private void Start()
    {
        // 初始化每個國家的信賴系統
        foreach (var country in game.resource.countries)
        {
            game.trust.InitializeTrusts(game.resource.countries, country.CountryName);
        }
    }

    public void NextDay() //判斷遊戲是否結束
    {
        game.DailyUpdate();
        foreach (var country in game.resource.countries)
        {
            if (country.morale.IsDefeated)
            {
                Debug.Log($"{country.CountryName} 因民心歸零而滅亡!");
            }
            /*if (country.IsAIControlled)
                country.AIUpdate();
            else
                country.DailyUpdate();

            if (!country.IsAIControlled && country.morale.IsDefeated)
                Debug.Log($"{country.CountryName} 因民心歸零而滅亡!");*/
        }
    }

}