/*using UnityEngine;
using System.Collections.Generic;

public class CountryDetailManager : MonoBehaviour
{
    public GameObject statRowPrefab;
    public Transform container; // 記得掛 Vertical Layout Group

    private Dictionary<string, StatRowUI> statRows = new Dictionary<string, StatRowUI>();

    public void UpdateFullList(Country country)
    {
        // 依照你需要的屬性，逐行更新
        RefreshRow("國家名稱", country.CountryName);
        RefreshRow("行動點 (AP)", country.AP.ToString());
        RefreshRow("目前人口", country.Population.ToString("N0"));
        RefreshRow("糧食庫存", country.Food.ToString("N0"));
        RefreshRow("鋼鐵資源", country.Iron.ToString("N0"));
        RefreshRow("木材資源", country.Wood.ToString("N0"));
        RefreshRow("軍事實力", country.MilPower.ToString("F1"));
        // 修改為：
        RefreshRow("民心穩定", country.morale.MoraleValue.ToString() + "%");
    }

    private void RefreshRow(string label, string value)
    {
        if (!statRows.ContainsKey(label))
        {
            GameObject go = Instantiate(statRowPrefab, container);
            StatRowUI row = go.GetComponent<StatRowUI>();
            statRows.Add(label, row);
        }
        statRows[label].SetInfo(label, value);
    }
}*/


using UnityEngine;
using System.Collections.Generic;

public class CountryDetailManager : MonoBehaviour
{
    public GameObject statRowPrefab;
    public Transform container; // 已經掛載了 Vertical Layout Group 的 StatContainer

    // 1. 新增：清空清單的方法，供 ResourceSystem 刷新前呼叫
    public void ClearList()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateFullList(Country country)
    {
        // 2. 為了並列顯示時能分辨 A 和 B，我們增加一行「國家標題」
        CreateNewRow("--- " + country.CountryName + " ---", "");

        // 依照需要的屬性，逐行生成新物件
        CreateNewRow("國家名稱", country.CountryName);
        CreateNewRow("行動點 (AP)", country.AP.ToString());
        CreateNewRow("目前人口", country.Population.ToString("N0"));
        CreateNewRow("糧食庫存", country.Food.ToString("N0"));
        CreateNewRow("鋼鐵資源", country.Iron.ToString("N0"));
        CreateNewRow("木材資源", country.Wood.ToString("N0"));
        CreateNewRow("軍事實力", country.MilPower.ToString("F1"));
        CreateNewRow("民心穩定", country.morale.MoraleValue.ToString() + "%");

        // 增加一個空白行作為國家間的間隔
        CreateNewRow("", "");
    }

    // 3. 修改：移除 Dictionary 邏輯，改為直接 Instantiate
    private void CreateNewRow(string label, string value)
    {
        GameObject go = Instantiate(statRowPrefab, container);
        StatRowUI row = go.GetComponent<StatRowUI>();

        if (row != null)
        {
            row.SetInfo(label, value);
        }
    }
}