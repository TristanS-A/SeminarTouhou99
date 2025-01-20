using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using Valve.Sockets;

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
        //Create socket for server
        NetworkingSockets server = new NetworkingSockets();

        //Gets server pollGroup
        uint pollGroup = server.CreatePollGroup();

        //Sets up some utils for monotoring the server
        NetworkingUtils networkingUtils = new NetworkingUtils();

        //Defined status callback for server monotering
        StatusCallback statusCallback = (ref StatusInfo info) =>
        {
            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;
                case ConnectionState.Connecting:
                    server.AcceptConnection(info.connection);
                    server.SetConnectionPollGroup(pollGroup, info.connection);
                    break;
                case ConnectionState.Connected:
                    Debug.Log("Client has been connected - ID: " + info.connection + " " + info.connectionInfo.address.GetIP());
                    break;
                case ConnectionState.ClosedByPeer:
                //No break here I assume to log the disconnection error
                case ConnectionState.ProblemDetectedLocally:
                    server.CloseConnection(info.connection);
                    Debug.Log("Client has been disconnected - ID: " + info.connection + " " + info.connectionInfo.address.GetIP())
                    break;
            }
        };

        //Sets server moderating callback
        networkingUtils.SetStatusCallback(statusCallback);

        //Defines the server address
        Address address = new Address();

        //Sets the server address to port 5000 
        address.SetAddress("::0", 5000);

        //Creates listening socket on server from set address
        uint listenSocket = server.CreateListenSocket(ref address);

        //Enable SPAN for this next part
/*#if VALVESOCKET_SPAN
        MessageCallback message = (in NetworkingMessage netMessage) => 
        {
            Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
        };
#else
        const int maxMessages = 20;
        NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
#endif*/
    }
}
