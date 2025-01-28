using System;
using System.Collections;
using System.Text;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using Valve.Sockets;

public class clientHandler : MonoBehaviour
{
    [SerializeField] private Button testClientButton;
    private NetworkingSockets client;
    private uint clientConnection = 0;
    private StatusCallback clientStatusCallback;
    NetworkingUtils clientNetworkingUtils = new NetworkingUtils();

    //MessageCallback message;
    const int maxMessages = 20;
    NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];

    byte[] messageDataBuffer = new byte[256];

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

        clientConnection = 0;

        StatusCallback status = (ref StatusInfo info) => {
            try
            {
                switch (info.connectionInfo.state)
                {
                    case ConnectionState.None:
                        break;

                    case ConnectionState.Connected:
                        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA Client connected to server - ID: " + clientConnection);
                        break;

                    case ConnectionState.ClosedByPeer:
                    case ConnectionState.ProblemDetectedLocally:
                        client.CloseConnection(clientConnection);
                        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA Client disconnected from server");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Log("ERRROR: " +  e);
                return;
            }
        };

        clientNetworkingUtils.SetStatusCallback(status);

        Address address = new Address();

        address.SetAddress("::1", 5000);

        clientConnection = client.Connect(ref address);

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

            #if VALVESOCKETS_SPAN
                    client.ReceiveMessagesOnConnection(clientConnection, message, 20);
            #else
                        int netMessagesCount = client.ReceiveMessagesOnConnection(clientConnection, netMessages, maxMessages);

            if (netMessagesCount > 0)
            {
                for (int i = 0; i < netMessagesCount; i++)
                {
                    ref NetworkingMessage netMessage = ref netMessages[i];

                    Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

                    netMessage.CopyTo(messageDataBuffer);
                    netMessage.Destroy();

                    string result = Encoding.ASCII.GetString(messageDataBuffer, 0, netMessage.length);
                    //string result = Encoding.ASCII.GetString(messageDataBuffer, 0, netMessage.length);

                    Debug.Log(result);
                }
            }
#endif
        }
    }
}
