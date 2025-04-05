using AOT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.Sockets;

public class ServerHandler : MonoBehaviour
{
    //Handles storing all networked player data (position, player gameobjects, and position interpolation)
    public struct PlayerGameData
    {
        public GameObject playerOBJ;
        public List<Vector3> playerPoses;
        public float playerInterpolationTracker;

        public void init()
        {
            playerPoses = new List<Vector3>();
            playerInterpolationTracker = 0;
        }
    }

    //Add a state for collecting result data and once it is all collected then send it maybe
    //Add to a dictionary of this when a player disconnects or dies/wins which sends an event with this data
    public struct PlayerStoredResultData
    {
        public string name;
        public int points;
        public float time;
        public int placement;
        public int score;
        public ResultContext resultContext;
    }

    public enum ResultContext
    {
        PLAYER_WON,
        PLAYER_DIED,
        PLAYER_DISCONNECTED
    }

    //Serialized buttons and prefabs
    [SerializeField] private Button testServerButton;
    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private GameObject m_PlayerHologramPrefab;

    //Player data storage
    Dictionary<uint, PlayerGameData> mPlayers = new();
    Dictionary<uint, PlayerStoredResultData> mPlayerResults = new();

    //Networking members (server)
    private NetworkingSockets server;
    private uint serverPlayerID = 0;
    private StatusCallback serverNetworkingUtils;
    private NetworkingUtils utils = new NetworkingUtils();
    private uint listenSocket;
    private float mPacketSendTime = 0.0f;
    private const float PACKET_TARGET_SEND_TIME = 0.033f;
    private System.Net.IPAddress mServerIP;
    private List<uint> connectedClients = new();

    //Netowrking packet message data
    private const int MAX_MESSAGES = 20;
    private NetworkingMessage[] netMessages = new NetworkingMessage[MAX_MESSAGES];
    private byte[] messageDataBuffer = new byte[256];

    //Gamestate storage
    private GameState mGameState = GameState.NONE;

    //Singleton instance
    public static ServerHandler instance;

