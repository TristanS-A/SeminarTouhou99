using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    enum ButtonType
    {
        START_GAME,
        MAIN_MENU
    }

    [SerializeField] private ButtonType mButtonType;
    [SerializeField] private Button mButton;
    [SerializeField] private bool mOnlyServerButton;

    // Start is called before the first frame update
    void Start()
    {
        if(ServerHandler.instance == null && mOnlyServerButton)
        {
            gameObject.SetActive(false);
        }

        switch (mButtonType)
        {
            case ButtonType.START_GAME:
                mButton.onClick.AddListener(StartGame);
                break;
            case ButtonType.MAIN_MENU:
                mButton.onClick.AddListener(GoToMainMenu);
                break;
        }
    }

    private void StartGame()
    {
        //Goes to game scene
        SceneManager.LoadScene(3);
    }

    private void GoToMainMenu()
    {
        //Starts ending session flow
        EventSystem.fireEvent(new EventType(EventType.EventTypes.END_GAME_SESSION));

        //Resets static values
        PlayerInfo.ResetPlayerPointInfo();

        //Goes to main menu scene
        SceneManager.LoadScene(0);
    }
}
