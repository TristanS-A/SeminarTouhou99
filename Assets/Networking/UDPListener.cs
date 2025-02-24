using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEditor.VersionControl;
using UnityEngine;

public static class UDPListener
{
    public static IPEndPoint ip;
    public static string data;

    private static IPAddress groupAddress = IPAddress.Parse("233.255.255.255");
    private static UdpClient client;

    private const int PORT = 20000;

    private static bool isReciving = false;

    public static void StartClient(bool receive)
    {
        Debug.Log("UDP - Starting Client");
        ip = new IPEndPoint(IPAddress.Any, PORT);

        if (client == null)
        {
            client = new UdpClient(ip);
            client.JoinMulticastGroup(groupAddress);

            isReciving = receive;

            client.BeginReceive(new AsyncCallback(RecieveServerInfo), null);
        }
    }

    public static void CloseClient()
    {
        if (client != null)
        {
            client.Close();
        }
    }

    public static void SendIP(string ip)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(ip);
        client.BeginSend(bytes, bytes.Length, "233.255.255.255", PORT, new AsyncCallback(RecieveServerInfo), null);
    }

    public static void RecieveServerInfo(IAsyncResult result)
    {
        byte[] recievedData = client.EndReceive(result, ref ip);
        data = Encoding.ASCII.GetString(recievedData);

        if (String.IsNullOrEmpty(data))
        {
            Debug.Log("No Data Recieved");
        }
        else
        {
            Debug.Log("data recived: " + data);
            if (isReciving)
            {
                eventSystem.fireEvent(new ReceiveIPEvent(data));
            }

        }

        client.BeginReceive(new AsyncCallback(RecieveServerInfo), null);
    }
}
