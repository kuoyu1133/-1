using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CSVTrainingLogger : MonoBehaviour
{
    private string filePath = @"C:\Unity\Project\MyProject\Logs\TrainingLog.csv";
    private int currentEpisode = 0;
    private int currentStep = 0;

    void Start()
    {
        // 確保目錄存在
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        // 初始化 CSV 標頭，欄位必須與 LogData 一致
        // 加入 Tag 用於區分 Agent 或 RBC
        string header = "Tag,Episode,Step,Action,Reward,AgentFood,AgentAP,AgentMil,TargetMil\n";
        File.WriteAllText(filePath, header);
    }

    public void OnNewEpisode() { currentEpisode++; currentStep = 0; }

    public void LogData(string countryTag, int action, float reward, Country c, Country t)
    {
        currentStep++;
        // 將資料轉換為字串
        string row = $"{countryTag},{currentEpisode},{currentStep},{action},{reward}," +
                     $"{c.Food},{c.AP},{c.MilPower},{t.MilPower}\n";

        File.AppendAllText(filePath, row);
    }

    public void LogGameResult(string countryTag, string result, int finalStep, float finalReward)
    {
        // 記錄一條特殊的結算資料，方便在 Excel 中過濾 "Result" 標籤
        string row = $"{countryTag}_RESULT,{currentEpisode},{finalStep},999,{finalReward},0,0,0,0,{result}\n";
        File.AppendAllText(filePath, row);
    }
}
