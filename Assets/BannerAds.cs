using System;
using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAds : MonoBehaviour
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

        Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);

        StartCoroutine(DisplayBannerAd());
    }

    IEnumerator DisplayBannerAd()
    {
        yield return new WaitForSeconds(1f);
        AdsManager.Instance.bannerAds.LoadBannerAd();
    }

    public void LoadBannerAd()
    {
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(adUnitId, options);
    }

    void OnBannerError(string message)
    {    }

    void OnBannerLoaded()
    {    }

    public void ShowBannerAd()
    {
        BannerOptions options = new BannerOptions
        {
            showCallback = BannerShown,
            clickCallback = BannerClicked,
            hideCallback = BannerHidden
        };
        Advertisement.Banner.Show(adUnitId, options);
    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
    }

    void BannerHidden()
    {    }

    void BannerClicked()
    {    }

    void BannerShown()
    {    }
}
