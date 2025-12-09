using UnityEngine;
using System.Collections.Generic;
//信賴值系統
public class TrustSystem : MonoBehaviour
{
    private Dictionary<string, int> trustValues = new Dictionary<string, int>(); //初始化信賴值
    private const int Min = -50;
    private const int Max = 100;
    //信賴值最大值100，最小值-50
    public void InitializeTrusts(List<Country> countries, string selfName)
    {
        trustValues.Clear();
        foreach (var c in countries)
        {
            if (c.CountryName != selfName)
                trustValues[c.CountryName] = 50; // 初始信賴值50
        }
    }
    public void ModifyTrust(string targetCountry, int amount)
    {
        if (!trustValues.ContainsKey(targetCountry)) return;
        trustValues[targetCountry] = Mathf.Clamp(trustValues[targetCountry] + amount, Min, Max);
    }
    public void SetTrust(string targetCountry, int value)
    {
        if (!trustValues.ContainsKey(targetCountry)) return;
        trustValues[targetCountry] = Mathf.Clamp(value, Min, Max);
    }
    public int GetTrust(string targetCountry)
    {
        if (!trustValues.ContainsKey(targetCountry)) return 0;
        return trustValues[targetCountry];
    }
    public bool CanTrade(string targetCountry) => GetTrust(targetCountry) >= 0;//判斷是否可以交易
    public bool CanEstablishRelations(string targetCountry) => GetTrust(targetCountry) > 50;//判斷是否可以建交
    public bool CanAlliance(string targetCountry) => GetTrust(targetCountry) > 70;//判斷是否可以軍事同盟
}