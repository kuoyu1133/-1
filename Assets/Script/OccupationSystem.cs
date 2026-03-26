using Mono.Cecil;
using UnityEngine;
//戰勝他國後可選擇是否簽署不平等協議(Y:信賴值重置為0 N:信賴值設為30)並獲得該國大量物資(各項物資年產量的50%)
public class OccupationSystem : MonoBehaviour
{
    bool isOccupied = false;
    bool Assigned = false;
    public void Occupy(CountryStateManager attack, CountryStateManager defend, bool playerAssigned)
    {
        Country defendCountry = defend.resource.countries.Find(c => c.CountryName == defend.CountryName); // 找防守國的資料
        Country attackCountry = attack.resource.countries.Find(c => c.CountryName == defend.CountryName); // 找進攻國的資料

        defendCountry.City -= 1;
        if (defend.occupation.isOccupied)
        {
            Debug.Log($"{defend.CountryName} 已被佔領，無法再次攻擊！");
            return;
        }

        // 記錄佔領狀態與玩家選擇
        defend.occupation.isOccupied = true;

        if (playerAssigned)
        {
            // 簽署協議
            // 每國獨立計算信賴值還沒處理
            attack.trust.SetTrust(attack.CountryName, 0);
            defend.trust.SetTrust(defend.CountryName, 0);

            // 產量提升25%
            attackCountry.DailyIronProd += Mathf.RoundToInt(defendCountry.DailyIronProd * 0.25f);
            attackCountry.DailyFoodProd += Mathf.RoundToInt(defendCountry.DailyFoodProd * 0.25f);
            attackCountry.DailyWoodProd += Mathf.RoundToInt(defendCountry.DailyWoodProd * 0.25f);

            if (defendCountry.City >= 2) {
                defendCountry.DailyIronProd -= Mathf.RoundToInt(defendCountry.DailyIronProd * 0.25f);
                defendCountry.DailyFoodProd -= Mathf.RoundToInt(defendCountry.DailyFoodProd * 0.25f);
                defendCountry.DailyWoodProd -= Mathf.RoundToInt(defendCountry.DailyWoodProd * 0.25f);
            }
            else {
                attackCountry.Iron += defendCountry.Iron;
                attackCountry.Food += defendCountry.Food;
                attackCountry.Wood += defendCountry.Wood;

                defendCountry.DailyIronProd = 0;
                defendCountry.DailyFoodProd = 0;
                defendCountry.DailyWoodProd = 0;
                defendCountry.Iron = 0;
                defendCountry.Food = 0;
                defendCountry.Wood = 0;
            }
            Debug.Log($"{attack.CountryName} 與 {defend.CountryName} 簽署協議，雙方信賴值 = 0");
        }

        else
        {
            // 拒絕協議
            // 每國獨立計算信賴值還沒處理
            attack.trust.SetTrust(attack.CountryName, 30);
            defend.trust.SetTrust(defend.CountryName, 30);

            Debug.Log($"{attack.CountryName} 與 {defend.CountryName} 拒絕簽署，雙方信賴值 = 30");
        }

        defend.occupation.Assigned = playerAssigned;
    }
}
