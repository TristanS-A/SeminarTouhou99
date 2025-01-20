using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using Valve.Sockets;

public class Test : MonoBehaviour
{
    public static Test Instance;

    [SerializeField] private Button testServerButton;
    [SerializeField] private Button testClientButton;
    private NetworkingSockets server;
    private NetworkingSockets client;
    private uint pollGroup;
    private StatusCallback serverStatusCallback;
    private StatusCallback clientStatusCallback;

    //New Version Code
    private StatusCallback status;
    //private NetworkingSockets networkClient;
    private uint listenSocket;
    private bool isServer;

    private uint connection;

    private List<string> messages = new List<string>();

    private void Awake()
    {
    }

    private void OnApplicationQuit()
    {
        Valve.Sockets.Library.Deinitialize();
        Debug.Log("Quit and Socket Lib Deanitialized");
    }

    void Start()
    {
        Debug.Log("Socket Lib Itialized");
        Valve.Sockets.Library.Initialize();

        Instance = this;

        NetworkingUtils utils = new NetworkingUtils();
        utils.SetDebugCallback(DebugType.Everything, DebugMessageCallback);

        if (testServerButton != null)
        {
            testServerButton.onClick.AddListener(StartServer);
        }
        if (testClientButton != null)
        {
            testClientButton.onClick.AddListener(StartClient);
        }
    }

    void StartServer()
    {
        isServer = true;
        server = new NetworkingSockets();
        Address address = new Address();
        address.SetAddress("::0", 7777);
        listenSocket = server.CreateListenSocket(address);

        serverStatusCallback = OnServerStatusUpdate;
    }

    [MonoPInvokeCallback(typeof(DebugCallback))]
    static void DebugMessageCallback(DebugType type, string message)
    {
        Debug.Log("Debug - Type: " + type + ", Message: " + message);
    }

    [MonoPInvokeCallback(typeof(StatusCallback))]
    static void OnServerStatusUpdate(StatusInfo info, System.IntPtr context)
    {
        switch (info.connectionInfo.state)
        {
            case ConnectionState.None:
                break;

            case ConnectionState.Connecting:
                Instance.server.AcceptConnection(info.connection);
                break;

            case ConnectionState.Connected:
                Debug.Log("Server connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                Instance.connection = info.connection;
                break;

            case ConnectionState.ClosedByPeer:
                Instance.server.CloseConnection(info.connection);
                Debug.Log("Server disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                break;
        }
    }

    void StartClient()
    {
        client = new NetworkingSockets();
        Address address = new Address();
        address.SetAddress("::1", 7777);

        connection = client.Connect(address);

        clientStatusCallback = OnClientStatusUpdate;
    }

    [MonoPInvokeCallback(typeof(StatusCallback))]
    static void OnClientStatusUpdate(StatusInfo info, System.IntPtr context)
    {
        switch (info.connectionInfo.state)
        {
            case ConnectionState.None:
                break;

            case ConnectionState.Connected:
                Debug.Log("Client connected to server - ID: " + Instance.connection);
                break;

            case ConnectionState.ClosedByPeer:
                Instance.client.CloseConnection(Instance.connection);
                Debug.Log("Client disconnected from server");
                break;

            case ConnectionState.ProblemDetectedLocally:
                Instance.client.CloseConnection(Instance.connection);
                Debug.Log("Client unable to connect");
                break;
        }
    }


    void Update()
    {
        if (server != null)
        {
            server.DispatchCallback(serverStatusCallback);
        }

        if (client != null)
        {
            client.DispatchCallback(clientStatusCallback);
        }
    }

    private string inputString;
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 40), "Server"))
        {
            StartServer();
        }
        else if (GUI.Button(new Rect(10, 60, 100, 40), "Client"))
        {
            StartClient();
        }
    }






    //[SerializeField] private Button testServerButton;
    //[SerializeField] private Button testClientButton;
    //private NetworkingSockets server;
    //private NetworkingSockets client;
    //private uint pollGroup;
    //private StatusCallback serverStatusCallback;
    //private StatusCallback clientStatusCallback;

    ////New Version Code
    //    private void Awake()
    //    {
    //        Debug.Log("Socket Lib Itialized");
    //        Valve.Sockets.Library.Initialize();
    //    }

    //    private void OnApplicationQuit()
    //    {
    //        if (server != null)
    //        {
    //            server.DestroyPollGroup(pollGroup);
    //        }
    //            Valve.Sockets.Library.Deinitialize();
    //            Debug.Log("Quit and Socket Lib Deanitialized");
    //    }

    //    private void Start()
    //    {
    //        testButton = GetComponent<Button>();
    //        if (testButton != null)
    //        {
    //            testButton.onClick.AddListener(RunServerSetUp);
    //        }
    //    }

    //    private void RunServerSetUp()
    //    {
    //        //Create socket for server
    //        server = new NetworkingSockets();

    //        //Gets server pollGroup
    //        pollGroup = server.CreatePollGroup();

    //        //Sets up some utils for monotoring the server
    //        NetworkingUtils networkingUtils = new NetworkingUtils();

    //        //Defined status callback for server monotering
    //        StatusCallback statusCallback = (ref StatusInfo info) =>
    //        {
    //            switch (info.connectionInfo.state)
    //            {
    //                case ConnectionState.None:
    //                    break;
    //                case ConnectionState.Connecting:
    //                    server.AcceptConnection(info.connection);
    //                    server.SetConnectionPollGroup(pollGroup, info.connection);
    //                    break;
    //                case ConnectionState.Connected:
    //                    Debug.Log("Client has been connected - ID: " + info.connection + " " + info.connectionInfo.address.GetIP());
    //                    break;
    //                case ConnectionState.ClosedByPeer:
    //                //No break here I assume to log the disconnection error
    //                case ConnectionState.ProblemDetectedLocally:
    //                    server.CloseConnection(info.connection);
    //                    Debug.Log("Client has been disconnected - ID: " + info.connection + " " + info.connectionInfo.address.GetIP());
    //                    break;
    //            }
    //        };

    //        //Sets server moderating callback
    //        networkingUtils.SetStatusCallback(statusCallback);

    //        //Defines the server address
    //        Address address = new Address();

    //        //Sets the server address to port 5000 
    //        address.SetAddress("::0", 5000);

    //        //Creates listening socket on server from set address
    //        uint listenSocket = server.CreateListenSocket(ref address);

    //        //Enable SPAN for this next part
    ///*#if VALVESOCKET_SPAN
    //        MessageCallback message = (in NetworkingMessage netMessage) => 
    //        {
    //            Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
    //        };
    //#else
    //        const int maxMessages = 20;
    //        NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
    //#endif*/

    //    }

    //    private void Update()
    //    {
    //        if (server != null)
    //        {
    //            server.RunCallbacks();

    //            //Enable SPAN for this next part
    ///*#if VALVESOCKETS_SPAN
    //		server.ReceiveMessagesOnPollGroup(pollGroup, message, 20);
    //#else
    //            int netMessagesCount = server.ReceiveMessagesOnPollGroup(pollGroup, netMessages, maxMessages);

    //            if (netMessagesCount > 0)
    //            {
    //                for (int i = 0; i < netMessagesCount; i++)
    //                {
    //                    ref NetworkingMessage netMessage = ref netMessages[i];

    //                    Console.WriteLine("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

    //                    netMessage.Destroy();
    //                }
    //            }
    //#endif*/
    //        }
    //    }
}
