using UnityEngine;
using TMPro; // 使用 TextMeshPro 效能更好、字體更清晰

public class CountryUIItem : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI nameText;       // 國家名稱文字
    public TextMeshProUGUI populationText; // 人口文字
    public TextMeshProUGUI apText;         // 行動點文字
    public TextMeshProUGUI resourceText;   // 資源合集 (糧/鐵/木)

    /// <summary>
    /// 將 Country 類的數據填入 UI 
    /// </summary>
    public void UpdateUI(Country country)
    {
        nameText.text = country.CountryName;
        populationText.text = $"人口: {country.Population:N0}"; // :N0 會加上千分位逗號
        apText.text = $"AP: {country.AP}";

        // 組合資源字串
        resourceText.text = $"🌾 糧食: {country.Food}  " +
                            $"⚙️ 鋼鐵: {country.Iron}  " +
                            $"🪵 木材: {country.Wood}";

        // 可選：如果民心低於 30 顯示紅色 (示意)
        // nameText.color = country.morale.GetMorale() < 30 ? Color.red : Color.white;
    }
}