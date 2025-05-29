using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string androidAdUnitId;
    [SerializeField] string iOSAdUnitId;
    [SerializeField] bool isTesting = true;

    string adUnitId;
    void Awake()
    {
#if UNITY_IOS
        adUnitId = iOSAdUnitId;
#elif UNITY_ANDROID
        adUnitId = androidAdUnitId;
#endif
    }

    public void LoadInterstitialAd()
    {
        Advertisement.Load(adUnitId, this);
    }

    public void ShowInterstitialAd()
    {
        Advertisement.Show(adUnitId, this);
        LoadInterstitialAd();
    }

    #region ShowCallbacks
    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log($"Interstitial ad loaded successfully for placement: {placementId}");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Failed to load interstitial ad for placement {placementId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        throw new System.NotImplementedException();
    }

    public void OnUnityAdsShowStart(string placementId)
    {  }

    public void OnUnityAdsShowClick(string placementId)
    {  }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}