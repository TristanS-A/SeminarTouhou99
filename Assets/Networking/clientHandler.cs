using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.Sockets;
using static clientHandler;

public class clientHandler : MonoBehaviour
{
    [SerializeField] private Button testClientButton;
    [SerializeField] private GameObject m_PlayerHologramPrefab;
    [SerializeField] private GameObject m_IPDisplay;
    private GameObject mClientPlayerReference;
    private NetworkingSockets client;
    private uint connectionIDOnServer = 0;
    private uint serverConnection = 0;
    private StatusCallback clientNetworkingUtils;
    NetworkingUtils utils = new NetworkingUtils();
    Dictionary<uint, GameObject> players = new Dictionary<uint, GameObject>();
    Dictionary<uint, List<Vector3>> playerPoses = new Dictionary<uint, List<Vector3>>();
    Dictionary<uint, float> playerInterpolationTracker = new Dictionary<uint, float>();
    private float mPacketSendTime = 0.0f;
    private const float PACKET_TARGET_SEND_TIME = 0.033f;

    private Dictionary<string, GameObject> mJoinableIPs = new Dictionary<string, GameObject>();
    serverHandler.GameState mGameState = serverHandler.GameState.NONE;

    //MessageCallback message;
    const int maxMessages = 20;
    NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];

    byte[] messageDataBuffer = new byte[256];

    public enum PacketType
    {
        PLAYER_DATA,
        REGISTER_PLAYER,
        PLAYER_COUNT
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

    private void OnEnable()
    {
        eventSystem.gameStarted += HandleGameStart;
        eventSystem.ipReceived += AddIP;
    }

    private void OnDisable()
    {
        eventSystem.gameStarted -= HandleGameStart;
        eventSystem.ipReceived -= AddIP;
    }

    private void OnApplicationQuit()
    {
        client.CloseConnection(serverConnection);
        UDPListener.CloseClient();
        Valve.Sockets.Library.Deinitialize();
        Debug.Log("Quit and Socket Lib Deanitialized");
    }

    // Start is called before the first frame update
    void Start()
    {
        Valve.Sockets.Library.Initialize();

        if (testClientButton != null)
        {
            testClientButton.onClick.AddListener(RunClientSetUp);
        }

        DebugCallback debugCallback = (DebugType type, string message) =>
        {
            Debug.Log("Client Debug - Type: " + type + ", Message: " + message);
        };

        utils.SetDebugCallback(DebugType.Everything, debugCallback);
    }

    private void RunClientSetUp()
    {
        Debug.Log("Starting Client...");

        DontDestroyOnLoad(transform.gameObject);

        client = new NetworkingSockets();
        clientNetworkingUtils = OnClientStatusUpdate;

        UDPListener.StartClient(true);
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
                        SendChatMessage();
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
                        eventSystem.fireEvent(new PlayerCountChangedEvent(playerCountData.playerCount));
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

    void SendChatMessage()
    {
        if (client != null && players.ContainsKey(connectionIDOnServer))
        {
            PlayerData playerData = new PlayerData();
            GameObject player = players[connectionIDOnServer];
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

    private void HandleGameStart(GameObject player)
    {
        mClientPlayerReference = player;
    }

    //Bad archatecture for now (this function should ONLY be called after AddClientPlayer). Look into refactoring.
    private void handleRegisterPlayer(RegisterPlayer playerData)
    {
        connectionIDOnServer = playerData.playerID;
        players.Add(connectionIDOnServer, mClientPlayerReference);
        playerPoses.Add(connectionIDOnServer, new());
        playerInterpolationTracker.Add(connectionIDOnServer, 0.0f);
    }

    //private void handleMovePlayer()
    //{
    //    if (players.ContainsKey(connectionIDOnServer))
    //    {
    //        Transform serverPlayerTransform = players[connectionIDOnServer].transform;
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
