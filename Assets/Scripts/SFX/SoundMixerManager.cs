using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour {
    [SerializeField] private AudioMixer mixer;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void SetMasterVolume(float volume) => mixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20f);
    public void SetSoundEffectsVolume(float volume) => mixer.SetFloat("soundEffectsVolume", Mathf.Log10(volume) * 20f);
    public void SetMusicVolume(float volume) => mixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20f);

}
