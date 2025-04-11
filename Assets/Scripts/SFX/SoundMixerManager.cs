using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour {
    [SerializeField] private AudioMixer mixer;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    public void SetMasterVolume(float volume) => mixer.SetFloat("masterVolume", volume);
    public void SetSoundEffectsVolume(float volume) => mixer.SetFloat("soundEffectsVolume", volume);
    public void SetMusicVolume(float volume) => mixer.SetFloat("musicVolume", volume);

}
