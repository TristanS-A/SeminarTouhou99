using AOT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.Sockets;

public class ClientHandler : MonoBehaviour
{
    //Serialized buttons and prefabs
    [SerializeField] private Button testClientButton;
    [SerializeField] private GameObject m_PlayerHologramPrefab;

    //Networking members (client)
    private NetworkingSockets client;
    private uint connectionIDOnServer = 0;
    private uint serverConnection = 0;
    private StatusCallback clientNetworkingUtils;
    private NetworkingUtils utils = new NetworkingUtils();

    //Player data storage
    private Dictionary<uint, ServerHandler.PlayerGameData> mPlayers = new();

    //Packet sending data
    private float mPacketSendTime = 0.0f;
    private const float PACKET_TARGET_SEND_TIME = 0.033f;
    private bool mShouldSendData = true;

    //Gamestate storage
    private ServerHandler.GameState mGameState = ServerHandler.GameState.NONE;

    //Singleton instance
    public static ClientHandler instance;

    //Netowrking packet message data
    private const int MAX_MESSAGES = 20;
    private NetworkingMessage[] netMessages = new NetworkingMessage[MAX_MESSAGES];
    private byte[] messageDataBuffer = new byte[256];

    //Debug mode for running multiple games on a single computer (Needs manual input IP to connect)
    [SerializeField] public bool mDebugMode = false;
    [SerializeField] public string mDebugDebugIP;

    //IP Joining struct
    public struct JoinIPData
    {
        public GameObject joinUIOBJ;
        public string name;

        public void setJoinUIOBJ(GameObject newJoinUIOBJ)
        {
            joinUIOBJ = newJoinUIOBJ;
        }
    }

    //Networking packet types
    public enum PacketType
    {
        PLAYER_DATA,
        REGISTER_PLAYER,
        PLAYER_COUNT,
        GAME_STATE,
        STORE_PLAYER_RESULTS,
        SEND_RESULT,
        OFFENSIVE_BOMB_DATA,
        OTHER_PLAYER_FINISH
    }

    ////Packet structs

