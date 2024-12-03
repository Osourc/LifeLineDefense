using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;

public class PhotonConnection : MonoBehaviourPunCallbacks
{
    public static PhotonConnection Instance {get; set;}

    public string[] mapPool;
    public TextMeshProUGUI roomIDText;       // Text to display Room ID
    public TextMeshProUGUI playersListText;  // Text to display players in the room
    public Button createButton;             // Button to create room / start game
    public Button joinButton;               // Button to join room
    public TMP_InputField roomIDInputField; // Input field for Room ID
    public Button leaveRoomButton;          // Button to leave the room
    public GameObject joinPanel;            // Panel for joining a room
    public GameObject createPanel;          // Panel for creating or starting a room
    public GameObject lobbyPanel;           // Panel holding lobby options

    private string _playerId;
    private string _playerGameName;         // Variable to store the player's GameName
    private IMongoCollection<BsonDocument> _playerCollection;

    public bool ManuallyDisconnected = false;

    private void Awake() 
    {
        // Singleton pattern: Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make persistent across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }
    }

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        // Add listeners to buttons
        createButton.onClick.AddListener(OnCreateOrStartButtonClicked);
        joinButton.onClick.AddListener(JoinRoom);
        leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);

        // Disable the Leave Room button initially
        leaveRoomButton.interactable = false;

        // Connect to Photon master server
        createButton.interactable = false; // Disabled until connected
        StartCoroutine(SetPlayerNickname()); // Start setting the nickname
        PhotonNetwork.ConnectUsingSettings();
    }

    private IEnumerator SetPlayerNickname()
    {
        // Initialize MongoDB connection
        var client = new MongoClient("mongodb+srv://db:db@cluster0.nth6c.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0");
        var database = client.GetDatabase("Server");
        _playerCollection = database.GetCollection<BsonDocument>("Player");

        // Wait until PlayerId is available
        while (string.IsNullOrEmpty(AuthenticationManager.Instance.PlayerId))
        {
            yield return null;
        }

        // Set the PlayerId as NickName
        _playerId = AuthenticationManager.Instance.PlayerId;
        yield return StartCoroutine(FetchPlayerGameName());

        if (!string.IsNullOrEmpty(_playerGameName))
        {
            PhotonNetwork.NickName = _playerGameName; // Set the Player's GameName as the Photon NickName
            Debug.Log("Player GameName set as nickname: " + _playerGameName);
        }
        else
        {
            PhotonNetwork.NickName = _playerId; // Fallback to PlayerId if no GameName found
            Debug.LogError("Player GameName not found for Player ID: " + _playerId);
        }
    }

    private IEnumerator FetchPlayerGameName()
{
    bool queryCompleted = false;

    // MongoDB query in a separate task to avoid blocking Unity's main thread
    System.Threading.Tasks.Task.Run(() =>
    {
        try
        {
            // Log the PlayerId (which is the MongoDB _id we're using to search)
            Debug.Log("Searching for PlayerId (Mongo _id): " + _playerId);

            // Query the collection using the '_id' field (since _id is a string)
            var filter = Builders<BsonDocument>.Filter.Eq("_id", _playerId);
            var playerDocument = _playerCollection.Find(filter).FirstOrDefault();

            // If the player is found and contains a "Gamename" field, set the game name
            if (playerDocument != null && playerDocument.Contains("GameName"))
            {
                _playerGameName = playerDocument["GameName"].AsString;
                Debug.Log("Player GameName found: " + _playerGameName);
            }
            else
            {
                if (playerDocument == null)
                {
                    Debug.LogError("Player with ID " + _playerId + " not found.");
                }
                else
                {
                    Debug.LogError("Player found, but 'Gamename' is missing.");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error fetching Player GameName: " + ex.Message);
        }
        queryCompleted = true;
    });

    // Wait until the query is completed
    while (!queryCompleted)
    {
        yield return null;
    }
}



    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon!");
        createButton.interactable = true; // Enable the button when connected
    }

    public void OnCreateOrStartButtonClicked()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            CreateRoom(); // Create a new room
        }
        else
        {
            StartGame(); // Start the game if already in a room
        }
    }

    public void CreateRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (string.IsNullOrEmpty(PhotonNetwork.NickName))
            {
                Debug.LogError("Nickname is not set yet. Please wait.");
                return;
            }

            string roomName = "LLD" + Random.Range(1000, 9999).ToString();
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
            PhotonNetwork.CreateRoom(roomName, roomOptions);

            roomIDText.text = "Room ID: " + roomName;
            createButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start"; // Set button to "Start"
        }
        else
        {
            Debug.LogError("Cannot create room. Client is not ready.");
        }
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            Debug.LogError("Nickname is not set yet. Please wait.");
            return;
        }

        string roomID = roomIDInputField.text;
        PhotonNetwork.JoinRoom(roomID);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        ShowPlayerGameNames();

        // Hide join panel and show create panel
        joinPanel.SetActive(false);
        createPanel.SetActive(true);
        lobbyPanel.SetActive(true);

        // Show "Start" button only if there are exactly 2 players
        createButton.gameObject.SetActive(PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient);

        // Enable the Leave Room button now that the player is in a room
        leaveRoomButton.interactable = true;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} joined the room.");
        ShowPlayerGameNames();

        // Enable "Start" button only if there are exactly 2 players
        createButton.gameObject.SetActive(PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} left the room.");
        ShowPlayerGameNames();

        // If Player 1 leaves, transfer ownership to the next player
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length > 0)
        {
            createButton.gameObject.SetActive(true); // New Player 1 sees "Start" button
        }

        UpdateUIPanelsAfterPlayerLeft();

        // Disable "Start" button if there are fewer than 2 players
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            createButton.gameObject.SetActive(false);
        }
    }

    public void OnLeaveRoomButtonClicked()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); // Leave the current room
            Debug.Log("Leaving the room...");
        }
        else
        {
            Debug.LogError("Not in a room to leave!");
            createButton.interactable = true; //************Newly Added to create a room after leaving
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left Room.");

        // Reset the UI elements
        roomIDText.text = "CREATE ROOM";
        playersListText.text = "";
        createButton.GetComponentInChildren<TextMeshProUGUI>().text = "Create Room";

        // Update UI panels to show lobby
        joinPanel.SetActive(false);
        createPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        // Disable the Leave Room button when leaving the room
        leaveRoomButton.interactable = false;

        // Ensure the "Create Room" button is visible when leaving
        createButton.gameObject.SetActive(true);
        createButton.interactable = true;
        

        //Ensure reconnection to the master server if disconnected 
        if (!ManuallyDisconnected && !PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Reconnecting to Master Server...");
            PhotonNetwork.ConnectUsingSettings(); // Reconnect if disconnected
        }

        ManuallyDisconnected = false;
    }

    private void UpdateUIPanelsAfterPlayerLeft()
    {
        if (PhotonNetwork.PlayerList.Length == 0)
        {
            joinPanel.SetActive(false);
            createPanel.SetActive(false);
            lobbyPanel.SetActive(true);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                joinPanel.SetActive(false);
                createPanel.SetActive(true);
                lobbyPanel.SetActive(false);
            }
            else
            {
                joinPanel.SetActive(false);
                createPanel.SetActive(false);
                lobbyPanel.SetActive(false);
            }
        }

        createButton.GetComponentInChildren<TextMeshProUGUI>().text = "Create Room";
    }

    private void ShowPlayerGameNames()
    {
        string playersList = "Players in Room:\n";
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            playersList += player.NickName + "\n";
        }
        playersListText.text = playersList;
    }

    private void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string selectedMap = mapPool[Random.Range(0, mapPool.Length)];
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            Debug.Log("Game Started!");
            PhotonNetwork.LoadLevel(selectedMap);
        }
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}