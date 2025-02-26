using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    enum ButtonType
    {
        START_GAME
    }

    [SerializeField] private ButtonType mButtonType;
    [SerializeField] private Button mButton;

    // Start is called before the first frame update
    void Start()
    {
        switch (mButtonType)
        {
            case ButtonType.START_GAME:
                mButton.onClick.AddListener(StartGame);
                break;
        }
    }

    private void StartGame()
    {
        //Goes to game scene
        SceneManager.LoadScene(3);
    }
}
