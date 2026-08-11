using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bgmClip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private void Start()
    {
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (audioSource == null || bgmClip == null)
        {
            return;
        }

        audioSource.clip = bgmClip;
        audioSource.volume = volume;
        audioSource.loop = true;

        audioSource.Play();
    }

    public void StopBGM()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}