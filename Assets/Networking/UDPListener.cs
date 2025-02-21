using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static class UDPListener {
    public static IPEndPoint ip;
    public static string data;

    private static  IPAddress groupAddress = IPAddress.Parse("233.255.255.255");
    private static UdpClient client;

    private const int PORT = 20000;

    public static void StartClient() {
        Debug.Log("UDP - Starting Client");
        ip = new IPEndPoint(IPAddress.Any, PORT);
        
        client = new UdpClient(ip);
        client.JoinMulticastGroup(groupAddress);
        client.BeginReceive(new AsyncCallback(RecieveServerInfo), null);
    }

    public static void RecieveServerInfo(IAsyncResult result) {
        byte[] recievedData = client.EndReceive(result, ref ip);
        data = Encoding.ASCII.GetString(recievedData);

        if (String.IsNullOrEmpty(data)) {
            Debug.Log("No Data Recieved");
        } else {
            Debug.Log(data);
        }

        client.BeginReceive(new AsyncCallback(RecieveServerInfo), null);
    }
}
