using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;   

public class AdsManager : MonoBehaviour
{
#if UNITY_IOS
    string gameID = "5865239";
#else
    string gameID = "5865238";
#endif

    void Start()
    {
        Advertisement.Initialize(gameID); 
    }
}