    //Debug mode for running multiple games on a single computer
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
        EventSystem.OnSendPlayerResultData += HandleSendPlayerResults;
        EventSystem.onEndGameSession += EndSession;
        EventSystem.OnFireOffensiveBomb += SendBombDataFromServer;
    }

    private void OnDisable()
    {
        EventSystem.gameStarted -= HandleGameStart;
        EventSystem.OnSendPlayerResultData -= HandleSendPlayerResults;
        EventSystem.onEndGameSession -= EndSession;
        EventSystem.OnFireOffensiveBomb -= SendBombDataFromServer;
    }

    //Handle singleton instance no-replication and networking setup
    void Start()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        instance = this;

        Valve.Sockets.Library.Initialize();

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

    //Handles closing of netorking lib and connections
    private void OnApplicationQuit()
    {
        HandleCloseConnection();
        Valve.Sockets.Library.Deinitialize();
    }

    private void HandleCloseConnection()
    {
        Debug.Log("Closing Connection");
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

        Address address = new();

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
        NetworkingMessage[] netMessages = new NetworkingMessage[MAX_MESSAGES];
#endif
    }

    //Connection callbacks for connection status on clients
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
                //MAKE SURE TO SEND RESULT DATA FROM CLIENT BEFORE DETECTING
                bool inGame = mGameState == GameState.GAME_STARTED;    
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

                ClientHandler.GameStateData gameState = new ClientHandler.GameStateData();
                gameState.type = (int)ClientHandler.PacketType.GAME_STATE;
                gameState.gameState = (int)EventType.EventTypes.START_GAME;

                Byte[] bytes = new Byte[Marshal.SizeOf(typeof(ClientHandler.GameStateData))];
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

            mPlayers.Add(serverPlayerID, ownPlayer);
        }
    }

    private void HandleGameFinish()
    {
        SceneManager.LoadScene(4);
        mGameState = GameState.RESULTS_SCREEN;

        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                ClientHandler.GameStateData gameState = new ClientHandler.GameStateData();
                gameState.type = (int)ClientHandler.PacketType.GAME_STATE;
                gameState.gameState = (int)EventType.EventTypes.GAME_FINISHED;

                Byte[] bytes = new Byte[Marshal.SizeOf(typeof(ClientHandler.GameStateData))];
                ////CONVERT THIS SENDING CODE INTO A FUNCTION MAYBE
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
        }
    }

    void Update()
    {
        if (server != null)
        {
            switch (mGameState)
            {
                case GameState.GAME_STARTED:
                    //Handles in-game networking
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
                    int netMessagesCount = server.ReceiveMessagesOnListenSocket(listenSocket, netMessages, MAX_MESSAGES);
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

                            ClientHandler.TypeFinder packetType = (ClientHandler.TypeFinder)Marshal.PtrToStructure(ptPoit, typeof(ClientHandler.TypeFinder));

                            switch ((ClientHandler.PacketType)packetType.type)
                            {
                                case ClientHandler.PacketType.PLAYER_DATA:
                                    ClientHandler.PlayerData packetData = (ClientHandler.PlayerData)Marshal.PtrToStructure(ptPoit, typeof(ClientHandler.PlayerData));
                                    handlePlayerData(packetData);
                                    break;
                                case ClientHandler.PacketType.STORE_PLAYER_RESULTS:
                                    HandleReceivePlayerResult();
                                    break;
                                case ClientHandler.PacketType.OFFENSIVE_BOMB_DATA:
                                    ClientHandler.OffensiveBombData data = (ClientHandler.OffensiveBombData)Marshal.PtrToStructure(ptPoit, typeof(ClientHandler.OffensiveBombData));
                                    EventSystem.OffensiveBombAttack(data.pos);               //Spawns offensive bomb to server client
                                    SendBombDataToAllOtherClients(data.pos, data.playerID);  //SPawns offensive bombs to all other clients
                                    break;
                            }
                            Marshal.FreeHGlobal(ptPoit);
                        }
                    }
#endif
                    break;
                case GameState.SEARCHING_FOR_PLAYERS:    
                    //Handles joining players
                    server.DispatchCallback(serverNetworkingUtils);

                    ////Refactor this to reset packet send time for actual game maybe (and to look better)
                    if (mPacketSendTime >= PACKET_TARGET_SEND_TIME)    
                    {
                        SendGameJoinMessage();
                        BroadcastPlayerCount();
                        mPacketSendTime = 0.0f;
                    }
                    mPacketSendTime += Time.deltaTime;
                    break;
                case GameState.RESULTS_SCREEN:          
                    //Handles result screen logic
                    server.DispatchCallback(serverNetworkingUtils);
                    BroadcastResults();
                    break;
            }
        }
    }

    //Sends player position data to all connected clients
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
                        ClientHandler.PlayerData playerData = new ClientHandler.PlayerData();
                        playerData.pos = player.playerOBJ.transform.position;
                        playerData.speed = 12;
                        playerData.type = (int)ClientHandler.PacketType.PLAYER_DATA;
                        playerData.playerID = playerID;

                        Byte[] bytes = new Byte[Marshal.SizeOf(typeof(ClientHandler.PlayerData))];
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
                    }
                }
            }
        }
    }

    //Sends players joined count to all clients lobby screens
    private void BroadcastPlayerCount()
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                foreach (uint playerID in mPlayers.Keys)
                {
                    ClientHandler.PlayerCountData playerCount = new ClientHandler.PlayerCountData();
                    playerCount.type = (int)ClientHandler.PacketType.PLAYER_COUNT;
                    playerCount.playerCount = connectedClients.Count + 1;

                    Byte[] bytes = new Byte[Marshal.SizeOf(typeof(ClientHandler.PlayerCountData))];
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

    //Sends all players' results to all clients result screens
    private void BroadcastResults()
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                foreach (uint playerID in mPlayerResults.Keys)
                {
                    PlayerStoredResultData player = mPlayerResults[playerID];

                    ClientHandler.PlayerSendResultData playerSendResultData = new ClientHandler.PlayerSendResultData();
                    playerSendResultData.type = (int)ClientHandler.PacketType.SEND_RESULT;
                    playerSendResultData.playerID = playerID;
                    playerSendResultData.time = player.time;
                    playerSendResultData.points = player.points;
                    playerSendResultData.name = player.name;
                    playerSendResultData.score = player.score;

                    IntPtr ptr = IntPtr.Zero;
                    byte[] bytes = new byte[Marshal.SizeOf(typeof(ClientHandler.PlayerSendResultData))];

                    try
                    {
                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ClientHandler.PlayerSendResultData)));
                        Marshal.StructureToPtr(playerSendResultData, ptr, true);
                        Marshal.Copy(ptr, bytes, 0, Marshal.SizeOf(typeof(ClientHandler.PlayerSendResultData)));
                    }
                    finally
                    {
                        server.SendMessageToConnection(connectedClients[i], bytes);
                        Marshal.FreeHGlobal(ptr);
                    }

                    EventSystem.fireEvent(new ReceiveResultEvent(playerSendResultData));
                }
            }
        }
    }

    //Broadcasts joining IP to clients wanting to join
    private void SendGameJoinMessage()
    {
        if (mServerIP != null && !mDebugMode)
        {
            UDPListener.SendIP(mServerIP.ToString(), PlayerInfo.PlayerName);
        }
    }

    //Handles registering a newly joined player (to be able to send its own connection ID on the server for identification)
    private void handlePlayerJoining(uint playerID)
    {
        if (!mPlayers.ContainsKey(playerID))
        {
            connectedClients.Add(playerID);

            PlayerGameData player = new PlayerGameData();
            player.init();

            mPlayers.Add(playerID, player);

            ClientHandler.RegisterPlayer playerData = new ClientHandler.RegisterPlayer();
            playerData.type = (int)ClientHandler.PacketType.REGISTER_PLAYER;
            playerData.playerID = playerID;

            Byte[] bytes = new Byte[Marshal.SizeOf(typeof(ClientHandler.RegisterPlayer))];
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

            //Refactor to use player dictionary
            EventSystem.fireEvent(new PlayerCountChangedEvent(connectedClients.Count + 1)); 
        }
    }

    //Handles a client leaving/disconnecting
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
                // Refactor to use player dictionary
                EventSystem.fireEvent(new PlayerCountChangedEvent(connectedClients.Count + 1)); 
            }
        }
    }

    //Handles intaking player position data
    private void handlePlayerData(ClientHandler.PlayerData playerData)
    {
        PlayerGameData player = new PlayerGameData();
        if (!mPlayers.ContainsKey(playerData.playerID))
        {
            player.playerOBJ = Instantiate(m_PlayerHologramPrefab); ;
            player.init();
            player.playerInterpolationTracker = 0.0f;

            mPlayers.Add(playerData.playerID, player);
        }
        //Maybe refactor this to instantiate holograms when HandleStartGame is run
        else if (mPlayers[playerData.playerID].playerOBJ == null)    
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

    //Handles interpolating player poses for clients
    private void handleInterpolatePlayerPoses()
    {
        var keys = mPlayers.Keys;
        for (int i = 0; i < mPlayers.Count; i++)
        {
            uint id = keys.ElementAt(i);
            if (mPlayers[id].playerPoses.Count > 0 && mPlayers[id].playerOBJ != null)
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

    //Handles receiving player result data from clients on their death (or win or disconnect to be implimented)
    private void HandleReceivePlayerResult()
    {
        ClientHandler.PlayerSendResultData playerReceivedResult = new ClientHandler.PlayerSendResultData();
        int size = Marshal.SizeOf(playerReceivedResult);
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(messageDataBuffer, 0, ptr, size);

            playerReceivedResult = (ClientHandler.PlayerSendResultData)Marshal.PtrToStructure(ptr, playerReceivedResult.GetType());

            PlayerStoredResultData playerStoreResult = new()
            {
                name = playerReceivedResult.name,
                points = playerReceivedResult.points,
                time = playerReceivedResult.time,
                score = playerReceivedResult.score,
                resultContext = playerReceivedResult.resultContext
            };
            Debug.Log("REceived result from : " + playerReceivedResult.playerID);
            mPlayerResults.Add(playerReceivedResult.playerID, playerStoreResult);

            PlayerGameData prevData = mPlayers[playerReceivedResult.playerID];

            switch (playerReceivedResult.resultContext)
            {
                case ResultContext.PLAYER_WON:
                    //Handles sending win data event for other handling of a player win (from client)
                    //The z = 2 makes the grave show up in front the bullets
                    EventSystem.SendPlayerWinData(false, new Vector3(prevData.playerOBJ.transform.position.x, prevData.playerOBJ.transform.position.y, -2));
                    break;
                case ResultContext.PLAYER_DIED:
                    //Handles sending death data event for other handling of a player death (from client)
                    //The z = 1 makes the grave show up behind the bullets
                    EventSystem.SendPlayerDeathData(false, new Vector3(prevData.playerOBJ.transform.position.x, prevData.playerOBJ.transform.position.y, 1));
                    break;
                case ResultContext.PLAYER_DISCONNECTED:
                    break;
            }

            SendPlayerFinishToAllOtherClients(playerReceivedResult.playerID, playerReceivedResult.resultContext);

            //Removes finished player
            Destroy(prevData.playerOBJ);
            prevData.playerOBJ = null;
            mPlayers[playerReceivedResult.playerID] = prevData;

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

    //Sends offensive bomb data to all other clients exept for the one that fired thw bomb
    private void SendBombDataToAllOtherClients(Vector2 pos, uint owningClient)
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                if (connectedClients[i] != owningClient)
                {
                    ClientHandler.OffensiveBombData bombData = new()
                    {
                        pos = pos,
                        playerID = connectedClients[i],
                        type = (int)ClientHandler.PacketType.OFFENSIVE_BOMB_DATA
                    };

                    Byte[] bytes = new Byte[Marshal.SizeOf(typeof(ClientHandler.OffensiveBombData))];
                    GCHandle pinStructure = GCHandle.Alloc(bombData, GCHandleType.Pinned);
                    try
                    {
                        Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                    }
                    finally
                    {
                        Debug.Log("SENDING BOMB DATA");
                        server.SendMessageToConnection(connectedClients[i], bytes);
                        pinStructure.Free();
                    }
                }
            }
        }
    }

    private void SendBombDataFromServer(Vector2 pos)
    {
        SendBombDataToAllOtherClients(pos, serverPlayerID);
    }

    //On server player die
    private void HandleSendPlayerResults(ServerHandler.ResultContext resContext)
    {
        PlayerStoredResultData playerStoreResult = new()
        {
            name = PlayerInfo.PlayerName,
            points = PlayerInfo.PlayerPoints,
            time = Time.time - PlayerInfo.PlayerTime,
            score = PlayerInfo.CalculateScore(),
            resultContext = resContext
        };

        mPlayerResults.Add(serverPlayerID, playerStoreResult);

        SendPlayerFinishToAllOtherClients(serverPlayerID, resContext);

        //Check if game is finished (all players are done playing)
        if (CheckIfGameFinished())
        {
            HandleGameFinish();
        }
    }

    //This function will be called when a client player dies/finished (and also disconnects probobly)
    private bool CheckIfGameFinished()
    {
        //Game will finish when all results are in (Add safety exception in case not but level is done maybe)
        return mPlayers.Count == mPlayerResults.Count;
    }

    private void SendPlayerFinishToAllOtherClients(uint clientThatDied, ServerHandler.ResultContext finishReason)
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                if (connectedClients[i] != clientThatDied)
                {
                    ClientHandler.OtherClientFinishState data = new()
                    {
                        playerID = clientThatDied,
                        type = (int)ClientHandler.PacketType.OTHER_PLAYER_FINISH,
                        finishState = finishReason
                    };

                    Byte[] bytes = new Byte[Marshal.SizeOf(typeof(ClientHandler.OtherClientFinishState))];
                    GCHandle pinStructure = GCHandle.Alloc(data, GCHandleType.Pinned);
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

    //Ends the netowrking session
    private void EndSession()
    {
        HandleCloseConnection();
        Destroy(gameObject);
    }
}