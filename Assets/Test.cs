using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private Button testButton;

    private void Awake()
    {
        Debug.Log("Socket Lib Itialized");
        Valve.Sockets.Library.Initialize();
    }

    private void OnApplicationQuit()
    {
        Valve.Sockets.Library.Deinitialize();
        Debug.Log("Quit and Socket Lib Deanitialized");
    }

    private void Start()
    {
        testButton = GetComponent<Button>();
        if (testButton != null)
        {
            testButton.onClick.AddListener(RunServerSetUp);
        }
    }

    private void RunServerSetUp()
    {

    }
}
