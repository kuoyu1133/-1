using UnityEngine;
//民心值系統

[System.Serializable]
public class MoraleSystem
{
    public int MoraleValue { get; private set; } = 50; //初始化民心值
    private const int Min = 0;
    private const int Max = 100;
    //民心值最大100，最小0
    public void ModifyMorale(int morale)//更改信賴值，確保不超過設定範圍
    {
        MoraleValue = Mathf.Clamp(MoraleValue + morale, Min, Max);
    }
    public bool IsDefeated => MoraleValue <= 0;//判斷遊戲是否失敗

    public void SetMorale(int morale)
    {
        MoraleValue = Mathf.Clamp(morale, Min, Max);
    }
}