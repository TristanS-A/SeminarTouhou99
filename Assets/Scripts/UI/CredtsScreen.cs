using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CredtsScreen : MonoBehaviour {
    [SerializeField] private GameObject panel, mainScreenPanel;

    [SerializeField] private Button backButton, creditsButton, optionsButton;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void HideMainScreen() {
        panel.SetActive(true);  
        mainScreenPanel.SetActive(false);
        backButton.gameObject.SetActive(true);
        creditsButton.gameObject.SetActive(false);
        optionsButton.gameObject.SetActive(true);
    }

    public void HidePanel() {
        panel.SetActive(false);
        mainScreenPanel.SetActive(true);
        backButton.gameObject.SetActive(false);
        creditsButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
    }

}
