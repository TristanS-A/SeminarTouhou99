using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Valve.Sockets;
using static ClientHandler;
using static ServerHandler;

public static class UDPListener
{
    public static IPEndPoint ip;
    public static string data;

    //private static IPAddress groupAddress = IPAddress.Parse("233.255.255.255");
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
            //client.JoinMulticastGroup(groupAddress);

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
            client = null;
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
            client.BeginSend(bytes, bytes.Length, "255.255.255.255", PORT, new AsyncCallback(RecieveServerInfo), null);
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
                    bool onSameLocalNetwork = true;
                    foreach (NetworkInterface nt in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (nt.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nt.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                        {

                            foreach (UnicastIPAddressInformation ip in nt.GetIPProperties().UnicastAddresses)
                            {
                                int newIPIndexPrev = 0;
                                int ipIndexPrev = 0;
                                int maskIndexPrev = 0;

                                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    string mask = ip.IPv4Mask.ToString();

                                    int newIPSegmentLength = ip.Address.ToString().IndexOf('.');
                                    int ipSegmentLength = newIPData.ip.IndexOf('.');
                                    int maskSegmentLength = mask.IndexOf('.');

                                    while(maskSegmentLength != 0 && int.Parse(mask.Substring(maskIndexPrev, maskSegmentLength)) != 0)
                                    {
                                        if (int.Parse(newIPData.ip.Substring(newIPIndexPrev, newIPSegmentLength)) != 
                                            int.Parse(ip.Address.ToString().Substring(ipIndexPrev, ipSegmentLength)))
                                        {
                                            onSameLocalNetwork = false;
                                            break;
                                        }

                                        newIPIndexPrev = newIPIndexPrev + newIPSegmentLength + 1;
                                        ipIndexPrev = ipIndexPrev + ipSegmentLength + 1;
                                        maskIndexPrev = maskIndexPrev + maskSegmentLength + 1;

                                        newIPSegmentLength = ip.Address.ToString().Substring(newIPIndexPrev).IndexOf('.');
                                        ipSegmentLength = newIPData.ip.Substring(ipIndexPrev).IndexOf('.');
                                        maskSegmentLength = mask.Substring(maskIndexPrev).IndexOf('.');

                                        if (maskSegmentLength == -1) { maskSegmentLength = mask.Length - maskIndexPrev; }
                                    }
                                }
                            }
                        }

                        if (onSameLocalNetwork)
                        {
                            EventSystem.ReceiveIP(newIPData.ip, newIPData.name);
                        }
                    }
                }

                client.BeginReceive(new AsyncCallback(RecieveServerInfo), null);
            }
        }
    }
}
