using AOT;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Burst.Intrinsics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.Sockets;
using static clientHandler;
using static EventType;

public class serverHandler : MonoBehaviour
{
    public struct PlayerGameData
    {
        public GameObject playerOBJ;
        public List<Vector3> playerPoses;
        public float playerInterpolationTracker;
        //public string name;

        public void init()
        { 
            playerPoses = new List<Vector3>();
            playerInterpolationTracker = 0;
        }
    }

    //Add a state for collecting result data and once it is all collected then send it maybe
    public struct PlayerStoredResultData    //Add to a dictionary of this when a player disconnects or dies/wins which sends an event with this data
    {
        public string name;
        public int points;
        public int time;
    }

    [SerializeField] private Button testServerButton;
    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private GameObject m_PlayerHologramPrefab;

    Dictionary<uint, PlayerGameData> mPlayers = new();
    Dictionary<uint, PlayerStoredResultData> mPlayerResults = new();
   
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
        GAME_STARTED,
        RESULTS_SCREEN
    }

    private void OnEnable()
    {
        EventSystem.gameStarted += HandleGameStart;
        EventSystem.onPlayerDeath += OnPlayerDie;
    }

    private void OnDisable()
    {
        EventSystem.gameStarted -= HandleGameStart;
        EventSystem.onPlayerDeath -= OnPlayerDie;
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
                bool inGame = mGameState == GameState.GAME_STARTED;    //MAKE SURE TO SEND RESULT DATA FROM CLIENT BEFORE DETECTING
                handlePlayerLeaving(info.connection, inGame);
                Debug.Log("Client disconnected - ID: " + info.connection + ", IP: " + info.connectionInfo.address.GetIP());
                break;
        }
    }

    private void HandleGameStart(GameObject player)
    {
        mGameState = GameState.GAME_STARTED;

        if (server != null)
        {
            var keys = mPlayers.Keys;
            for (int i = 0; i < mPlayers.Count; i++)
            {
                PlayerGameData pD = new();
                pD.init();
                pD.playerOBJ = Instantiate(m_PlayerHologramPrefab);
                mPlayers[keys.ElementAt(i)] = pD;

                clientHandler.GameStateData gameState = new clientHandler.GameStateData();
                gameState.type = (int)clientHandler.PacketType.GAME_STATE;
                gameState.gameState = (int)EventType.EventTypes.START_GAME;

                Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.GameStateData))];
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

            PlayerGameData ownPlayer = new();
            ownPlayer.playerOBJ = player;
            ownPlayer.init();

            mPlayers.Add(0, ownPlayer);
        }
    }

    private void HandleGameFinish()
    {
        mGameState = GameState.RESULTS_SCREEN;

        if (server != null)
        {
            var keys = mPlayers.Keys;
            for (int i = 0; i < mPlayers.Count; i++)
            {
                clientHandler.GameStateData gameState = new clientHandler.GameStateData();
                gameState.type = (int)clientHandler.PacketType.GAME_STATE;
                gameState.gameState = (int)EventType.EventTypes.GAME_FINISHED;

                Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.GameStateData))];
                GCHandle pinStructure = GCHandle.Alloc(gameState, GCHandleType.Pinned);
                try                                                                                 ////CONVERT THIS INTO A FUNCTION MAYBE
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

        SceneManager.LoadScene(4);
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
                                case PacketType.PLAYER_RESULT:
                                    HandleReceivePlayerResult();
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
                case GameState.RESULTS_SCREEN:
                    BroadcastResults();
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
                foreach (uint playerID in mPlayers.Keys)
                {
                    PlayerGameData player = mPlayers[playerID];
                    if (playerID != connectedClients[i] && player.playerOBJ != null)
                    {
                        clientHandler.PlayerData playerData = new clientHandler.PlayerData();
                        playerData.pos = player.playerOBJ.transform.position;
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
                foreach (uint playerID in mPlayers.Keys)
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
                foreach (uint playerID in mPlayers.Keys)
                {
                    clientHandler.PlayerName playerName = new clientHandler.PlayerName();
                    playerName.type = (int)clientHandler.PacketType.PLAYER_NAME;
                    playerName.name = "mPlayers[playerID].name";

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
                foreach (uint playerID in mPlayerResults.Keys)
                {
                    PlayerStoredResultData player = mPlayerResults[playerID];
                    if (playerID != connectedClients[i])
                    {
                        clientHandler.PlayerSendResultData PlayerSendResultData = new clientHandler.PlayerSendResultData();
                        PlayerSendResultData.type = (int)clientHandler.PacketType.PLAYER_RESULT;
                        PlayerSendResultData.playerID = playerID;
                        PlayerSendResultData.time = 0;
                        PlayerSendResultData.points = 0;
                        PlayerSendResultData.name = player.name;

                        Byte[] bytes = new Byte[Marshal.SizeOf(typeof(clientHandler.PlayerSendResultData))];
                        GCHandle pinStructure = GCHandle.Alloc(PlayerSendResultData, GCHandleType.Pinned);
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
        if (!mPlayers.ContainsKey(playerID))
        {
            connectedClients.Add(playerID);

            PlayerGameData player = new PlayerGameData();
            player.init();

            mPlayers.Add(playerID, player);

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

            EventSystem.fireEvent(new PlayerCountChangedEvent(connectedClients.Count + 1)); //Refactor to use player dictionary
        }
    }

    private void handlePlayerLeaving(uint playerID, bool inGame)
    {
        if (mPlayers.ContainsKey(playerID))
        {
            connectedClients.Remove(playerID);
            Destroy(mPlayers[playerID].playerOBJ);
            if (!inGame)
            {
                mPlayers.Remove(playerID);
            }
            server.CloseConnection(playerID);

            if (mGameState == GameState.SEARCHING_FOR_PLAYERS)
            {
                EventSystem.fireEvent(new PlayerCountChangedEvent(connectedClients.Count + 1)); //Refactor to use player dictionary
            }
        }
    }

    private void handlePlayerData(clientHandler.PlayerData playerData)
    {
        PlayerGameData player = new PlayerGameData();
        if (!mPlayers.ContainsKey(playerData.playerID))
        {
            player.playerOBJ = Instantiate(m_PlayerHologramPrefab); ;
            player.init();
            player.playerInterpolationTracker = 0.0f;

            mPlayers.Add(playerData.playerID, player);
        }
        else if (mPlayers[playerData.playerID].playerOBJ == null)    //Maybe refactor this to instantiate holograms when HandleStartGame is run
        {
            //playerOBJ = Instantiate(m_PlayerHologramPrefab);
            //playerPoses.Add(playerData.playerID, new());
            //playerInterpolationTracker.Add(playerData.playerID, 0.0f);
        }

        mPlayers[playerData.playerID].playerPoses.Clear();
        mPlayers[playerData.playerID].playerPoses.Add(mPlayers[playerData.playerID].playerOBJ.transform.position);
        mPlayers[playerData.playerID].playerPoses.Add(playerData.pos);

        PlayerGameData pData = mPlayers[playerData.playerID];
        pData.playerInterpolationTracker = 0.0f;
        mPlayers[playerData.playerID] = pData;
    }

    private void handleInterpolatePlayerPoses()
    {
        var keys = mPlayers.Keys;
        for (int i = 0; i < mPlayers.Count; i++)
        {
            uint id = keys.ElementAt(i);
            if (mPlayers[id].playerPoses.Count > 0)
            {
                GameObject playerOBJ = mPlayers[id].playerOBJ;
                playerOBJ.transform.position = Vector3.Lerp(mPlayers[id].playerPoses[0], mPlayers[id].playerPoses[1], mPlayers[id].playerInterpolationTracker / PACKET_TARGET_SEND_TIME);

                PlayerGameData pData = mPlayers[id];
                pData.playerInterpolationTracker += Time.deltaTime;
                mPlayers[id] = pData;

                if (mPlayers[id].playerInterpolationTracker >= PACKET_TARGET_SEND_TIME)
                {
                    mPlayers[id].playerPoses.Clear();
                }
            }
        }
    }

    private void HandleReceivePlayerResult()
    {
        clientHandler.PlayerSendResultData playerReceivedResult = new clientHandler.PlayerSendResultData();
        int size = Marshal.SizeOf(playerReceivedResult);
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(messageDataBuffer, 0, ptr, size);

            playerReceivedResult = (clientHandler.PlayerSendResultData)Marshal.PtrToStructure(ptr, playerReceivedResult.GetType());

            PlayerStoredResultData playerStoreResult = new() { name = playerReceivedResult.name, 
                                                               points = playerReceivedResult.points, 
                                                               time = playerReceivedResult.time };
            Debug.Log("REceived result from : " + playerReceivedResult.playerID);
            mPlayerResults.Add(playerReceivedResult.playerID, playerStoreResult);

            //Check if game is finished (all players are done playing)
            if (CheckIfGameFinished())
            {
                HandleGameFinish();
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    //This function will be called when a player dies/finished (and also disconnects probobly)
    private bool CheckIfGameFinished()
    {
        //Game will finish when all results are in (Add safety exception in case not but level is done maybe)
        if (mPlayers.Count == mPlayerResults.Count)
        {
            return true;
        }

        return false;
    }

    private void OnPlayerDie()
    {
        PlayerStoredResultData playerStoreResult = new()
        {
            name = PlayerInfo.PlayerName,
            points = PlayerInfo.PlayerPoints,
            time = PlayerInfo.PlayerTime
        };

        mPlayerResults.Add(0, playerStoreResult);

        //Check if game is finished (all players are done playing)
        if (CheckIfGameFinished())
        {
            HandleGameFinish();
        }
    }
}
