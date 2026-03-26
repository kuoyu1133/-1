using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance;
    private Queue<string> logQueue = new Queue<string>();
    private bool isPrinting = false;
    public bool canPrint = true; // 🚩 全域開關：控制是否允許 Log 輸出
    public bool IsBusy => logQueue.Count > 0 || isPrinting;

    void Awake() { Instance = this; }

    public void EnqueueLog(string message)
    {
        logQueue.Enqueue(message);
        if (!isPrinting) StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isPrinting = true;
        while (logQueue.Count > 0)
        {
            if (!canPrint) { yield return null; continue; } // 暫停時不噴 Log
            Debug.Log(logQueue.Dequeue());
            // 🚩 這裡控制全域所有 Log 的強制間隔
            yield return new WaitForSeconds(0.5f);
        }
        isPrinting = false;
    }

    public void EnqueueActionLog(string countryName, string actionMessage, bool isAI)
    {
        string tag = isAI ? "<color=green>[AI]</color>" : "<color=yellow>[玩家]</color>";
        string finalLog = $"{tag} {countryName} : {actionMessage}";
        EnqueueLog(finalLog); // 呼叫你原本的排隊系統
    }
}