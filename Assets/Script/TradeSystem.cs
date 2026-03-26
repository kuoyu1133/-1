using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class TradeSystem : MonoBehaviour
{
    public List<CountryStateManager> allCountries; // 所有國家
    //public GameObject tradeUIPanel; // 提供一個是否交易的UI
    public TMP_Text tradeUIText;
    public Button acceptButton;
    public Button rejectButton;

    private TradeRequest currentRequest;

    public const float ironPerPerson = 0.02f;  // 100 人需要 2 個鐵 → 1 人需要 0.02
    public const float foodPerPerson = 0.05f;  // 100 人需要 5 個糧食 → 1 人需要 0.05
    public const float woodPerPerson = 0.03f;  // 100 人需要 3 個木材 → 1 人需要 0.03

    [System.Serializable]
    public class TradeRequest
    {
        public CountryStateManager requester;
        public CountryStateManager provider;
        public string resourceType;
        public int amount;
        public bool accepted;
        public bool isPlayerRequest;
    }

    void Start()
    {
        //Debug.Log("[TradeSystem] Start 被呼叫");
        //tradeUIPanel.SetActive(false);

        /*acceptButton.onClick.AddListener(() => //偵測到同意交易按鈕被點擊，執行下列操作
        {
            Debug.Log("[TradeSystem] acceptButton 被點擊");
            if (currentRequest != null)
            {
                currentRequest.accepted = true;
                ExecuteTrade(currentRequest);
                //tradeUIPanel.SetActive(false);
            }
        });

        rejectButton.onClick.AddListener(() => //偵測到不同意交易按鈕被點擊，執行下列操作
        {
            Debug.Log("[TradeSystem] rejectButton 被點擊");
            if (currentRequest != null)
            {
                currentRequest.accepted = false;
                ExecuteTrade(currentRequest);
                //tradeUIPanel.SetActive(false);
            }
        });*/
    }

    public void DailyTrade(CountryStateManager self)
    {
        Country selfData = self.resource.countries.Find(c => c.CountryName == self.CountryName);
        if (selfData == null) return;

        string[] resourceTypes = { "Food", "Iron", "Wood" };

        foreach (string rType in resourceTypes)
        {
            int myNeed = GetResourceNeed(self, rType);

            // 只有我真的缺資源時才執行
            if (myNeed > 0)
            {
                // 簡化邏輯：直接向「市場」或「抽象來源」購買
                // 如果你要跟特定國家買，要在這裡 FindProvider

                int tradeAmount = Mathf.Min(myNeed, 500); // 限制單次交易上限，防止數值暴增

                Debug.Log($"<color=yellow>[交易執行]</color> {self.CountryName} 購買了 {tradeAmount} {rType}");

                // 執行增加資源
                switch (rType)
                {
                    case "Food": selfData.Food += tradeAmount; break;
                    case "Iron": selfData.Iron += tradeAmount; break;
                    case "Wood": selfData.Wood += tradeAmount; break;
                }

                // 如果有金錢系統，記得在這裡扣錢
                // selfData.Gold -= tradeAmount * price; 
            }
        }
    
    // 其他 AI 國家之間的交易仍然自動處理
    /*
    foreach (var requester in allCountries)
    {
        foreach (var provider in allCountries)
        {
            if (requester == provider || requester == playerCountry || provider == playerCountry) continue;

            foreach (var resource in new string[] { "Iron", "Food", "Wood" })
            {
                int needed = GetResourceNeed(requester, resource);
                if (needed > 0)
                {
                    CountryStateManager bestProvider = FindProvider(requester, resource, needed);
                    if (bestProvider != null && bestProvider != playerCountry)
                    {
                        TradeRequest request = new TradeRequest
                        {
                            requester = requester,
                            provider = bestProvider,
                            resourceType = resource,
                            amount = needed,
                            isPlayerRequest = false
                        };
                        request.accepted = DecideTrade(bestProvider, request);
                        ExecuteTrade(request);
                    }
                }
            }
        }
    }
    */
    }


    void ShowTradeUI(TradeRequest request) //只顯示與玩家相關交易
    {
        if (!request.isPlayerRequest) return;
        Debug.Log($"[TradeSystem] ShowTradeUI() 被呼叫: {request.requester.CountryName} 要求 {request.amount} {request.resourceType}");
        //tradeUIPanel.SetActive(true);
        //tradeUIText.text = $"{request.requester.CountryName} 向 {request.provider.CountryName} 請求交易 {request.amount} {request.resourceType}。\n是否同意交易？";
    }

    public int GetResourceNeed(CountryStateManager country, string resource)
    {
        Country pCountry = country.resource.countries.Find(c => c.CountryName == country.CountryName);

        // 計算需求總量
        int threshold = 0;
        switch (resource)
        {
            case "Iron":
                threshold = Mathf.CeilToInt(pCountry.Population * ironPerPerson);
                break;
            case "Food":
                threshold = Mathf.CeilToInt(pCountry.Population * foodPerPerson);
                break;
            case "Wood":
                threshold = Mathf.CeilToInt(pCountry.Population * woodPerPerson);
                break;
        }

        // 計算缺口
        int current = 0;
        switch (resource)
        {
            case "Iron": current = pCountry.Iron; break;
            case "Food": current = pCountry.Food; break;
            case "Wood": current = pCountry.Wood; break;
        }

        return current < threshold ? threshold - current : 0;
    }

    public CountryStateManager FindProvider(CountryStateManager requester, string resource) //找最多資源的國家提供
    {
        Country requestCountry = requester.resource.countries.Find(c => c.CountryName == requester.CountryName);
        CountryStateManager bestProvider = null;
        int maxAvailable = 0;

        foreach (var country in allCountries)
        {
            if (country == requester) continue;

            int available = 0;
            switch (resource)
            {
                case "Iron": available = requestCountry.Iron; break;
                case "Food": available = requestCountry.Food; break;
                case "Wood": available = requestCountry.Wood; break;
            }

            if (available > maxAvailable)
            {
                maxAvailable = available;
                bestProvider = country;
            }
        }
        return bestProvider;
    }

    bool DecideTrade(CountryStateManager provider, TradeRequest request)
    {
        Country provideCountry = provider.resource.countries.Find(c => c.CountryName == provider.CountryName);
        int available = 0;
        switch (request.resourceType)
        {
            case "Iron": available = provideCountry.Iron; break;
            case "Food": available = provideCountry.Food; break;
            case "Wood": available = provideCountry.Wood; break;
        }
        return available >= request.amount;
    }

    public void ExecuteTrade(TradeRequest request)
    {
        Debug.Log($"[TradeSystem] ExecuteTrade() 被呼叫: {request.requester.CountryName} -> {request.provider.CountryName} {request.amount} {request.resourceType}, accepted={request.accepted}");
        int finalAmount = Mathf.Max(0, request.amount);
        int tariff = 30;//基礎關稅
        Country requesterData = request.requester.resource.countries.Find(c => c.CountryName == request.requester.CountryName);
        Country providerData = request.provider.resource.countries.Find(c => c.CountryName == request.provider.CountryName);

        if (request.requester.trust.CanEstablishRelations(providerData.CountryName)) tariff = 20;
        if (request.requester.trust.CanAlliance(providerData.CountryName)) tariff = 10;
        finalAmount = finalAmount * (100 - tariff) / 100;


        if (request.accepted)
        {
            switch (request.resourceType)
            {
                case "Iron":
                    finalAmount = Mathf.Min(finalAmount, providerData.Iron);
                    if (finalAmount <= 0) return; // 沒東西可賣，直接結束
                    requesterData.Iron += finalAmount;
                    providerData.Iron -= finalAmount;
                    break;
                case "Food":
                    finalAmount = Mathf.Min(finalAmount, providerData.Food);
                    if (finalAmount <= 0) return;
                    requesterData.Food += finalAmount;
                    providerData.Food -= finalAmount;
                    break;
                case "Wood":
                    finalAmount = Mathf.Min(finalAmount, providerData.Wood);
                    if (finalAmount <= 0) return;
                    requesterData.Food += finalAmount;
                    providerData.Food -= finalAmount;
                    break;
            }

            requesterData.Food = Mathf.Clamp(requesterData.Food, 0, 1000000);
            providerData.Food = Mathf.Clamp(providerData.Food, 0, 1000000);

            request.requester.trust.ModifyTrust(providerData.CountryName, 10);
            request.provider.trust.ModifyTrust(requesterData.CountryName, 10);

            Debug.Log($"{requesterData.CountryName} 從 {providerData.CountryName} 成功交易 {finalAmount} {request.resourceType}，雙方信賴+10");
        }
        else
        {
            request.requester.trust.ModifyTrust(providerData.CountryName, -15);
            request.provider.trust.ModifyTrust(requesterData.CountryName, -15);
            Debug.Log($"{requesterData.CountryName} 從 {providerData.CountryName} 提出交易被拒絕，雙方信賴-5");
        }
    }
}
