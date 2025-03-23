using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using static clientHandler;
using static serverHandler;

public static class UDPListener
{
    public static IPEndPoint ip;
    public static string data;

    private static IPAddress groupAddress = IPAddress.Parse("233.255.255.255");
    private static UdpClient client;

    private const int PORT = 20000;

    private static bool isReciving = false;

    private static byte[] messageDataBuffer = new byte[100];

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct IPData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string ip;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string name;
    };

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
            client.Dispose();
        }
    }

    public static void SendIP(string ip, string serverName)
    {
        //byte[] bytes = Encoding.ASCII.GetBytes(ip);
        IPData newIPData = new IPData { ip = ip , name = serverName };

        IntPtr ptr = IntPtr.Zero;
        byte[] bytes = new byte[Marshal.SizeOf(typeof(IPData))];

        try
        {
            ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IPData)));
            Marshal.StructureToPtr(newIPData, ptr, true);
            Marshal.Copy(ptr, bytes, 0, Marshal.SizeOf(typeof(IPData)));
        }
        finally
        {
            client.BeginSend(bytes, bytes.Length, "233.255.255.255", PORT, new AsyncCallback(RecieveServerInfo), null);
            Marshal.FreeHGlobal(ptr);
        }
    }

    public static void RecieveServerInfo(IAsyncResult result)
    {
        if (result != null)
        {
            byte[] receivedIPData = client.EndReceive(result, ref ip);
            IPData newIPData = new IPData();
            int size = Marshal.SizeOf(newIPData);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(receivedIPData, 0, ptr, size);

                newIPData = (IPData)Marshal.PtrToStructure(ptr, newIPData.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);

                if (isReciving)
                {
                    EventSystem.fireEvent(new ReceiveIPEvent(newIPData.ip, newIPData.name));
                }

                client.BeginReceive(new AsyncCallback(RecieveServerInfo), null);
            }
        }
    }
}
