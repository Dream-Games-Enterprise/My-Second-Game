using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
    public AudioClip foodPickupClip;
    public AudioClip gameOverClip;
    public AudioClip buttonClickClip;
    public AudioClip flashStartClip;
    public AudioClip deathExplosionClip;

    bool soundEnabled = true;

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
        if (!soundEnabled || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
    }


    public void PlayFoodPickup()
    {
        if (!soundEnabled || foodPickupClip == null) return;

        sfxSource.pitch = Random.Range(0.92f, 1.08f);
        sfxSource.clip = foodPickupClip;
        sfxSource.Play();
    }

    public void PlayFlash()
    {
        if (!soundEnabled || flashStartClip == null) return;

        if (flashStartClip != null)
            PlaySFX(flashStartClip);
    }

    public void PlayDeathExplosion()
    {
        if (!soundEnabled || deathExplosionClip == null) return;

        if (deathExplosionClip != null)
            PlaySFX(deathExplosionClip);
    }

    public void PlayGameOver() => PlaySFX(gameOverClip);
    public void PlayButtonClick() => PlaySFX(buttonClickClip);
}
