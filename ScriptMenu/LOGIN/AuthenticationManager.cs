using UnityEngine;
using TMPro; 
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;



public class AuthenticationManager : MonoBehaviour
{
    // Singleton instance to make AuthenticationManager accessible globally
    public static AuthenticationManager Instance { get; set; }

    // Registration and Login UI Elements
    [Header("Registration UI")]
    public TMP_InputField registerUsernameInputField;
    public TMP_InputField registerGameNameInputField;
    public TMP_InputField registerPasswordInputField;

    [Header("Login UI")]
    public TMP_InputField loginUsernameInputField;
    public TMP_InputField loginPasswordInputField;

    [Header("Error Text")]
    public TMP_Text LoginFeedbackText;
    public TMP_Text RegisterFeedbackText;

     // Change Password UI
    [Header("Change Password UI")]
    public TMP_InputField changePasswordUsernameInputField;
    public TMP_InputField oldPasswordInputField;
    public TMP_InputField newPasswordInputField;
    public TMP_InputField confirmPasswordInputField;
    public TMP_Text changePasswordFeedbackText;
    public GameObject changePasswordPanel;

    

    public LoadingManager loadingmanager;

    private IMongoCollection<Player> playerCollection;
    private MongoClient mongoClient;

    // Property to store the player's _id for DnaManager access
    public string PlayerId { get; private set; }

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        
        
        // Check for internet connection at the start of the game
        if (!IsConnectedToInternet())
        {
            ShowErrorAndExit("No internet connection. Please connect to the internet to play.");
            return;
        }

