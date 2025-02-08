using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using Valve.Sockets;
using static clientHandler;

public class clientHandler : MonoBehaviour
{
    [SerializeField] private Button testClientButton;
    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private GameObject m_PlayerHologramPrefab;
    private NetworkingSockets client;
    private uint connectionIDOnServer = 0;
    private uint serverConnection = 0;
    private StatusCallback clientStatusCallback;
    NetworkingUtils clientNetworkingUtils = new NetworkingUtils();
    Dictionary<uint, GameObject> players = new Dictionary<uint, GameObject>();
    Dictionary<uint, List<Vector3>> playerPoses = new Dictionary<uint, List<Vector3>>();
    Dictionary<uint, float> playerInterpolationTracker = new Dictionary<uint, float>();
    private float mPacketSendTime = 0.0f;
    private const float PACKET_TARGET_SEND_TIME = 0.033f;

    //MessageCallback message;
    const int maxMessages = 20;
    NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];

    byte[] messageDataBuffer = new byte[256];

    public enum PacketType
    {
        PLAYER_DATA,
        REGISTER_PLAYER
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
        public Vector2 pos;
        public float speed;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct RegisterPlayer
    {
        public int type;
        public uint playerID;
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

        clientNetworkingUtils.SetDebugCallback(DebugType.Everything, debugCallback);
    }

    private void RunClientSetUp()
    {
        Debug.Log("Starting Client...");

        client = new NetworkingSockets();

        serverConnection = 0;

        StatusCallback status = (ref StatusInfo info) => {
            try
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
            catch (Exception e)
            {
                Debug.Log("ERRROR: " + e);
                return;
            }
        };

        clientNetworkingUtils.SetStatusCallback(status);

        Address address = new Address();

        address.SetAddress("::1", 5000);

        serverConnection = client.Connect(ref address);

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
    }

    // Update is called once per frame
    void Update()
    {
        if (client != null)
        {
            client.RunCallbacks();

            handleInterpolatePlayerPoses();
            if (mPacketSendTime >= PACKET_TARGET_SEND_TIME)
            {
                SendChatMessage();
                mPacketSendTime = 0.0f;
            }
            mPacketSendTime += Time.deltaTime;

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
                    }

                    Marshal.FreeHGlobal(ptPoit);
                }
            }
#endif
        }
    }

    private void FixedUpdate()
    {
        handleMovePlayer();
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

    private void handleRegisterPlayer(RegisterPlayer playerData)
    {
        connectionIDOnServer = playerData.playerID;
        players.Add(connectionIDOnServer, Instantiate(m_PlayerPrefab));
        playerPoses.Add(connectionIDOnServer, new());
        playerInterpolationTracker.Add(connectionIDOnServer, 0.0f);
    }

    private void handleMovePlayer()
    {
        if (players.ContainsKey(connectionIDOnServer))
        {
            Transform serverPlayerTransform = players[connectionIDOnServer].transform;
            if (Input.GetKey(KeyCode.W))
            {
                serverPlayerTransform.position += new Vector3(0, 0.1f, 0);
            }
            if (Input.GetKey(KeyCode.D))
            {
                serverPlayerTransform.position += new Vector3(0.1f, 0, 0);
            }
            if (Input.GetKey(KeyCode.S))
            {
                serverPlayerTransform.position += new Vector3(0, -0.1f, 0);
            }
            if (Input.GetKey(KeyCode.A))
            {
                serverPlayerTransform.position += new Vector3(-0.1f, 0, 0);
            }
        }
    }
}
