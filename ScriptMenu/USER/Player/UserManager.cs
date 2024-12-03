using System.Collections;
using UnityEngine;
using TMPro;
using MongoDB.Driver;
using MongoDB.Bson;

public class UserManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI playerIdText;

    private string _playerId;
    private MongoClient _mongoClient;
    private IMongoDatabase _database;
    private IMongoCollection<BsonDocument> _playerCollection;

    private void Start()
    {
        // Initialize MongoDB connection
        string connectionString = "mongodb+srv://db:db@cluster0.nth6c.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
        _mongoClient = new MongoClient(connectionString);
        _database = _mongoClient.GetDatabase("Server");
        _playerCollection = _database.GetCollection<BsonDocument>("Player");

        // Start coroutine to fetch and display data
        StartCoroutine(WaitForIdAndFetchData());
    }

    private IEnumerator WaitForIdAndFetchData()
    {
        while (string.IsNullOrEmpty(AuthenticationManager.Instance.PlayerId))
        {
            yield return null; // Wait until PlayerId is available
        }

        _playerId = AuthenticationManager.Instance.PlayerId;
        Debug.Log("Player ID retrieved: " + _playerId);

        // Fetch player data from MongoDB
        yield return FetchPlayerData();
    }

    private IEnumerator FetchPlayerData()
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", _playerId);
        var playerDataTask = _playerCollection.Find(filter).FirstOrDefaultAsync();

        yield return new WaitUntil(() => playerDataTask.IsCompleted); // Wait for the query to complete

        if (playerDataTask.Result != null)
        {
            var playerData = playerDataTask.Result;
            string username = playerData.Contains("Username") ? playerData["Username"].AsString : "Unknown";

            // Update UI
            usernameText.text = "Username: " + username;
            playerIdText.text = "ID: " + _playerId;
        }
        else
        {
            Debug.LogError("Player not found in the database.");
        }
    }
}