        InitializeMongoDBConnection();
        InitializeInputFields();
        HideFeedbackMessages();
        
    }

    private void ShowErrorAndExit(string message)
    {
        // Display error message to the player
        ShowFeedback(LoginFeedbackText, message, Color.red);
        Application.Quit(); // Exit the game if no internet connection
    }
    
    #region Initialize
    private void InitializeMongoDBConnection()
    {
        // Replace with your MongoDB connection string
        string connectionString = "mongodb+srv://db:db@cluster0.nth6c.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
        mongoClient = new MongoClient(connectionString);

        try
        {
            // Attempt to get the database and collection to ensure the connection is established
            var database = mongoClient.GetDatabase("Server");
            playerCollection = database.GetCollection<Player>("Player");
            ShowFeedback(RegisterFeedbackText, "Connected to MongoDB.", Color.green);
        }
        catch (MongoConnectionException ex)
        {
            ShowFeedback(RegisterFeedbackText, "MongoDB connection failed: " + ex.Message, Color.red);
            playerCollection = null; // Set to null to avoid further calls
            Application.Quit(); // Exit the game if the connection fails
        }
    }
    

    private void InitializeInputFields()
    {
        registerPasswordInputField.contentType = TMP_InputField.ContentType.Password;
        loginPasswordInputField.contentType = TMP_InputField.ContentType.Password;
        registerPasswordInputField.ForceLabelUpdate();
        loginPasswordInputField.ForceLabelUpdate();
    }

    private void HideFeedbackMessages()
    {
        LoginFeedbackText.gameObject.SetActive(false);
        RegisterFeedbackText.gameObject.SetActive(false);
    }
    #endregion

    public void OnRegisterButtonClick()
    {
        if (!IsMongoDBConnected())
        {
            ShowFeedback(RegisterFeedbackText, "MongoDB not connected. Please check the connection.", Color.red);
            return;
        }

        if (!IsConnectedToInternet())
        {
            ShowFeedback(RegisterFeedbackText, "No internet connection. Please connect and try again.", Color.red);
            return;
        }

        string username = registerUsernameInputField.text;
        string gameName = registerGameNameInputField.text;
        string password = registerPasswordInputField.text;

        if (IsInputValid(username, gameName, password))
        {
          
        if (IsRegistrationValid(username, gameName, password))
        {
            // Proceed with registration
           RegisterPlayer(username, gameName, password);
        }
         
            

        }
        else
        {
            ShowFeedback(RegisterFeedbackText, "Please fill in all fields for registration.", Color.red);
        }
    }

        private bool IsRegistrationValid(string username, string gameName, string password)
{
    // Check username length
    if (username.Length < 8 || username.Length > 14)
    {
        Debug.Log("Username length validation failed.");
        ShowFeedback(RegisterFeedbackText, "Username must be between 8 and 14 characters.", Color.red);
        return false;
    }

    // Regex pattern to allow only letters, numbers, and underscores in the username
    Regex usernamePattern = new Regex(@"^[a-zA-Z0-9_]+$");
    if (!usernamePattern.IsMatch(username))
    {
        Debug.Log("Username contains invalid characters.");
        ShowFeedback(RegisterFeedbackText, "Username can only contain letters, numbers, and underscores.", Color.red);
        return false;
    }

    // Check game name length
    if (gameName.Length < 8 || gameName.Length > 14)
    {
        Debug.Log("Game name length validation failed.");
        ShowFeedback(RegisterFeedbackText, "Game name must be between 8 and 14 characters.", Color.red);
        return false;
    }

    // Password validation (same as before)
    Regex passwordPattern = new Regex(@"^(?=(.*[A-Z]){2})(?=(.*\d){2})(?=(.*[!@#$%^&*.\-]){1}).{12,24}$");
    if (!passwordPattern.IsMatch(password))
    {
        Debug.Log("Password pattern validation failed.");
        ShowFeedback(RegisterFeedbackText, "Password must be 12-24 characters, with 2 uppercase letters, 2 numbers, and 1 special character.", Color.red);
        return false;
    }

    return true;
}



    public void OnLoginButtonClick()
{
    if (!IsMongoDBConnected())
    {
        ShowFeedback(LoginFeedbackText, "MongoDB not connected. Please check the connection.", Color.red);
        return;
    }

    if (!IsConnectedToInternet())
    {
        ShowFeedback(LoginFeedbackText, "No internet connection. Please connect and try again.", Color.red);
        return;
    }

    string username = loginUsernameInputField.text;
    string password = loginPasswordInputField.text;

    if (IsInputValid(username, string.Empty, password)) // gameName can be empty for login
    {
        // Regex pattern to allow only letters, numbers, and underscores in the username
        Regex usernamePattern = new Regex(@"^[a-zA-Z0-9_]+$");
        if (!usernamePattern.IsMatch(username))
        {
            ShowFeedback(LoginFeedbackText, "Username can only contain letters, numbers, and underscores.", Color.red);
            return;
        }

        LoginPlayer(username, password);
    }
    else
    {
        ShowFeedback(LoginFeedbackText, "Please fill in all fields for login.", Color.red);
    }
}

    private bool IsMongoDBConnected()
    {
        return playerCollection != null;
    }

    private bool IsInputValid(string username, string gameName, string password)
    {
        return !string.IsNullOrWhiteSpace(username) &&
               (string.IsNullOrWhiteSpace(gameName) || !string.IsNullOrWhiteSpace(gameName)) &&
               !string.IsNullOrWhiteSpace(password);
    }

    #region Register
    private async void RegisterPlayer(string username, string gameName, string password)
    {
        bool success = await Player.RegisterPlayer(playerCollection, username, gameName, password);
        if (success)
        {
            ShowFeedback(RegisterFeedbackText, "Registration successful!", Color.green);
            var newlyRegisteredPlayer = await playerCollection.Find(p => p.Username == username).FirstOrDefaultAsync();
            if (newlyRegisteredPlayer != null)
            {
                SceneManager.LoadScene("MainMenu");
                
                
                
                // Set PlayerId to be accessed by DnaManager
                PlayerId = newlyRegisteredPlayer._id.ToString();
                
            }
            
        }
        else
        {
            ShowFeedback(RegisterFeedbackText, "Username already exists.", Color.red);
        }
    }
    #endregion


    #region Change Password

    public async void OnChangePasswordButtonClick()
    {
        string username = changePasswordUsernameInputField.text;
        string oldPassword = oldPasswordInputField.text;
        string newPassword = newPasswordInputField.text;
        string confirmPassword = confirmPasswordInputField.text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            ShowFeedback(changePasswordFeedbackText, "All fields are required.", Color.red);
            return;
        }

        if (newPassword != confirmPassword)
        {
            ShowFeedback(changePasswordFeedbackText, "New password and confirm password do not match.", Color.red);
            return;
        }

        Regex passwordPattern = new Regex(@"^(?=(.*[A-Z]){2})(?=(.*\d){2})(?=(.*[!@#$%^&*.\-]){1}).{12,24}$");
        if (!passwordPattern.IsMatch(newPassword))
        {
            ShowFeedback(changePasswordFeedbackText, "New password must be 12-24 characters, with 2 uppercase letters, 2 numbers, and 1 special character.", Color.red);
            return;
        }

        var filter = Builders<Player>.Filter.Eq(p => p.Username, username);
        var player = await playerCollection.Find(filter).FirstOrDefaultAsync();

        if (player == null)
        {
            ShowFeedback(changePasswordFeedbackText, "Username not found.", Color.red);
            return;
        }

        if (player.ShaPassword != Player.HashPassword(oldPassword))
        {
            ShowFeedback(changePasswordFeedbackText, "Old password is incorrect.", Color.red);
            return;
        }

        // Update password in the database
        var update = Builders<Player>.Update.Set(p => p.ShaPassword, Player.HashPassword(newPassword));
        await playerCollection.UpdateOneAsync(filter, update);

        ShowFeedback(changePasswordFeedbackText, "Password changed successfully!", Color.green);
        
        ClearChangePasswordFields();
        changePasswordPanel.SetActive(false);
    }

    public async void OnChangePasswordUsernameFieldChange()
    {
        string username = changePasswordUsernameInputField.text;

        if (string.IsNullOrWhiteSpace(username))
        {
            SetInputFieldColor(changePasswordUsernameInputField, Color.white);
            return;
        }

        var filter = Builders<Player>.Filter.Eq(p => p.Username, username);
        var player = await playerCollection.Find(filter).FirstOrDefaultAsync();

        if (player != null)
        {
            SetInputFieldColor(changePasswordUsernameInputField, Color.green);
        }
        else
        {
            SetInputFieldColor(changePasswordUsernameInputField, Color.red);
        }
    }

    public async void OnOldPasswordFieldChange()
    {
        string username = changePasswordUsernameInputField.text;
        string oldPassword = oldPasswordInputField.text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(oldPassword))
        {
            SetInputFieldColor(oldPasswordInputField, Color.white);
            return;
        }

        var filter = Builders<Player>.Filter.Eq(p => p.Username, username);
        var player = await playerCollection.Find(filter).FirstOrDefaultAsync();

        if (player != null && player.ShaPassword == Player.HashPassword(oldPassword))
        {
            SetInputFieldColor(oldPasswordInputField, Color.green);
        }
        else
        {
            SetInputFieldColor(oldPasswordInputField, Color.red);
        }
    }

    private void SetInputFieldColor(TMP_InputField inputField, Color color)
    {
        inputField.textComponent.color = color;
    }

    private void ClearChangePasswordFields()
    {
        changePasswordUsernameInputField.text = "";
        oldPasswordInputField.text = "";
        newPasswordInputField.text = "";
        confirmPasswordInputField.text = "";
    }

    #endregion


    #region Login
    private async void LoginPlayer(string username, string password)
    {
        Player loggedInPlayer = await Player.Login(playerCollection, username, password);
        
        if (loggedInPlayer == null)
        {
            var filter = Builders<Player>.Filter.Eq(p => p.Username, username);
            var player = await playerCollection.Find(filter).FirstOrDefaultAsync();

            if (player != null && player.IsBanned)
            {
                ShowFeedback(LoginFeedbackText, "Your account is banned. Please contact support.", Color.red);
                return;
            }

            ShowFeedback(LoginFeedbackText, "Invalid username or password.", Color.red);
            return;
        }

        playerUsername = loggedInPlayer.Username;

        // Successful login
        ShowFeedback(LoginFeedbackText, $"Login successful! Welcome, {loggedInPlayer.GameName}", Color.green);

        await Player.AddNewFieldsToExistingPlayers(playerCollection);
        // Set PlayerId to be accessed by DnaManager
        PlayerId = loggedInPlayer._id.ToString();
        
       

        SceneManager.LoadScene("MainMenu");
        ClearInputFields();
    }
    #endregion

    private void ClearInputFields()
    {
        registerUsernameInputField.text = "";
        registerGameNameInputField.text = "";
        registerPasswordInputField.text = "";
        loginUsernameInputField.text = "";
        loginPasswordInputField.text = "";
    }


    private bool IsConnectedToInternet()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    private void ShowFeedback(TMP_Text feedbackText, string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);
    }

    #region Banning

    private Coroutine banCheckCoroutine;
    private string playerUsername; // Ensure this is set when a player logs in

    private void OnEnable()
    {
        if (banCheckCoroutine == null)
        {
            banCheckCoroutine = StartCoroutine(CheckBanStatusPeriodically());
        }
    }

    private void OnDisable()
    {
        if (banCheckCoroutine != null)
        {
            StopCoroutine(banCheckCoroutine);
            banCheckCoroutine = null;
        }
    }

    private IEnumerator CheckBanStatusPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);

            bool isBanned = false;
            var checkBanTask = CheckIfBanned(playerUsername);

            while (!checkBanTask.IsCompleted)
            {
                yield return null;
            }

            isBanned = checkBanTask.Result;

            if (isBanned)
            {
                SwitchToLoginPanel();
                yield break;
            }
        }
    }

    private void SwitchToLoginPanel()
    {
       
        SceneManager.LoadScene("Login");
        ShowFeedback(LoginFeedbackText, "Your account has been banned. Please contact support.", Color.red);
    }

    private async Task<bool> CheckIfBanned(string username)
    {
        var filter = Builders<Player>.Filter.Eq(p => p.Username, username);
        var player = await playerCollection.Find(filter).FirstOrDefaultAsync();

        return player?.IsBanned ?? false;
    }
    #endregion

        // New Method: GetPlayerCollection to share collection with DnaManager
        public IMongoCollection<Player> GetPlayerCollection()
        {
            return playerCollection;
        }
    


 
}
