using System.Collections;
using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using TMPro;

public class DnaManager : MonoBehaviour
{
    private static DnaManager _instance;
    public static DnaManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DnaManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("DnaManager");
                    _instance = obj.AddComponent<DnaManager>();
                }
            }
            return _instance;
        }
    }

    private string _playerId;
    private IMongoCollection<BsonDocument> _playerCollection;
    private int _dnaCount;

    public int DnaCount => _dnaCount;
    public string PlayerId => _playerId;

    void Start()
    {
        var client = new MongoClient("mongodb+srv://db:db@cluster0.nth6c.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0");
        var database = client.GetDatabase("Server");
        _playerCollection = database.GetCollection<BsonDocument>("Player");

        StartCoroutine(WaitForIdAndFetchData());
    }

    private IEnumerator WaitForIdAndFetchData()
    {
        while (string.IsNullOrEmpty(AuthenticationManager.Instance.PlayerId))
        {
            yield return null;
        }

        _playerId = AuthenticationManager.Instance.PlayerId;
        Debug.Log("Player ID retrieved: " + _playerId);

        // Fetch data asynchronously
        Task.Run(async () => await FetchPlayerDataAsync());

        // Start polling the database in a separate async task
        Task.Run(async () => await PollDnaCountAsync());
    }

    private async Task FetchPlayerDataAsync()
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", _playerId);
            var playerDocument = await _playerCollection.Find(filter).FirstOrDefaultAsync();

            if (playerDocument != null)
            {
                Debug.Log("Player data fetched successfully.");
                _dnaCount = playerDocument.Contains("DnaCount") ? playerDocument["DnaCount"].AsInt32 : 0;
                UpdateDnaUI();
            }
            else
            {
                Debug.LogError("Player data not found in MongoDB.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error fetching player data: " + ex.Message);
        }
    }

    public async void AddDna(int amount)
    {
        if (string.IsNullOrEmpty(_playerId)) return;

        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", _playerId);
            var update = Builders<BsonDocument>.Update.Inc("DnaCount", amount);

            var result = await _playerCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                _dnaCount += amount;
                Debug.Log("DNA added successfully.");
                UpdateDnaUI();
            }
            else
            {
                Debug.LogError("Failed to add DNA.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error adding DNA: " + ex.Message);
        }
    }

    public async void SpendDnaForBuff(int cost)
    {
        if (_dnaCount >= cost)
        {
            // Deduct the DNA cost
            _dnaCount -= cost;

            // Update the player data in the database
            var filter = Builders<BsonDocument>.Filter.Eq("_id", _playerId);
            var update = Builders<BsonDocument>.Update.Set("DnaCount", _dnaCount);
            await _playerCollection.UpdateOneAsync(filter, update);

            Debug.Log($"Spent {cost}.");

            // Update the UI to reflect the new DNA count
            UpdateDnaUI();
        }
        else
        {
            Debug.LogError("Not enough DNA to purchase this buff.");
        }
    }

    private async Task PollDnaCountAsync()
    {
        while (true)
        {
            await Task.Delay(5000);
            await FetchPlayerDataAsync();
        }
    }

    [SerializeField] private TextMeshProUGUI dnaCountText;
    private void UpdateDnaUI()
    {
        if (dnaCountText != null)
        {
            dnaCountText.text = _dnaCount.ToString(); // Update the TMP text
        }
        
    }
}
