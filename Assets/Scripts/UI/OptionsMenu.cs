using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {
    [Serializable]
    public class Option {
        public Button button;
        public GameObject panel;
    }
    [SerializeField] private List<Option> options;
    private int currentIndex = 0;

    [Header("Temp Fix")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Button lobbyButton, optionButton, creditsButton;
    [SerializeField] private Slider m_Master, m_SFX, m_Music;

    private void Awake() {
        SwitchPanelByIndex(currentIndex);
    }

    private void Start()
    {
        m_Master.onValueChanged.AddListener(SoundMixerManager.Instance.SetMasterVolume);
        m_SFX.onValueChanged.AddListener(SoundMixerManager.Instance.SetSoundEffectsVolume);
        m_Music.onValueChanged.AddListener(SoundMixerManager.Instance.SetMusicVolume);
    }

    private void ActivatePanel(GameObject panelToActivate, Func<GameObject> GetStartElement) {
        foreach (var option in options) {
            options[currentIndex].panel.SetActive(false);
        }

        panelToActivate.SetActive(true);

        StartCoroutine(SetSelectedAfterDelay(() => {
            GameObject startElement = GetStartElement?.Invoke();
        }));
    }

    public void SwitchPanelByIndex(int index) {
        ActivatePanel(options[index].panel, () => {
            currentIndex = index;
            return options[index].panel;
        });
    }

    private IEnumerator SetSelectedAfterDelay(Action action) {
        yield return null;
        action.Invoke();
    }

    public void HideOptionPanel() {
        optionPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        optionButton.gameObject.SetActive(true);
        lobbyButton.gameObject.SetActive(false);
        creditsButton.gameObject.SetActive(true);
    }

    public void HideLobbyPanel() {
        lobbyPanel.SetActive(false);
        optionPanel.SetActive(true);
        optionButton.gameObject.SetActive(false);
        lobbyButton.gameObject.SetActive(true);
        creditsButton.gameObject.SetActive(false);
    }
}
