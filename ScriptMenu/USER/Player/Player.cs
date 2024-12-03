using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using UnityEngine; // Needed for Application class

public class Player
{
    #region Documents
    public string _id { get; set; }
    public string Username { get; set; }
    public string GameName { get; set; }
    public string ShaPassword { get; private set; }
    public int DnaCount { get; set; }
    public bool IsBanned { get; set; }
    public int Points { get; set; } // New Points field
    public Dictionary<string, List<int>> Chapters { get; set; } // New Chapters field
    
    public List<Transaction> Transactions { get; set; }

    public class Transaction
    {
        public DateTime TransactionDate { get; set; }
        public int ItemIndex { get; set; }
        public int DnaChange { get; set; }
    }
    #endregion

    #region Dna
    // Method to add random DNA count between 10 and 50
    public void AddRandomDna()
    {
        System.Random random = new System.Random(); // Specify System.Random to avoid ambiguity
        int randomAmount = random.Next(10, 51); // Generates a random number between 10 and 50
        DnaCount += randomAmount;
    }
    #endregion

    #region ID
    // Method to generate a custom 12-character alphanumeric ID
    private string GenerateCustomId(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random(); // Specify System.Random to avoid ambiguity
        var stringBuilder = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(chars[random.Next(chars.Length)]);
        }

        return stringBuilder.ToString();
    }
    #endregion

    #region Create
    // Example CRUD Methods
    public static async Task CreatePlayer(IMongoCollection<Player> playerCollection, Player player)
    {
        await playerCollection.InsertOneAsync(player);
    }

    public static async Task<Player> GetPlayerByUsername(IMongoCollection<Player> playerCollection, string username)
    {
        return await playerCollection.Find(p => p.Username == username).FirstOrDefaultAsync();
    }

    public static async Task UpdateDnaCount(IMongoCollection<Player> playerCollection, ObjectId id, int newDnaCount)
    {
        var filter = Builders<Player>.Filter.Eq("_id", id);
        var update = Builders<Player>.Update.Set("DnaCount", newDnaCount);
        await playerCollection.UpdateOneAsync(filter, update);
    }
    #endregion

    #region HashPassword
    // Hash the password using SHA256
    public static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2")); // Convert byte to hex string
            }
            return builder.ToString();
        }
    }
    #endregion

    public Player(string username, string gameName, string password)
    {
        Username = username;
        GameName = gameName;
        ShaPassword = password;  // You may want to hash the password here
        DnaCount = 0; // Initialize DnaCount or any other default values
        Points = 0; // Initialize Points with default value
        _id = GenerateCustomId(12); // Generate a unique custom ID

        // Initialize Chapters with sample data
        Chapters = new Dictionary<string, List<int>>()
        {
            { "ch1", new List<int> { 1, 2, 3, 4, 5 } },
            { "ch2", new List<int> { 1, 2, 3, 4, 5 } },
            { "ch3", new List<int> { 1, 2, 3, 4, 5 } }
        };
    }

    #region Register Task
    public static async Task<bool> RegisterPlayer(IMongoCollection<Player> playerCollection, string username, string gameName, string password)
    {
        // Check if the username already exists
        var existingPlayer = await playerCollection.Find(p => p.Username == username).FirstOrDefaultAsync();
        if (existingPlayer != null)
        {
            return false; // Username already exists
        }

        // Create a new player
        var hashedPassword = HashPassword(password);
        Player newPlayer = new Player(username, gameName, hashedPassword);

        // Insert the new player into the database
        await playerCollection.InsertOneAsync(newPlayer);
        return true; // Registration successful
    }
    #endregion

    #region Login Task
    // Login method
    public static async Task<Player> Login(IMongoCollection<Player> playerCollection, string username, string password)
    {
        // Define filter for username
        var filter = Builders<Player>.Filter.Eq(p => p.Username, username);

        // Retrieve player by username
        var player = await playerCollection.Find(filter).FirstOrDefaultAsync();

        if (player == null)
        {
            Debug.LogWarning($"Player {username} not found.");
            return null; // Player not found
        }

        // Check if the player is banned
        if (player.IsBanned)
        {
            Debug.LogWarning($"Player {username} is banned and cannot log in.");
            return null; // Player is banned
        }

        // Verify the password
        if (player.ShaPassword == HashPassword(password))
        {
            return player; // Login successful
        }

        Debug.LogWarning("Incorrect password.");
        return null; // Invalid password
    }
    #endregion

    #region Update Existing Data (Add New Fields for Existing Players)
    // Update existing players to add new fields (Chapters and Points)
    public static async Task AddNewFieldsToExistingPlayers(IMongoCollection<Player> playerCollection)
    {
        // Define the update operation to add the new fields with default values
        var update = Builders<Player>.Update
            .Set(p => p.Points, 0) // Default value for Points
            .Set(p => p.Chapters, new Dictionary<string, List<int>>()
            {
                { "ch1", new List<int> { 1, 2, 3, 4, 5 } },
                { "ch2", new List<int> { 1, 2, 3, 4, 5 } },
                { "ch3", new List<int> { 1, 2, 3, 4, 5 } }
            }); // Default chapters

        // Update all players that do not have these fields
        await playerCollection.UpdateManyAsync(
            Builders<Player>.Filter.Exists("Chapters", false), // Only update players without "Chapters" field
            update
        );
    }
    #endregion

    // Check for internet connection
    private static bool IsConnectedToInternet()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}
