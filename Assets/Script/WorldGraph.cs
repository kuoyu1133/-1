using System.Collections.Generic;
using UnityEngine;

public class WorldGraph : MonoBehaviour
{
    public Dictionary<CountryStateManager, List<CountryStateManager>> adjacencyList =
        new Dictionary<CountryStateManager, List<CountryStateManager>>();
    private void InitializeGraph()
    {
        var gm = GameManager.Instance;

        // 先找到對應國家的 CountryStateManager
        CountryStateManager ironCountry = gm.game.GetCountryByName("鐵之國");
        CountryStateManager techCountry = gm.game.GetCountryByName("科技國");
        CountryStateManager tradeCountry = gm.game.GetCountryByName("貿易國");
        CountryStateManager woodCountry = gm.game.GetCountryByName("木材國");
        CountryStateManager foodCountry = gm.game.GetCountryByName("糧食國");
        CountryStateManager luxuryCountry = gm.game.GetCountryByName("奢侈品國");

        // 鐵之國和科技國、貿易國、木材國接壤
        AddConnection(ironCountry, techCountry);
        AddConnection(ironCountry, tradeCountry);
        AddConnection(ironCountry, woodCountry);

        // 科技國和鐵之國、糧食國接壤
        AddConnection(techCountry, foodCountry);

        // 貿易國和鐵之國、科技國、糧食國、奢侈品國接壤
        AddConnection(tradeCountry, foodCountry);
        AddConnection(tradeCountry, luxuryCountry);

        // 木材國只和鐵之國接壤（已建立）

        // 糧食國和科技國、貿易國、奢侈品國接壤（已建立科技國、貿易國）
        AddConnection(foodCountry, luxuryCountry);

        // 奢侈品國和糧食國、貿易國接壤（已建立）
    }
    private void Start()
    {
        InitializeGraph();
    }
    public void AddConnection(CountryStateManager a, CountryStateManager b) //製作雙向graph圖
    {
        if (!adjacencyList.ContainsKey(a)) adjacencyList[a] = new List<CountryStateManager>();
        if (!adjacencyList.ContainsKey(b)) adjacencyList[b] = new List<CountryStateManager>();

        if (!adjacencyList[a].Contains(b)) adjacencyList[a].Add(b);
        if (!adjacencyList[b].Contains(a)) adjacencyList[b].Add(a);
    }

    public bool CheckAttack(CountryStateManager attacker, CountryStateManager defender)
    {
        if (adjacencyList.ContainsKey(attacker) && adjacencyList[attacker].Contains(defender)) return true;

        return HasAlliancePath(attacker, defender);
    }

    private bool HasAlliancePath(CountryStateManager start, CountryStateManager end)
    {
        HashSet<CountryStateManager> visited = new HashSet<CountryStateManager>();
        Queue<CountryStateManager> queue = new Queue<CountryStateManager>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == end) return true;

            if (!adjacencyList.ContainsKey(current)) continue;

            foreach (var neighbor in adjacencyList[current])
            {
                if (!visited.Contains(neighbor) && neighbor.trust.CanAlliance(neighbor.CountryName))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        return false;
    }
}