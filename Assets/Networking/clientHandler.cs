using AOT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.Sockets;
using static clientHandler;
using static serverHandler;

public class clientHandler : MonoBehaviour
{
    [SerializeField] private Button testClientButton;
    [SerializeField] private GameObject m_PlayerHologramPrefab;
    [SerializeField] private GameObject m_IPDisplay;
    private NetworkingSockets client;
    private uint connectionIDOnServer = 0;
    private uint serverConnection = 0;
    private StatusCallback clientNetworkingUtils;
    NetworkingUtils utils = new NetworkingUtils();

    Dictionary<uint, serverHandler.PlayerGameData> mPlayers = new();
    Dictionary<uint, serverHandler.PlayerStoredResultData> mPlayerResults = new();

    private float mPacketSendTime = 0.0f;
    private const float PACKET_TARGET_SEND_TIME = 0.033f;

    private Dictionary<string, GameObject> mJoinableIPs = new Dictionary<string, GameObject>();
    serverHandler.GameState mGameState = serverHandler.GameState.NONE;

    public static clientHandler instance;

    //MessageCallback message;
    const int maxMessages = 20;
    NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];

    byte[] messageDataBuffer = new byte[256];

    [SerializeField] private bool mDebugMode = false;
    [SerializeField] private string mDebugDebugIP;

    public enum PacketType
    {
        PLAYER_DATA,
        REGISTER_PLAYER,
        PLAYER_COUNT,
        GAME_STATE,
        STORE_PLAYER_RESULTS,
        SEND_RESULT
    }

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

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string name;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct GameStateData
    {
        public int type;
        public int gameState;
    };

    private void OnEnable()
    {
        EventSystem.gameStarted += HandleGameStart;
        EventSystem.ipReceived += AddIP;
        EventSystem.onPlayerDeath += OnPlayerDie;
        EventSystem.onEndGameSession += EndSession;
    }

    private void OnDisable()
    {
        EventSystem.gameStarted -= HandleGameStart;
        EventSystem.ipReceived -= AddIP;
        EventSystem.onPlayerDeath -= OnPlayerDie;
        EventSystem.onEndGameSession -= EndSession;
    }

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
    }

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(this);
        }

        instance = this;

        Valve.Sockets.Library.Initialize();

        if (testClientButton != null)
        {
            testClientButton.onClick.AddListener(RunClientSetUp);
        }

        DebugCallback debugCallback = (DebugType type, string message) =>
        {
            //Debug.Log("Client Debug - Type: " + type + ", Message: " + message);
        };

        utils.SetDebugCallback(DebugType.Everything, debugCallback);
    }

    private void RunClientSetUp()
    {
        Debug.Log("Starting Client...");

        DontDestroyOnLoad(transform.gameObject);

        client = new NetworkingSockets();
        clientNetworkingUtils = OnClientStatusUpdate;

        if (!mDebugMode)
        {
            UDPListener.StartClient(true);
        }
        else
        {
            mJoinableIPs.Add(mDebugDebugIP, null);
        }

        mGameState = serverHandler.GameState.LOOKING_FOR_HOST;

        SceneManager.LoadScene(1);
    }

    private void AddIP(string ip)
    {
        if (!mJoinableIPs.ContainsKey(ip))
        {
            mJoinableIPs.Add(ip, null);
        }
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
                Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA Client connected to server - ID: " + serverConnection);
                break;

            case ConnectionState.ClosedByPeer:
            case ConnectionState.ProblemDetectedLocally:
                client.CloseConnection(serverConnection);
                Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA Client disconnected from server");
                break;
        }
    }

    private void DisplayJoinableIPs()
    {
        int ipYCord = 220;
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            var keys = mJoinableIPs.Keys;
            for (int i = 0; i < keys.Count; i++)
            {
                if (mJoinableIPs[keys.ElementAt(i)] == null)
                {
                    Canvas canvas = FindObjectOfType<Canvas>();
                    GameObject newIPDisplay = Instantiate(m_IPDisplay, canvas.transform);
                    mJoinableIPs[keys.ElementAt(i)] = newIPDisplay;

                    Button joinB = newIPDisplay.GetComponentInChildren<Button>();
                    TextMeshProUGUI joinBText = joinB.GetComponentInChildren<TextMeshProUGUI>();

                    EventTrigger trigger = newIPDisplay.GetComponentInChildren<EventTrigger>();
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerDown;
                    entry.callback.AddListener((data) => { JoinHost((BaseEventData)data); });
                    trigger.triggers.Add(entry);
                }

                GameObject displayOBJ = mJoinableIPs[keys.ElementAt(i)];
                TextMeshProUGUI hostName = displayOBJ.GetComponentInChildren<TextMeshProUGUI>();
                Button joinButton = displayOBJ.GetComponentInChildren<Button>();
                TextMeshProUGUI joinButtonTextOBJ = joinButton.GetComponentInChildren<TextMeshProUGUI>();

                displayOBJ.transform.position = new Vector3(displayOBJ.transform.position.x, ipYCord, displayOBJ.transform.position.z);
                hostName.text = keys.ElementAt(i);
                joinButtonTextOBJ.text = "Join!";
                ipYCord -= (int)hostName.rectTransform.rect.height;
            }
        }
    }

    public void JoinHost(BaseEventData eventData)
    {
        if (eventData.selectedObject != null)
        {
            InitClientJoin(eventData.selectedObject.GetComponentInParent<TextMeshProUGUI>().text);
            mGameState = serverHandler.GameState.SEARCHING_FOR_PLAYERS;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (client != null)
        {
            client.DispatchCallback(clientNetworkingUtils);

            switch (mGameState)
            {
                case serverHandler.GameState.GAME_STARTED:
                    handleInterpolatePlayerPoses();
                    if (mPacketSendTime >= PACKET_TARGET_SEND_TIME)
                    {
                        SendPlayerData();
                        mPacketSendTime = 0.0f;
                    }
                    mPacketSendTime += Time.deltaTime;
                    HandleNetMessages();
                    break;

                case serverHandler.GameState.LOOKING_FOR_HOST:
                    DisplayJoinableIPs();
                    break;
                case serverHandler.GameState.SEARCHING_FOR_PLAYERS:
                    HandleNetMessages();
                    break;
                case serverHandler.GameState.RESULTS_SCREEN:
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
        int netMessagesCount = client.ReceiveMessagesOnConnection(serverConnection, netMessages, maxMessages);

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
                                mGameState = GameState.RESULTS_SCREEN;
                                break;
                        }
                        break;
                }

                Marshal.FreeHGlobal(ptPoit);
            }
        }
