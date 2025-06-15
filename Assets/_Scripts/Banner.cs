using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine;

public class Banner : MonoBehaviour
{
    private BannerView _bannerView;

#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-8589584410755755/3560727599"; 
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-8589584410755755/4944277242";
#else
    private string _adUnitId = "unused";
#endif

    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Mobile Ads SDK initialized.");
            CreateAndLoadBanner();
        });
    }

    void CreateAndLoadBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }

        _bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Top);

        AdRequest request = new AdRequest();
        _bannerView.LoadAd(request);
    }

    void OnDestroy()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
        }
    }

    public void HideBanner()
    {
        _bannerView?.Hide();
    }

    public void ShowBanner()
    {
        _bannerView?.Show();
    }
}
