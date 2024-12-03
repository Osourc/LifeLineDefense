using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource backgroundMusicSource;
    public AudioSource tapSoundSource;

    [Header("UI Elements")]
    public Toggle musicToggle;
    public Toggle effectsToggle;
    public Slider musicVolumeSlider;
    public Slider effectsVolumeSlider;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI effectsVolumeText;

    [Header("Colors")]
    public Color onColor = Color.green;  // Green for "ON"
    public Color offColor = Color.red;   // Red for "OFF"

    private Image musicToggleBackground;
    private Image effectsToggleBackground;

    private string mainMenuSceneName = "MainMenu"; // Change this to your actual Main Menu scene name

    private void Awake()
    {
        // Singleton pattern to ensure only one SoundManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Keep the SoundManager across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Set initial values: 100% volume at start
        musicVolumeSlider.value = 1.0f;  // 100% volume
        effectsVolumeSlider.value = 1.0f; // 100% volume

        // Get the background images of the toggles
        musicToggleBackground = musicToggle.GetComponentInChildren<Image>();
        effectsToggleBackground = effectsToggle.GetComponentInChildren<Image>();

        // Initialize UI elements
        UpdateMusicState();
        UpdateEffectsState();
        UpdateMusicVolume();
        UpdateEffectsVolume();

        // Add listeners to handle checkbox and slider changes
        musicToggle.onValueChanged.AddListener(delegate { ToggleMusic(); });
        effectsToggle.onValueChanged.AddListener(delegate { ToggleEffects(); });
        musicVolumeSlider.onValueChanged.AddListener(delegate { UpdateMusicVolume(); });
        effectsVolumeSlider.onValueChanged.AddListener(delegate { UpdateEffectsVolume(); });

        // Subscribe to scene change events
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene change events
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.name == mainMenuSceneName)
        {
            // Resume background music in the main menu
            if (!backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.Play();
            }
        }
        else
        {
            // Stop background music in other scenes
            backgroundMusicSource.Stop();
        }
    }

    private void Update()
    {
        DetectTap();
    }

    // Detect touch or mouse click to trigger tap sound once
    private void DetectTap()
    {
        if ((Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) && effectsToggle.isOn)
        {
            PlayTapSoundOnce();
        }
    }

    // Play tap sound once to avoid doubling
    private void PlayTapSoundOnce()
    {
        if (!tapSoundSource.isPlaying)  // Ensure only one tap sound plays at a time
        {
            tapSoundSource.PlayOneShot(tapSoundSource.clip);
        }
    }

    // Play background music
    public void PlayBackgroundMusic()
    {
        backgroundMusicSource.Play();
    }

    // Toggle the background music on/off based on checkbox
    public void ToggleMusic()
    {
        backgroundMusicSource.mute = !musicToggle.isOn;
        UpdateMusicState();
    }

    // Toggle the sound effects on/off based on checkbox
    public void ToggleEffects()
    {
        tapSoundSource.mute = !effectsToggle.isOn;
        UpdateEffectsState();
    }

    // Update background music volume based on slider value
    public void UpdateMusicVolume()
    {
        backgroundMusicSource.volume = musicVolumeSlider.value;
        musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100).ToString() + "%";
    }

    // Update sound effects volume based on slider value
    public void UpdateEffectsVolume()
    {
        tapSoundSource.volume = effectsVolumeSlider.value;
        effectsVolumeText.text = Mathf.RoundToInt(effectsVolumeSlider.value * 100).ToString() + "%";
    }

    // Update the state of the background music mute/unmute
    private void UpdateMusicState()
    {
        musicToggle.isOn = !backgroundMusicSource.mute;
        UpdateToggleColor(musicToggle, musicToggle.isOn);
    }

    // Update the state of the sound effects mute/unmute
    private void UpdateEffectsState()
    {
        effectsToggle.isOn = !tapSoundSource.mute;
        UpdateToggleColor(effectsToggle, effectsToggle.isOn);
    }

    // Update the toggle's background color based on its state
    private void UpdateToggleColor(Toggle toggle, bool isOn)
    {
        Image toggleBackground = toggle.GetComponentInChildren<Image>();
        toggleBackground.color = isOn ? onColor : offColor;
    }
}
