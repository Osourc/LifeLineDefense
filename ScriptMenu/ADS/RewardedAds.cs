using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using TMPro; // Import the TMPro namespace

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string androidAdUnitId;
    [SerializeField] private string iosAdUnitId;
    
    // Reference to the TextMeshProUGUI component to display DNA count
    [SerializeField] private TextMeshProUGUI dnaCountText;
     [SerializeField] private TextMeshProUGUI dnaCountText2;

    private string adUnitId;

    private void Awake()
    {
        #if UNITY_IOS
            adUnitId = iosAdUnitId;
        #elif UNITY_ANDROID
            adUnitId = androidAdUnitId;
        #endif
    }

    public void LoadRewardedAd()
    {
        Advertisement.Load(adUnitId, this);
    }

    public void ShowRewardedAd()
    {
        Advertisement.Show(adUnitId, this);
        LoadRewardedAd();
    }

    #region LoadCallbacks
    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Interstitial Ad Loaded");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message) { }
    #endregion

    #region ShowCallbacks
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message) { }

    public void OnUnityAdsShowStart(string placementId) { }

    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == adUnitId && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            DnaManager.Instance.AddDna(10);
            Debug.Log("Added 10 Dna");
            UpdateDnaCountDisplay(); // Update the display after adding DNA
        }
    }
    #endregion

    // Method to update the displayed DNA count
    private void UpdateDnaCountDisplay()
    {
        if (dnaCountText != null)
        {
            dnaCountText.text = "DNA Count: " + DnaManager.Instance.DnaCount; // Display the DNA count
            dnaCountText2.text = "DNA Count: " + DnaManager.Instance.DnaCount; // Display the DNA count
        }
    }
}
