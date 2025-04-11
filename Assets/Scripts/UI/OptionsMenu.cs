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

    private void Awake() {
        SwitchPanelByIndex(currentIndex);
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
}
