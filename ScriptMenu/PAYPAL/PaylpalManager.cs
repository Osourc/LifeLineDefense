using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class PayPalManager : MonoBehaviour
{
    [Header("PayPal Settings")]
    [SerializeField] private string clientId; // Assign via Unity Inspector
    [SerializeField] private string secret;  // Assign via Unity Inspector
    private string accessToken;

    [Header("UI Elements")]
    public Button[] purchaseButtons;
    public TMP_Text[] priceTexts;
    public TMP_Text dnaText;
    public TMP_Text dnaText2;

    [Header("Prices and DNA Changes")]
    public float[] itemPrices;  // Example: [1.0f, 2.0f, 3.0f]
    public int[] dnaChanges;    // Changed DnaReward to DnaChange

    [Header("Cancel Payment")]
    public GameObject CancelPanel;
    public Button Cancel;
    [SerializeField] private TextMeshProUGUI textComponent;



    private int dnaCount;
    private string playerId;
    private IMongoCollection<BsonDocument> playerCollection;

    private const string TokenUrl = "https://api-m.sandbox.paypal.com/v1/oauth2/token";
    private const string OrderUrl = "https://api-m.sandbox.paypal.com/v2/checkout/orders";

    private DateTime tokenExpiryTime;
    private bool isPurchaseInProgress = false;

    private void Start()
    {
        InitializeMongoDB();
        InitializeUI();
        StartCoroutine(WaitForAuthenticationAndFetchData());
    }

    private void InitializeMongoDB()
    {
        StartCoroutine(InitializeMongoDBCoroutine());
    }

    private IEnumerator InitializeMongoDBCoroutine()
    {
        try
        {
            var client = new MongoClient("mongodb+srv://db:db@cluster0.nth6c.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0");
            var database = client.GetDatabase("Server");
            playerCollection = database.GetCollection<BsonDocument>("Player");
        }
        catch (Exception e)
        {
            Debug.LogError("MongoDB initialization failed: " + e.Message);
        }
        yield return null;
    }

    private void InitializeUI()
    {
        if (purchaseButtons.Length != itemPrices.Length || itemPrices.Length != dnaChanges.Length)
        {
            Debug.LogError("Mismatch in purchase button count, item prices, and DNA changes.");
            return;
        }

        for (int i = 0; i < purchaseButtons.Length; i++)
        {
            int index = i;
            purchaseButtons[i].onClick.AddListener(() => OnPurchaseButtonClicked(index));
            priceTexts[i].text = $"${itemPrices[i]}";
        }
    }

    private IEnumerator WaitForAuthenticationAndFetchData()
    {
        while (string.IsNullOrEmpty(AuthenticationManager.Instance.PlayerId))
            yield return null;

        playerId = AuthenticationManager.Instance.PlayerId;
        FetchPlayerData();
    }

    private void FetchPlayerData()
    {
        if (playerCollection == null)
        {
            Debug.LogError("Player collection is not initialized.");
            return;
        }

        var filter = Builders<BsonDocument>.Filter.Eq("_id", playerId);
        var playerData = playerCollection.Find(filter).FirstOrDefault();

        if (playerData != null)
        {
            dnaCount = playerData["DnaCount"].AsInt32;
            UpdateDnaText();
        }
        else
        {
            Debug.LogError("Player data not found.");
        }
    }

        private void UpdateDnaText()
        {
            if (dnaText != null) dnaText.text = dnaCount.ToString();
            if (dnaText2 != null) dnaText2.text = dnaCount.ToString();
        }
        void PlayTypewriterEffect(string fullText, float typingSpeed)
    {
        int textLength = fullText.Length;
        
        LeanTween.value(gameObject, 0, textLength, textLength * typingSpeed)
            .setOnUpdate((float value) =>
            {
                int charCount = Mathf.FloorToInt(value);
                textComponent.text = fullText.Substring(0, charCount);
            })
            .setEase(LeanTweenType.linear);
    }


    private void OnPurchaseButtonClicked(int index)
    {
        if (!IsInternetAvailable())
        {
            Debug.LogError("No internet connection.");
            return;
        }
        if (isPurchaseInProgress)
        {
            Debug.LogError("A purchase is already in progress. Please wait.");
            return;
        }

        foreach (var button in purchaseButtons)
            button.interactable = false;

        purchaseButtons[index].GetComponentInChildren<TMP_Text>().text = "Processing...";
        StartCoroutine(LoginAndPurchase(itemPrices[index], dnaChanges[index], index));

        CancelPanel.SetActive(true); 
        PlayTypewriterEffect("Processing...", 0.5f);
  
    }


    public void OnCancelPayment()
    {
        
        // Show the cancel panel
        CancelPanel.SetActive(false);

        // Reset the buttons and stop the current process
        foreach (var button in purchaseButtons)
            button.interactable = true;

        // Reset button text
        foreach (var button in purchaseButtons)
            button.GetComponentInChildren<TMP_Text>().text = "Purchase";

        // Stop all running coroutines and reset the flag
        StopAllCoroutines();
        Debug.Log("Purchase operation cancelled.");
    }

    private IEnumerator LoginAndPurchase(float itemPrice, int itemDnaChange, int index)
    {

        yield return GetAccessToken((token) =>
        {
            if (!string.IsNullOrEmpty(token))
            {
                Debug.Log("Access token obtained successfully.");
                StartCoroutine(CreateAndCaptureOrder(itemPrice, itemDnaChange, index));
            }
            else
            {
                Debug.LogError("Failed to obtain access token.");
                ResetPurchaseButton(index);
          
            }
        });
    }

    private IEnumerator GetAccessToken(Action<string> onSuccess)
    {
        if (!IsInternetAvailable())
        {
            Debug.LogError("No internet connection.");
            onSuccess(null);
            yield break;
        }

        if (!string.IsNullOrEmpty(accessToken) && DateTime.UtcNow < tokenExpiryTime)
        {
            onSuccess(accessToken);
            yield break;
        }

        string credentials = $"{clientId}:{secret}";
        string encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        UnityWebRequest request = UnityWebRequest.PostWwwForm(TokenUrl, "");
        request.SetRequestHeader("Authorization", $"Basic {encodedCredentials}");
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("grant_type=client_credentials"));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<PayPalTokenResponse>(request.downloadHandler.text);
            accessToken = response.access_token;
            tokenExpiryTime = DateTime.UtcNow.AddSeconds(response.expires_in - 60); // Add buffer for token renewal.
            onSuccess(accessToken);
        }
        else
        {
            Debug.LogError($"Failed to get access token: {request.error}");
            onSuccess(null);
            
         
        }
    }

    private IEnumerator CreateAndCaptureOrder(float itemPrice, int itemDnaChange, int index)
    {
        UnityWebRequest createOrderRequest = new UnityWebRequest(OrderUrl, "POST");
        createOrderRequest.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        createOrderRequest.SetRequestHeader("Content-Type", "application/json");

        string orderData = $"{{\"intent\":\"CAPTURE\",\"purchase_units\":[{{\"amount\":{{\"currency_code\":\"USD\",\"value\":\"{itemPrice}\"}}}}]}}";
        createOrderRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(orderData));
        createOrderRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return createOrderRequest.SendWebRequest();

        if (createOrderRequest.result == UnityWebRequest.Result.Success)
        {
            var orderResponse = JsonUtility.FromJson<PayPalOrderResponse>(createOrderRequest.downloadHandler.text);
            if (orderResponse.status == "CREATED")
            {
                string approvalLink = orderResponse.links.Find(link => link.rel == "approve")?.href;

                if (!string.IsNullOrEmpty(approvalLink))
                {
                    Debug.Log($"Redirecting to PayPal: {approvalLink}");
                    Application.OpenURL(approvalLink);

                    yield return StartCoroutine(WaitForUserApproval(orderResponse.id, itemPrice, itemDnaChange, index));
                }
                else
                {
                    Debug.LogError("Approval link not found in PayPal response.");
                    ResetPurchaseButton(index);
                
                }
            }
            else
            {
                Debug.LogError($"Order status not approved: {orderResponse.status}");
                ResetPurchaseButton(index);
          
            }
        }
        else
        {
            Debug.LogError($"Failed to create order: {createOrderRequest.error}");
            ResetPurchaseButton(index);
       
        }
    }

    private IEnumerator WaitForUserApproval(string orderId, float itemPrice, int itemDnaChange, int index)
{
    bool approvalReceived = false;
    float timeout = 60f; // Timeout in seconds
    float timer = 0f;

    while (!approvalReceived && timer < timeout)
    {
        UnityWebRequest orderStatusRequest = new UnityWebRequest($"{OrderUrl}/{orderId}", "GET");
        orderStatusRequest.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        orderStatusRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return orderStatusRequest.SendWebRequest();

        if (orderStatusRequest.result == UnityWebRequest.Result.Success)
        {
            var orderStatusResponse = JsonUtility.FromJson<PayPalOrderResponse>(orderStatusRequest.downloadHandler.text);

            if (orderStatusResponse.status == "APPROVED")
            {
                approvalReceived = true;
                StartCoroutine(CapturePayment(orderId, itemPrice, itemDnaChange, index));
                yield break;
            }
            else if (orderStatusResponse.status == "CANCELLED" || orderStatusResponse.status == "DECLINED")
            {
                Debug.LogError($"Payment was {orderStatusResponse.status}. Aborting.");
                break;
            }
            else
            {
                Debug.Log($"Payment status: {orderStatusResponse.status}. Waiting for approval...");
            }
        }
        else
        {
            Debug.LogError($"Failed to check order status: {orderStatusRequest.error}");
            break;
        }

        timer += 2f;
        yield return new WaitForSeconds(2f);
    }

    // Timeout or user did not approve the payment
    if (!approvalReceived)
    {
        Debug.LogError("Payment approval timed out or failed.");
        ResetPurchaseButton(index);
      
    }
}



    private IEnumerator CapturePayment(string orderId, float itemPrice, int itemDnaChange, int index)
    {
        UnityWebRequest captureRequest = new UnityWebRequest($"{OrderUrl}/{orderId}/capture", "POST");
        captureRequest.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        captureRequest.SetRequestHeader("Content-Type", "application/json");
        captureRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return captureRequest.SendWebRequest();

        if (captureRequest.result == UnityWebRequest.Result.Success)
        {
            dnaCount += itemDnaChange;
            UpdateDnaText();
            Debug.Log($"Payment captured successfully. Added {itemDnaChange} DNA.");

            StartCoroutine(UpdatePlayerDataInMongoDB(itemDnaChange, index));
            DisplayReceipt(itemDnaChange, itemPrice);
            ResetPurchaseButton(index);
      

        }
        else
        {
            Debug.LogError($"Failed to capture payment: {captureRequest.error}");
            ResetPurchaseButton(index);
       
        }
    }

    

    private void ResetPurchaseButton(int index)
    {
        // purchaseButtons[index].interactable = true;
        // purchaseButtons[index].GetComponentInChildren<TMP_Text>().text = "Purchase";
        OnCancelPayment();
        
    }

    private void DisplayReceipt(int itemDnaChange, float itemPrice)
    {
        Debug.Log($"Receipt: DNA Added: {itemDnaChange}, Price: ${itemPrice}");
    }

    private IEnumerator UpdatePlayerDataInMongoDB(int itemDnaChange, int index)
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", playerId);
            var update = Builders<BsonDocument>.Update.Inc("DnaCount", itemDnaChange);

            var transactionLog = new BsonDocument
            {
                { "TransactionDate", DateTime.UtcNow },
                { "ItemIndex", index },
                { "DnaChange", itemDnaChange }
            };

            playerCollection.UpdateOne(filter, update);
            playerCollection.UpdateOne(filter, Builders<BsonDocument>.Update.Push("Transactions", transactionLog));

            Debug.Log("Player DNA count updated in MongoDB.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"MongoDB update failed: {ex.Message}");
        }
        yield return null;
    }

    private bool IsInternetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}




[System.Serializable]
public class PayPalTokenResponse
{
    public string access_token;
    public int expires_in;
}

[System.Serializable]
public class PayPalOrderResponse
{
    public string id;
    public string status;
    public List<PayPalLink> links;
}

[System.Serializable]
public class PayPalLink
{
    public string rel;
    public string href;
}
