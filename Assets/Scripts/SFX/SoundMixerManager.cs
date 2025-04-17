using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour {
    public static SoundMixerManager Instance {
        get => singleton;
        set {
            if (value == null) {
                singleton = null;
            } else if (singleton == null) {
                singleton = value;
            } else if (singleton != value) {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(SoundMixerManager)}!");
            }
        }
    }
    private static SoundMixerManager singleton;
    [SerializeField] private AudioMixer mixer;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    public void SetMasterVolume(float volume) => mixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20f);
    public void SetSoundEffectsVolume(float volume) => mixer.SetFloat("soundEffectsVolume", Mathf.Log10(volume) * 20f);
    public void SetMusicVolume(float volume) => mixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20f);

}
