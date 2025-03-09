using AOT;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.Sockets;
using static clientHandler;

public class serverHandler : MonoBehaviour
{
    [SerializeField] private Button testServerButton;
    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private GameObject m_PlayerHologramPrefab;
    Dictionary<uint, GameObject> players = new Dictionary<uint, GameObject>();
    Dictionary<uint, List<Vector3>> playerPoses = new Dictionary<uint, List<Vector3>>();
    Dictionary<uint, float> playerInterpolationTracker = new Dictionary<uint, float>();
    private NetworkingSockets server;
    //private uint serverPlayerID = 0;
    private uint pollGroup;
    private StatusCallback serverNetworkingUtils;
    NetworkingUtils utils = new NetworkingUtils();
    private uint listenSocket;
    private float mPacketSendTime = 0.0f;
    private const float PACKET_TARGET_SEND_TIME = 0.033f;
    private System.Net.IPAddress mServerIP;

    //MessageCallback message;
    const int maxMessages = 20;
    NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
    byte[] messageDataBuffer = new byte[256];

    string inputString;

    List<uint> connectedClients = new();

    GameState mGameState = GameState.NONE;

    public static serverHandler instance;

    [SerializeField] private bool mDebugMode = false;

    public enum GameState
    {
        NONE,
        LOOKING_FOR_HOST,
        SEARCHING_FOR_PLAYERS,
        GAME_STARTED
    }

    private void OnEnable()
    {
        eventSystem.gameStarted += HandleGameStart;
    }

    private void OnDisable()
    {
        eventSystem.gameStarted -= HandleGameStart;
    }

    // Start is called before the first frame update
    void Start()
    {
        Valve.Sockets.Library.Initialize();

        instance = this;

        if (testServerButton != null)
        {
            testServerButton.onClick.AddListener(RunServerSetUp);
        }

        DebugCallback debugCallback = (DebugType type, string message) =>
        {
            Debug.Log("Sever Debug - Type: " + type + ", Message: " + message);
        };

        utils.SetDebugCallback(DebugType.Everything, debugCallback);
    }

    private void OnApplicationQuit()
    {
        Valve.Sockets.Library.Deinitialize();
        if (!mDebugMode)
        {
            UDPListener.CloseClient();
            Debug.Log("Quit and Socket Lib Deanitialized");
        }
    }

