using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor.MemoryProfiler;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using Valve.Sockets;

public class serverHandler : MonoBehaviour
{
    [SerializeField] private Button testServerButton;
    private NetworkingSockets server;
    private uint pollGroup;
    private NetworkingUtils serverNetworkingUtils = new NetworkingUtils();
    private uint listenSocket;

    //MessageCallback message;
    const int maxMessages = 20;
    NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];

    string inputString;

    List<uint> connectedClients = new();

    // Start is called before the first frame update
    void Start()
    {
        Valve.Sockets.Library.Initialize();

        if (testServerButton != null)
        {
            testServerButton.onClick.AddListener(RunServerSetUp);
        }

        DebugCallback debugCallback = (DebugType type, string message) =>
        {
            Debug.Log("Sever Debug - Type: " + type + ", Message: " + message);
        };

        serverNetworkingUtils.SetDebugCallback(DebugType.Everything, debugCallback);
    }

    private void OnApplicationQuit()
    {
        if (server != null)
        {
            server.DestroyPollGroup(pollGroup);
        }

        Valve.Sockets.Library.Deinitialize();
        Debug.Log("Quit and Socket Lib Deanitialized");
    }

    private void RunServerSetUp()
    {
        Debug.Log("Starting Server...");

        server = new NetworkingSockets();

        pollGroup = server.CreatePollGroup();

        serverNetworkingUtils.SetStatusCallback(serverStatusCallback);

        Address address = new Address();

        address.SetAddress("::0", 5000);

        listenSocket = server.CreateListenSocket(ref address);

#if VALVESOCKETS_SPAN
        message = (in NetworkingMessage netMessage) => {
            Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
        };
#else
        const int maxMessages = 20;

        NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
#endif
    }

    [MonoPInvokeCallback(typeof(StatusCallback))]
    private void serverStatusCallback(ref StatusInfo info)
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
                connectedClients.Add(info.connection);
                Debug.Log("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                break;

            case ConnectionState.ClosedByPeer:
            case ConnectionState.ProblemDetectedLocally:
                server.CloseConnection(info.connection);
                Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (server != null)
        {
            server.RunCallbacks();

            //Enable SPAN for this next part
#if VALVESOCKETS_SPAN
            server.ReceiveMessagesOnPollGroup(pollGroup, message, 20);
#else
            int netMessagesCount = server.ReceiveMessagesOnPollGroup(pollGroup, netMessages, maxMessages);

            if (netMessagesCount > 0)
            {
                for (int i = 0; i < netMessagesCount; i++)
                {
                    ref NetworkingMessage netMessage = ref netMessages[i];

                    Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

                    netMessage.Destroy();
                }
            }
#endif
        }
    }

    private void OnGUI()
    {
        for (int i = 0; i < connectedClients.Count; i++)
        {
            GUI.TextArea(new Rect(100, 10 + 20 * i, 200, 20), "ClientID: " + (connectedClients[i]).ToString());

            if (GUI.Button(new Rect(300, 10 + 20 * i, 100, 20), "Remove"))
            {
                server.CloseConnection(connectedClients[i]);
                connectedClients.RemoveAt(i);
            }
        }

        inputString = GUI.TextField(new Rect(200, 370, 400, 50), inputString);
        if (GUI.Button(new Rect(200, 450, 100, 50), "send"))
        {
            SendChatMessage(inputString);
            inputString = "";
        }

        //if (server != null)
        //{
        //    if (GUI.Button(new Rect(150, 550, 100, 20), "Stop Sever"))
        //    {
        //        server.CloseListenSocket(listenSocket);
        //        server.DestroyPollGroup(pollGroup);
        //        server = null;
        //        serverNetworkingUtils.Dispose();
        //    }
        //}
    }

    void SendChatMessage(string message)
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                clientHandler.PlayerData playerData = new clientHandler.PlayerData();
                playerData.pos = new Vector2(0, 10);
                playerData.speed = 12;
                playerData.type = 1;
                playerData.playerid = 1;


                Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.PlayerData))];
                GCHandle pinStructure = GCHandle.Alloc(playerData, GCHandleType.Pinned);
                try
                {
                    Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                }
                finally
                {
                    server.SendMessageToConnection(connectedClients[i], bytes);
                    pinStructure.Free();
                }

                //byte[] bytes = Encoding.ASCII.GetBytes(playerData);
                //server.SendMessageToConnection(connectedClients[i], bytes);

                //messages.Add(message);
            }
        }
    }
}