    [StructLayout(LayoutKind.Sequential)]
    public struct TypeFinder
    {
        public int type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerData
    {
        public int type;
        public uint playerID;
        public Vector3 pos;
        public float speed;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PlayerName
    {
        public int type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RegisterPlayer
    {
        public int type;
        public uint playerID;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerCountData
    {
        public int type;
        public int playerCount;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PlayerSendResultData
    {
        public int type;
        public uint playerID;
        public float time;
        public int points;
        public int score;
        public ServerHandler.ResultContext resultContext;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string name;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct GameStateData
    {
        public int type;
        public int gameState;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct OffensiveBombData
    {
        public int type;
        public uint playerID;
        public Vector3 pos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OtherClientFinishState
    {
        public int type;
        public uint playerID;
        public ServerHandler.ResultContext finishState;
    }

    private void OnEnable()
    {
        EventSystem.gameStarted += HandleGameStart;
        EventSystem.OnSendPlayerResultData += HandleSendPlayerResults;
        EventSystem.onEndGameSession += EndSession;
        EventSystem.OnFireOffensiveBomb += SendBombData;
    }

    private void OnDisable()
    {
        EventSystem.gameStarted -= HandleGameStart;
        EventSystem.OnSendPlayerResultData -= HandleSendPlayerResults;
        EventSystem.onEndGameSession -= EndSession;
        EventSystem.OnFireOffensiveBomb -= SendBombData;
    }

    //Handles closing of netorking lib and connections
    private void OnApplicationQuit()
    {
        HandleCloseConnection();

        Valve.Sockets.Library.Deinitialize();
        Debug.Log("Quit and Socket Lib Deanitialized");
    }

    private void HandleCloseConnection()
    {
        Debug.Log("Closing Connection");
        if (client != null)
        {
            client.CloseConnection(serverConnection);
        }

        if (!mDebugMode)
        {
            UDPListener.CloseClient();
        }

        Valve.Sockets.Library.Deinitialize();
    }

    //Handle singleton instance no-replication and networking setup
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        Valve.Sockets.Library.Initialize();

        if (testClientButton != null)
        {
            testClientButton.onClick.AddListener(SwitchToLobbyScene);
        }

        DebugCallback debugCallback = (DebugType type, string message) =>
        {
            //Debug.Log("Client Debug - Type: " + type + ", Message: " + message);
        };

        utils.SetDebugCallback(DebugType.Everything, debugCallback);
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void SwitchToLobbyScene()
    {
        DontDestroyOnLoad(transform.gameObject);
        SceneManager.LoadScene(1);
    }

    public void RunClientSetUp()
    {
        Debug.Log("Starting Client...");

        client = new NetworkingSockets();
        clientNetworkingUtils = OnClientStatusUpdate;

        if (!mDebugMode)
        {
            UDPListener.StartClient(true);
        }

        mGameState = ServerHandler.GameState.LOOKING_FOR_HOST;
    }

    private void InitClientJoin(string ip)
    {
        serverConnection = 0;

        Address address = new Address();

        address.SetAddress(ip, 5000);

        serverConnection = client.Connect(address);

#if VALVESOCKETS_SPAN
        message = (in NetworkingMessage netMessage) =>
        {
            Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

            netMessage.CopyTo(messageDataBuffer);
            //netMessage.Destroy();

            string result = Encoding.ASCII.GetString(messageDataBuffer);
            Debug.Log(result);
        };
#else
        const int maxMessages = 20;

        NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
#endif

        SceneManager.LoadScene(2);
    }

    [MonoPInvokeCallback(typeof(StatusCallback))]
    void OnClientStatusUpdate(StatusInfo info, System.IntPtr context)
    {
        switch (info.connectionInfo.state)
        {
            case ConnectionState.None:
                break;
            case ConnectionState.Connected:
                serverConnection = info.connection;
                Debug.Log("Client connected to server - ID: " + serverConnection);
                break;

            case ConnectionState.ClosedByPeer:
            case ConnectionState.ProblemDetectedLocally:
                client.CloseConnection(serverConnection);
                Debug.Log("Client disconnected from server");
                break;
        }
    }

    //Joining trigger on a IP display button ti join that server;
    public void JoinHost(BaseEventData eventData)
    {
        if (eventData.selectedObject != null)
        {
            InitClientJoin(eventData.selectedObject.GetComponent<IPStorageAttachment>().IP);
            mGameState = ServerHandler.GameState.SEARCHING_FOR_PLAYERS;
        }
    }


    void Update()
    {
        if (client != null)
        {
            client.DispatchCallback(clientNetworkingUtils);

            switch (mGameState)
            {
                case ServerHandler.GameState.GAME_STARTED:
                    handleInterpolatePlayerPoses();
                    if (mPacketSendTime >= PACKET_TARGET_SEND_TIME && mShouldSendData)
                    {
                        SendPlayerData();
                        mPacketSendTime = 0.0f;
                    }
                    mPacketSendTime += Time.deltaTime;
                    HandleNetMessages();
                    break;

                case ServerHandler.GameState.LOOKING_FOR_HOST:
                    break;
                case ServerHandler.GameState.SEARCHING_FOR_PLAYERS:
                    HandleNetMessages();
                    break;
                case ServerHandler.GameState.RESULTS_SCREEN:
                    HandleNetMessages();
                    break;
            }
        }
    }

    private void HandleNetMessages()
    {
#if VALVESOCKETS_SPAN
                    client.ReceiveMessagesOnConnection(serverConnection, message, 20);
#else
        int netMessagesCount = client.ReceiveMessagesOnConnection(serverConnection, netMessages, MAX_MESSAGES);

        if (netMessagesCount > 0)
        {
            for (int i = 0; i < netMessagesCount; i++)
            {
                ref NetworkingMessage netMessage = ref netMessages[i];

                Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

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
                    case PacketType.REGISTER_PLAYER:
                        RegisterPlayer playerRegisterPacketData = (RegisterPlayer)Marshal.PtrToStructure(ptPoit, typeof(RegisterPlayer));
                        handleRegisterPlayer(playerRegisterPacketData);
                        break;
                    case PacketType.PLAYER_COUNT:
                        PlayerCountData playerCountData = (PlayerCountData)Marshal.PtrToStructure(ptPoit, typeof(PlayerCountData));
                        EventSystem.fireEvent(new PlayerCountChangedEvent(playerCountData.playerCount));
                        break;
                    case PacketType.SEND_RESULT:
                        PlayerSendResultData playerSendResultData = (PlayerSendResultData)Marshal.PtrToStructure(ptPoit, typeof(PlayerSendResultData));
                        EventSystem.fireEvent(new ReceiveResultEvent(playerSendResultData));
                        break;
                    case PacketType.GAME_STATE:
                        GameStateData gameStateData = (GameStateData)Marshal.PtrToStructure(ptPoit, typeof(GameStateData));
                        switch ((EventType.EventTypes)gameStateData.gameState)
                        {
                            case EventType.EventTypes.START_GAME:
                                SceneManager.LoadScene(3);
                                break;
                            case EventType.EventTypes.GAME_FINISHED:
                                SceneManager.LoadScene(4);
                                mGameState = ServerHandler.GameState.RESULTS_SCREEN;
                                break;
                        }
                        break;
                    case PacketType.OFFENSIVE_BOMB_DATA:
                        OffensiveBombData data = (OffensiveBombData)Marshal.PtrToStructure(ptPoit, typeof(OffensiveBombData));
                        //EventSystem.OffensiveBombAttack(data.pos);
                        mPlayers[data.playerID].playerOBJ.GetComponent<HologramPlayer>().SpawnOffensiveBomb(data.pos);
                        break;
                    case PacketType.OTHER_PLAYER_FINISH:
                        OtherClientFinishState otherPlayerData = (OtherClientFinishState)Marshal.PtrToStructure(ptPoit, typeof(OtherClientFinishState));
                        HandleOtherPlayerFinish(otherPlayerData.playerID, otherPlayerData.finishState);
                        break;
                }

                Marshal.FreeHGlobal(ptPoit);
            }
        }
#endif
    }

    //Sends player position data to server
    void SendPlayerData()
    {
        if (client != null && mPlayers.ContainsKey(connectionIDOnServer) && mPlayers[connectionIDOnServer].playerOBJ != null)
        {
            PlayerData playerData = new PlayerData();
            GameObject player = mPlayers[connectionIDOnServer].playerOBJ;
            playerData.pos = player.transform.position;
            playerData.speed = 12;
            playerData.type = (int)PacketType.PLAYER_DATA;
            playerData.playerID = connectionIDOnServer;


            Byte[] bytes = new Byte[Marshal.SizeOf(typeof(PlayerData))];
            GCHandle pinStructure = GCHandle.Alloc(playerData, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
            }
            finally
            {
                Debug.Log("SENDING PLAYER DATA");
                client.SendMessageToConnection(serverConnection, bytes);
                pinStructure.Free();
            }

            //byte[] bytes = Encoding.ASCII.GetBytes(playerData);
            //server.SendMessageToConnection(connectedClients[i], bytes);

            //messages.Add(message);
        }
    }

    //Handles intaking player position data
    private void handlePlayerData(PlayerData playerData)
    {
        if (playerData.playerID != connectionIDOnServer)
        {
            ServerHandler.PlayerGameData player = new ServerHandler.PlayerGameData();
            if (!mPlayers.ContainsKey(playerData.playerID))
            {
                player.playerOBJ = Instantiate(m_PlayerHologramPrefab);
                player.init();
                player.playerInterpolationTracker = 0.0f;

                mPlayers.Add(playerData.playerID, player);
            }
            else if (mPlayers[playerData.playerID].playerOBJ == null)    //Maybe refactor this to instantiate holograms when HandleStartGame is run
            {
                return;
            }

            mPlayers[playerData.playerID].playerPoses.Clear();
            Debug.Log(mPlayers[playerData.playerID].playerPoses);
            mPlayers[playerData.playerID].playerPoses.Add(mPlayers[playerData.playerID].playerOBJ.transform.position);
            mPlayers[playerData.playerID].playerPoses.Add(playerData.pos);

            ServerHandler.PlayerGameData pData = mPlayers[playerData.playerID];
            pData.playerInterpolationTracker = 0.0f;
            mPlayers[playerData.playerID] = pData;
        }
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

                ServerHandler.PlayerGameData pData = mPlayers[id];
                pData.playerInterpolationTracker += Time.deltaTime;
                mPlayers[id] = pData;

                if (mPlayers[id].playerInterpolationTracker >= PACKET_TARGET_SEND_TIME)
                {
                    mPlayers[id].playerPoses.Clear();
                }
            }
        }
    }

    private void HandleGameStart(GameObject player)
    {
        mGameState = ServerHandler.GameState.GAME_STARTED;

        var keys = mPlayers.Keys;
        for (int i = 0; i < mPlayers.Count; i++)
        {
            if (keys.ElementAt(i) != connectionIDOnServer)
            {
                ServerHandler.PlayerGameData pD = new();
                pD.init();
                pD.playerOBJ = Instantiate(m_PlayerHologramPrefab);
                mPlayers[keys.ElementAt(i)] = pD;
            }
        }

        ServerHandler.PlayerGameData pData = new();
        pData.init();
        pData.playerOBJ = player;
        mPlayers[connectionIDOnServer] = pData;
    }

    //Bad archatecture for now (this function should ONLY be called after AddClientPlayer). Look into refactoring.
    private void handleRegisterPlayer(RegisterPlayer playerData)
    {
        connectionIDOnServer = playerData.playerID;

        ServerHandler.PlayerGameData pData = new();
        pData.init();

        mPlayers.Add(connectionIDOnServer, pData);
    }

    private void HandleSendPlayerResults(ServerHandler.ResultContext resContext)
    {
        PlayerSendResultData playerResults = new PlayerSendResultData();
        playerResults.type = (int)ClientHandler.PacketType.STORE_PLAYER_RESULTS;
        playerResults.playerID = connectionIDOnServer;
        playerResults.name = PlayerInfo.PlayerName;
        playerResults.time = Time.time - PlayerInfo.PlayerTime;
        playerResults.points = PlayerInfo.PlayerPoints;
        playerResults.score = PlayerInfo.CalculateScore();
        playerResults.resultContext = resContext;

        IntPtr ptr = IntPtr.Zero;
        byte[] bytes = new byte[Marshal.SizeOf(typeof(PlayerSendResultData))];

        try
        {
            ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PlayerSendResultData)));
            Marshal.StructureToPtr(playerResults, ptr, true);
            Marshal.Copy(ptr, bytes, 0, Marshal.SizeOf(typeof(PlayerSendResultData)));
        }
        finally
        {
            client.SendMessageToConnection(serverConnection, bytes);
            Marshal.FreeHGlobal(ptr);
        }

        mShouldSendData = false;
    }

    //Ends the netowrking session
    private void EndSession()
    {
        HandleCloseConnection();
        Destroy(gameObject);
    }

    //Sends offensive bomb data to server
    private void SendBombData(Vector2 pos)
    {
        if (client != null && mPlayers.ContainsKey(connectionIDOnServer) && mPlayers[connectionIDOnServer].playerOBJ != null)
        {
            OffensiveBombData bombData = new()
            {
                pos = pos,
                playerID = connectionIDOnServer,
                type = (int)PacketType.OFFENSIVE_BOMB_DATA
            };

            Byte[] bytes = new Byte[Marshal.SizeOf(typeof(OffensiveBombData))];
            GCHandle pinStructure = GCHandle.Alloc(bombData, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(pinStructure.AddrOfPinnedObject(), bytes, 0, bytes.Length);
            }
            finally
            {
                Debug.Log("SENDING BOMB DATA");
                client.SendMessageToConnection(serverConnection, bytes);
                pinStructure.Free();
            }
        }
    }

    private void HandleOtherPlayerFinish(uint otherPlayerID, ServerHandler.ResultContext finishReason)
    {
        ServerHandler.PlayerGameData prevData = mPlayers[otherPlayerID];

        switch (finishReason)
        {
            case ServerHandler.ResultContext.PLAYER_WON:
                //Handles sending win data event for other handling of a player win (from client)
                //The z = 2 makes the grave show up in front the bullets
                EventSystem.SendPlayerWinData(false, new Vector3(prevData.playerOBJ.transform.position.x, prevData.playerOBJ.transform.position.y, prevData.playerOBJ.transform.position.z));
                break;
            case ServerHandler.ResultContext.PLAYER_DIED:
                //Handles sending death data event for other handling of a player death (from client)
                EventSystem.SendPlayerDeathData(false, new Vector3(prevData.playerOBJ.transform.position.x, prevData.playerOBJ.transform.position.y, prevData.playerOBJ.transform.position.z));
                break;
            case ServerHandler.ResultContext.PLAYER_DISCONNECTED:
                break;
        }

        Destroy(mPlayers[otherPlayerID].playerOBJ);
        prevData.playerOBJ = null;
        mPlayers[otherPlayerID] = prevData;
    }
}