    private void RunServerSetUp()
    {
        Debug.Log("Starting Server...");

        //Makes sure server handler persists scenes
        DontDestroyOnLoad(transform.gameObject);

        //Creates server socket
        server = new NetworkingSockets();

        //Sets callbacks
        serverNetworkingUtils = serverStatusCallback;

        Address address = new Address();

        //Gets IP address to host from
        mServerIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

        address.SetAddress(mServerIP.ToString(), 5000);

        listenSocket = server.CreateListenSocket(address);

        if (!mDebugMode)
        {
            //Starts UDP client to broadcast host IP
            UDPListener.StartClient(false);
        }

        mGameState = GameState.SEARCHING_FOR_PLAYERS;

        //Switches to lobby scene
        SceneManager.LoadScene(2);

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
    private void serverStatusCallback(StatusInfo info, System.IntPtr context)
    {
        switch (info.connectionInfo.state)
        {
            case ConnectionState.None:
                break;

            case ConnectionState.Connecting:
                server.AcceptConnection(info.connection);
                break;

            case ConnectionState.Connected:
                handlePlayerJoining(info.connection);
                Debug.Log("Client connected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                break;

            case ConnectionState.ClosedByPeer:
            case ConnectionState.ProblemDetectedLocally:
                handlePlayerLeaving(info.connection);
                Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                break;
        }
    }

    private void HandleGameStart(GameObject player)
    {
        mGameState = GameState.GAME_STARTED;

        if (server != null)
        {
            var keys = players.Keys;
            for (int i = 0; i < players.Count; i++)
            {
                players[keys.ElementAt(i)] = Instantiate(m_PlayerHologramPrefab);

                clientHandler.GameStartData gameState = new clientHandler.GameStartData();
                gameState.type = (int)clientHandler.PacketType.GAME_STATE;
                gameState.gameState = (int)eventType.EventTypes.START_GAME;

                Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.GameStartData))];
                GCHandle pinStructure = GCHandle.Alloc(gameState, GCHandleType.Pinned);
                try
                {
                    Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                }
                finally
                {
                    server.SendMessageToConnection(connectedClients[i], bytes);
                    pinStructure.Free();
                }
            }

            players.Add(0, player);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (server != null)
        {
            switch (mGameState)                 //Handles gameplay networking
            {
                case GameState.GAME_STARTED:
                    server.DispatchCallback(serverNetworkingUtils);

                    handleInterpolatePlayerPoses();
                    if (mPacketSendTime >= PACKET_TARGET_SEND_TIME)
                    {
                        SendPlayerData();
                        mPacketSendTime = 0.0f;
                    }
                    mPacketSendTime += Time.deltaTime;

                    //Enable SPAN for this next part
#if VALVESOCKETS_SPAN
                server.ReceiveMessagesOnPollGroup(pollGroup, message, 20);
#else
                    int netMessagesCount = server.ReceiveMessagesOnListenSocket(listenSocket, netMessages, maxMessages);

                    if (netMessagesCount > 0)
                    {
                        for (int i = 0; i < netMessagesCount; i++)
                        {
                            ref NetworkingMessage netMessage = ref netMessages[i];

                            Debug.Log("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

                            netMessage.CopyTo(messageDataBuffer);
                            netMessage.Destroy();

                            ////REFERENCE: https://stackoverflow.com/questions/17840552/c-sharp-cast-a-byte-array-to-an-array-of-struct-and-vice-versa-reverse

                            IntPtr ptPoit = Marshal.AllocHGlobal(messageDataBuffer.Length);
                            Marshal.Copy(messageDataBuffer, 0, ptPoit, messageDataBuffer.Length);

                            TypeFinder packetType = (TypeFinder)Marshal.PtrToStructure(ptPoit, typeof(TypeFinder));

                            switch ((PacketType)packetType.type)
                            {
                                case PacketType.PLAYER_DATA:
                                    PlayerData packetData = (PlayerData)Marshal.PtrToStructure(ptPoit, typeof(PlayerData));
                                    handlePlayerData(packetData);
                                    break;
                            }

                            Marshal.FreeHGlobal(ptPoit);
                        }
                    }
#endif
                    break;
                case GameState.SEARCHING_FOR_PLAYERS:    //Handles joining players
                    server.DispatchCallback(serverNetworkingUtils);

                    if (mPacketSendTime >= PACKET_TARGET_SEND_TIME)    ////Refactor this to reset packet send time for actual game maybe (and to look better)
                    {
                        SendGameJoinMessage();
                        BroadcastPlayerCount();
                        BroadcastPlayerName();
                        mPacketSendTime = 0.0f;
                    }
                    mPacketSendTime += Time.deltaTime;
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        //handleMovePlayer();
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

        //inputString = GUI.TextField(new Rect(200, 370, 400, 50), inputString);
        //if (GUI.Button(new Rect(200, 450, 100, 50), "send"))
        //{
        //    SendPlayerData(inputString);
        //    inputString = "";
        //}

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

    private void SendPlayerData()
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                foreach (uint playerID in players.Keys)
                {
                    GameObject player = players[playerID];
                    if (playerID != connectedClients[i] && player != null)
                    {
                        clientHandler.PlayerData playerData = new clientHandler.PlayerData();
                        playerData.pos = player.transform.position;
                        playerData.speed = 12;
                        playerData.type = (int)clientHandler.PacketType.PLAYER_DATA;
                        playerData.playerID = playerID;


                        Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.PlayerData))];
                        GCHandle pinStructure = GCHandle.Alloc(playerData, GCHandleType.Pinned);
                        try
                        {
                            Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                        }
                        finally
                        {
                            Debug.Log("SENDING PLAYER DATA FOR PLAYER: " + playerID);
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
    }

    private void BroadcastPlayerCount()
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                foreach (uint playerID in players.Keys)
                {
                    clientHandler.PlayerCountData playerCount = new clientHandler.PlayerCountData();
                    playerCount.type = (int)clientHandler.PacketType.PLAYER_COUNT;
                    playerCount.playerCount = connectedClients.Count + 1;

                    Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.PlayerCountData))];
                    GCHandle pinStructure = GCHandle.Alloc(playerCount, GCHandleType.Pinned);
                    try
                    {
                        Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                    }
                    finally
                    {
                        server.SendMessageToConnection(connectedClients[i], bytes);
                        pinStructure.Free();
                    }
                }
            }
        }
    }

    private void BroadcastPlayerName()
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                foreach (uint playerID in players.Keys)
                {
                    clientHandler.PlayerName playerName = new clientHandler.PlayerName();
                    playerName.type = (int)clientHandler.PacketType.PLAYER_NAME;
                    string str = "HIIIIIIIIIII THere";
                    playerName.name = str;

                    //Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.PlayerName))];
                    // GCHandle pinStructure = GCHandle.Alloc(playerName, GCHandleType.Pinned);

                    IntPtr ptr = IntPtr.Zero;
                    byte[] arr = new byte[Marshal.SizeOf(typeof(clientHandler.PlayerName))];

                    try
                    {
                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(clientHandler.PlayerName)));
                        Marshal.StructureToPtr(playerName, ptr, true);
                        Marshal.Copy(ptr, arr, 0, Marshal.SizeOf(typeof(clientHandler.PlayerName)));
                    }
                    finally
                    {
                        server.SendMessageToConnection(connectedClients[i], arr);
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
        }
    }

    private void BroadcastResults()
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                foreach (uint playerID in players.Keys)
                {
                    GameObject player = players[playerID];
                    if (playerID != connectedClients[i] && player != null)
                    {
                        clientHandler.PlayerResultData playerResultData = new clientHandler.PlayerResultData();
                        playerResultData.type = (int)clientHandler.PacketType.PLAYER_RESULT;
                        playerResultData.playerID = playerID;
                        playerResultData.time = 0;
                        playerResultData.points = 0;


                        Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.PlayerResultData))];
                        GCHandle pinStructure = GCHandle.Alloc(playerResultData, GCHandleType.Pinned);
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
    }

    private void SendGameJoinMessage()
    {
        if (mServerIP != null && !mDebugMode)
        {
            UDPListener.SendIP(mServerIP.ToString());
        }
    }

    private void handlePlayerJoining(uint playerID)
    {
        if (!players.ContainsKey(playerID))
        {
            connectedClients.Add(playerID);
            players.Add(playerID, null);
            playerPoses.Add(playerID, new());
            playerInterpolationTracker.Add(playerID, 0.0f);

            clientHandler.RegisterPlayer playerData = new clientHandler.RegisterPlayer();
            playerData.type = (int)clientHandler.PacketType.REGISTER_PLAYER;
            playerData.playerID = playerID;


            Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.RegisterPlayer))];
            GCHandle pinStructure = GCHandle.Alloc(playerData, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
            }
            finally
            {
                Debug.Log("SENDING PLAYER DATA");
                server.SendMessageToConnection(playerID, bytes);
                pinStructure.Free();
            }

            eventSystem.fireEvent(new PlayerCountChangedEvent(connectedClients.Count + 1)); //Refactor to use player dictionary
        }
    }

    private void handlePlayerLeaving(uint playerID)
    {
        if (players.ContainsKey(playerID))
        {
            connectedClients.Remove(playerID);
            Destroy(players[playerID]);
            players.Remove(playerID);
            playerPoses.Remove(playerID);
            playerInterpolationTracker.Remove(playerID);
            server.CloseConnection(playerID);

            if (mGameState == GameState.SEARCHING_FOR_PLAYERS)
            {
                eventSystem.fireEvent(new PlayerCountChangedEvent(connectedClients.Count + 1)); //Refactor to use player dictionary
            }
        }
    }

    private void handlePlayerData(clientHandler.PlayerData playerData)
    {
        GameObject playerOBJ;
        if (!players.ContainsKey(playerData.playerID))
        {
            playerOBJ = Instantiate(m_PlayerHologramPrefab);
            players.Add(playerData.playerID, playerOBJ);
            playerPoses.Add(playerData.playerID, new());
            playerInterpolationTracker.Add(playerData.playerID, 0.0f);
        }
        else if (players[playerData.playerID] == null)    //Maybe refactor this to instantiate holograms when HandleStartGame is run
        {
            playerOBJ = Instantiate(m_PlayerHologramPrefab);
            playerPoses.Add(playerData.playerID, new());
            playerInterpolationTracker.Add(playerData.playerID, 0.0f);
        }

        playerPoses[playerData.playerID].Clear();
        playerPoses[playerData.playerID].Add(players[playerData.playerID].transform.position);
        playerPoses[playerData.playerID].Add(playerData.pos);

        playerInterpolationTracker[playerData.playerID] = 0.0f;
    }

    private void handleInterpolatePlayerPoses()
    {
        foreach (uint key in playerPoses.Keys)
        {
            if (playerPoses[key].Count > 0)
            {
                GameObject playerOBJ = players[key];
                playerOBJ.transform.position = Vector3.Lerp(playerPoses[key][0], playerPoses[key][1], playerInterpolationTracker[key] / PACKET_TARGET_SEND_TIME);

                playerInterpolationTracker[key] += Time.deltaTime;

                if (playerInterpolationTracker[key] >= PACKET_TARGET_SEND_TIME)
                {
                    playerPoses[key].Clear();
                }
            }
        }
    }

    //private void handleMovePlayer()
    //{
    //    if (players.ContainsKey(serverPlayerID))
    //    {
    //        Transform serverPlayerTransform = players[serverPlayerID].transform;
    //        if (Input.GetKey(KeyCode.W))
    //        {
    //            serverPlayerTransform.position += new Vector3(0, 0.1f, 0);
    //        }
    //        if (Input.GetKey(KeyCode.D))
    //        {
    //            serverPlayerTransform.position += new Vector3(0.1f, 0, 0);
    //        }
    //        if (Input.GetKey(KeyCode.S))
    //        {
    //            serverPlayerTransform.position += new Vector3(0, -0.1f, 0);
    //        }
    //        if (Input.GetKey(KeyCode.A))
    //        {
    //            serverPlayerTransform.position += new Vector3(-0.1f, 0, 0);
    //        }
    //    }
    //}
}
