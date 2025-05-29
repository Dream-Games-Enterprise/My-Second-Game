using UnityEngine;
using UnityEngine.Advertisements;

public class InitialiseAds : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string androidGameID;
    [SerializeField] string iOSGameID;

    string gameID;

    public void InitialiseAdsNow()
    {
#if UNITY_IOS
            gameID = iOSGameID;
#elif UNITY_ANDROID
        gameID = androidGameID;
#elif UNITY_EDITOR
            gameID = "1234567";
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
            Advertisement.Initialize(gameID, true, this);
    }
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads Initialization Complete");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}
