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
using static clientHandler;

public class serverHandler : MonoBehaviour
{
    [SerializeField] private Button testServerButton;
    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private GameObject m_PlayerHologramPrefab;
    Dictionary<uint, GameObject> players = new Dictionary<uint, GameObject>();
    private NetworkingSockets server;
    private uint serverPlayerID = 0;
    private uint pollGroup;
    private NetworkingUtils serverNetworkingUtils = new NetworkingUtils();
    private uint listenSocket;

    //MessageCallback message;
    const int maxMessages = 20;
    NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
    byte[] messageDataBuffer = new byte[256];

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

        players.Add(0, Instantiate(m_PlayerPrefab));

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

    // Update is called once per frame
    void Update()
    {
        if (server != null)
        {
            server.RunCallbacks();

            SendChatMessage();

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
        }
    }

    private void FixedUpdate()
    {
        handleMovePlayer();
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
        //    SendChatMessage(inputString);
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

    void SendChatMessage()
    {
        if (server != null)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                foreach (uint playerID in players.Keys)
                {
                    clientHandler.PlayerData playerData = new clientHandler.PlayerData();
                    GameObject player = players[playerID];
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
                        Debug.Log("SENDING PLAYER DATA");
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

    private void handlePlayerJoining(uint playerID)
    {
        if (!players.ContainsKey(playerID))
        {
            connectedClients.Add(playerID);
            players.Add(playerID, Instantiate(m_PlayerHologramPrefab));

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
        }
    }

    private void handlePlayerLeaving(uint playerID)
    {
        if (players.ContainsKey(playerID))
        {
            connectedClients.Remove(playerID);
            players.Remove(playerID);
            server.CloseConnection(playerID);
        }
    }

    private void handlePlayerData(clientHandler.PlayerData playerData)
    {
        GameObject playerOBJ;
        if (!players.ContainsKey(playerData.playerID))
        {
            playerOBJ = Instantiate(m_PlayerHologramPrefab);
            players.Add(playerData.playerID, playerOBJ);
        }
        else
        {
            playerOBJ = players[playerData.playerID];
        }

        playerOBJ.transform.position = playerData.pos;
    }

    private void handleMovePlayer()
    {
        if (players.ContainsKey(serverPlayerID))
        {
            Transform serverPlayerTransform = players[serverPlayerID].transform;
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
