using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
    public AudioClip foodPickupClip;
    public AudioClip gameOverClip;
    public AudioClip buttonClickClip;

    void Awake()
    {
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

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayFoodPickup()
    {
        if (foodPickupClip != null)
        {
            sfxSource.pitch = Random.Range(0.92f, 1.08f);
            sfxSource.clip = foodPickupClip;
            sfxSource.Play();
        }
    }

    public void PlayFlash()
    {
        
    }

    public void PlayGameOver() => PlaySFX(gameOverClip);
    public void PlayButtonClick() => PlaySFX(buttonClickClip);
}
