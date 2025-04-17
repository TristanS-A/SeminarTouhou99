using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance {
        get => singleton;
        set {
            if (value == null) {
                singleton = null;
            } else if (singleton == null) {
                singleton = value;
            } else if (singleton != value) {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(SoundManager)}!");
            }
        }
    }
    private static SoundManager singleton;
    [SerializeField] private AudioSource sfx;

    private void Awake() {
        if (Instance != null)
        {
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

    public void PlaySFXClip(AudioClip clip, Transform spawnPos, float volume, float pitch = 1.0f) {
        if (clip != null) {
            AudioSource audio = Instantiate(sfx, spawnPos.position, Quaternion.identity);
            audio.clip = clip;
            audio.volume = volume;
            audio.pitch = pitch;
            audio.Play();

            float duration = audio.clip.length;

            Destroy(audio.gameObject, duration);
        }
    }

    public void PlayRandomSFXClip(List<AudioClip> clip, Transform spawnPos, float volume) {
        if (clip != null) {
            int rand = Random.Range(0, clip.Count);

            AudioSource audio = Instantiate(sfx, spawnPos.position, Quaternion.identity);
            audio.clip = clip[rand];
            audio.volume = volume;
            audio.Play();

            float duration = audio.clip.length;

            Destroy(audio.gameObject, duration);
        }
    }
}
