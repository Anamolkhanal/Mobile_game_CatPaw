using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer & Groups")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] private AudioClip missSfx;

    [Header("Exposed Param Names (Mixer)")]
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [Header("Auto Switch by Scene (optional)")]
    [SerializeField] private bool autoSwitchMusic = true;
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "SampleScene";

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private bool musicOn = true;
    private bool sfxOn = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 1f;
        if (musicGroup != null) musicSource.outputAudioMixerGroup = musicGroup;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        if (sfxGroup != null) sfxSource.outputAudioMixerGroup = sfxGroup;

        ApplyMixerMuteStates();

        if (autoSwitchMusic)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            // Handle current scene on startup
            ApplyMusicForActiveScene();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void PlayMenuMusic()
    {
        if (menuMusic == null) { Debug.LogWarning("AudioManager: menuMusic clip not assigned."); return; }
        if (musicSource.clip == menuMusic && musicSource.isPlaying) return;
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicForActiveScene();
    }

    private void ApplyMusicForActiveScene()
    {
        if (!autoSwitchMusic) return;
        var sceneName = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(menuSceneName) && sceneName == menuSceneName)
        {
            PlayMenuMusic();
            return;
        }
        if (!string.IsNullOrEmpty(gameSceneName) && sceneName == gameSceneName)
        {
            PlayGameMusic();
            return;
        }
    }

    public void PlayGameMusic()
    {
        if (gameMusic == null) { Debug.LogWarning("AudioManager: gameMusic clip not assigned."); return; }
        if (musicSource.clip == gameMusic && musicSource.isPlaying) return;
        musicSource.clip = gameMusic;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PlayHitSfx()
    {
        if (!sfxOn || hitSfx == null) return;
        sfxSource.PlayOneShot(hitSfx);
    }

    public void PlayMissSfx()
    {
        if (!sfxOn || missSfx == null) return;
        sfxSource.PlayOneShot(missSfx);
    }

    public void SetMusicOn(bool on)
    {
        musicOn = on;
        ApplyMixerMuteStates();
        if (!musicOn) StopMusic();
    }

    public void SetSfxOn(bool on)
    {
        sfxOn = on;
        ApplyMixerMuteStates();
    }

    public void ToggleMusic() => SetMusicOn(!musicOn);
    public void ToggleSfx() => SetSfxOn(!sfxOn);

    private void ApplyMixerMuteStates()
    {
        // Use mixer params if available; -80 dB as mute, 0 dB as full
        if (audioMixer != null)
        {
            if (!string.IsNullOrEmpty(musicVolumeParam))
            {
                float temp;
                bool has = audioMixer.GetFloat(musicVolumeParam, out temp);
                if (has)
                {
                    audioMixer.SetFloat(musicVolumeParam, musicOn ? 0f : -80f);
                }
                else
                {
                    Debug.LogWarning($"AudioMixer exposed param not found: {musicVolumeParam}. Make sure it's exposed in the mixer.");
                }
            }
            if (!string.IsNullOrEmpty(sfxVolumeParam))
            {
                float temp;
                bool has = audioMixer.GetFloat(sfxVolumeParam, out temp);
                if (has)
                {
                    audioMixer.SetFloat(sfxVolumeParam, sfxOn ? 0f : -80f);
                }
                else
                {
                    Debug.LogWarning($"AudioMixer exposed param not found: {sfxVolumeParam}. Make sure it's exposed in the mixer.");
                }
            }
        }
        else
        {
            // Fallback if no mixer assigned
            musicSource.mute = !musicOn;
            sfxSource.mute = !sfxOn;
        }
    }
}


