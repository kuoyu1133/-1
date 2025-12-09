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
    public int Military;
    public float MilPower;
    public int Defense;


    public int Iron;
    public int Food;
    public int Wood;
    public float PopulationGrowthRate;//人口成長率

    private void Awake()//初始呼叫函式
    {
        announcement = gameObject.AddComponent<AnnouncementSystem>();//賦予遊戲物件元素
        trust = gameObject.AddComponent<TrustSystem>();
        //morale = gameObject.AddComponent<MoraleSystem>();
        policy = gameObject.AddComponent<PolicySystem>();
        resource = gameObject.AddComponent<ResourceSystem>();
        occupation = gameObject.AddComponent<OccupationSystem>();
    }


    public void DailyUpdate()
    {
        foreach (Country country in resource.countries)
        {
            resource.UpdateDay(country);

            if (country.Food < country.Population * 0.05f)
                morale.ModifyMorale(-1);
            else
                morale.ModifyMorale(5);
        }

        if (morale.IsDefeated)
            Debug.Log($"{CountryName} 因民心歸零而滅亡！");

    }
    public CountryStateManager GetCountryByName(string name)
    {
        foreach (var country in resource.countries)
        {
            if (country.CountryName == name)
                return this; // 返回對應的 CountryStateManager
        }
        return null;
    }

    /*public void AIUpdate()
    {
        // AI 行為: 自動產生政策或交易
        DailyUpdate();
    }*/
}