#endif
    }

    private void FixedUpdate()
    {
        //handleMovePlayer();
    }

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

    private void handlePlayerData(PlayerData playerData)
    {
        if (playerData.playerID != connectionIDOnServer)
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
            Debug.Log(mPlayers[playerData.playerID].playerPoses);
            mPlayers[playerData.playerID].playerPoses.Add(mPlayers[playerData.playerID].playerOBJ.transform.position);
            mPlayers[playerData.playerID].playerPoses.Add(playerData.pos);

            PlayerGameData pData = mPlayers[playerData.playerID];
            pData.playerInterpolationTracker = 0.0f;
            mPlayers[playerData.playerID] = pData;
        }
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

    private void HandleGameStart(GameObject player)
    {
        mGameState = GameState.GAME_STARTED;

        var keys = mPlayers.Keys;
        for (int i = 0; i < mPlayers.Count; i++)
        {
            if (keys.ElementAt(i) != connectionIDOnServer)
            {
                PlayerGameData pD = new();
                pD.init();
                pD.playerOBJ = Instantiate(m_PlayerHologramPrefab);
                mPlayers[keys.ElementAt(i)] = pD;
            }
        }

        PlayerGameData pData = new();
        pData.init();
        pData.playerOBJ = player;
        mPlayers[connectionIDOnServer] = pData;
    }

    //Bad archatecture for now (this function should ONLY be called after AddClientPlayer). Look into refactoring.
    private void handleRegisterPlayer(RegisterPlayer playerData)
    {
        connectionIDOnServer = playerData.playerID;

        PlayerGameData pData = new();
        pData.init();

        mPlayers.Add(connectionIDOnServer, pData);
    }

    private void OnPlayerDie()
    {
        PlayerSendResultData playerResults = new PlayerSendResultData();
        playerResults.type = (int)clientHandler.PacketType.STORE_PLAYER_RESULTS;
        playerResults.playerID = connectionIDOnServer;
        playerResults.name = PlayerInfo.PlayerName;
        playerResults.time = Time.time - PlayerInfo.PlayerTime;
        playerResults.points = PlayerInfo.PlayerPoints;

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
    }

    private void EndSession()
    {
        HandleCloseConnection();
        Destroy(gameObject);
    }
}