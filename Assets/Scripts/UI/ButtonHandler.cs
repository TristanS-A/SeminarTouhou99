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
        JOIN_LAN_ROOM,
        JOIN_WAN_ROOM,
        HOST_ROOM,
        MAIN_MENU,
    }

    [SerializeField] private ButtonType mButtonType;
    [SerializeField] private Button mButton;
    [SerializeField] private bool mOnlyServerButton;

    [SerializeField] private GameObject m_SceneTransition;

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
            case ButtonType.JOIN_LAN_ROOM:
                mButton.onClick.AddListener(SwitchToJoinLANLobbyScene);
                break;
            case ButtonType.JOIN_WAN_ROOM:
                mButton.onClick.AddListener(SwitchToJoinWANLobbyScene);
                break;
            case ButtonType.HOST_ROOM:
                mButton.onClick.AddListener(SwitchToJoinWANLobbyScene);
                break;
        }
    }

    private void StartGame()
    {
        EventSystem.SendGameStartState();

        LAN_DiscoveryClient.CloseClient();

        EventSystem.StartGame();

        Instantiate(m_SceneTransition).GetComponentInChildren<TransitionHandler>().sceneToTransitionTo = 4;
    }

    private void SwitchToJoinLANLobbyScene()
    {
        SceneManager.LoadScene(1);
    }

    private void SwitchToJoinWANLobbyScene()
    {
        SceneManager.LoadScene(2);
    }

    private void GoToMainMenu()
    {
        //Starts ending session flow
        EventSystem.EndGameSession();

        //Closes LAN discovery client if it exists
        LAN_DiscoveryClient.CloseClient();

        //Resets static values
        PlayerInfo.ResetPlayerPointInfo();

        //Goes to main menu scene
        Instantiate(m_SceneTransition).GetComponentInChildren<TransitionHandler>().sceneToTransitionTo = 0;
    }
}
