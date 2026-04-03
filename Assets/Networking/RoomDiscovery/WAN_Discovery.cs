using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class WAN_Discovery
{
    public static string GenerateRoomID(string ipForRoom)
    {
        ushort currOctet;
        ushort encodedOctet = 0;

        int ipIndex = 0;
        int ipSegmentLength = ipForRoom.IndexOf('.');

        int ipLength = ipForRoom.Length;

        while (ushort.TryParse(ipForRoom.Substring(ipIndex, ipSegmentLength), out currOctet))
        {
            encodedOctet ^= currOctet;

            ipIndex = ipIndex + ipSegmentLength + 1;

            if (ipIndex >= ipLength)
            {
                break;
            }

            ipSegmentLength = ipForRoom.Substring(ipIndex).IndexOf('.');

            if (ipSegmentLength == -1)
            {
                ipSegmentLength = ipLength - ipIndex;
            }
        }

        //ushort id = (short)(octet1 ^ octet2 ^ octet3 ^ octet4);

        string encodedString = encodedOctet.ToString();

        int strSize = encodedString.Length;

        for (int i = strSize; i < 6; i++)
        {
            encodedString += "0";
        }

        return encodedOctet.ToString();
    }

    public static async Task<bool> AddRoomToActiveRooms(string roomID, string roomName, string ip)
    {
        UnityWebRequest request = UnityWebRequest.Post("https://touhou99stun.vercel.app/addRoom", 
            "{\"roomName\":\"" + roomName + "\", \"roomID\":\"Hi\", \"roomIP\":\"" + ip + "\"}", "application/json");

        //request.SetRequestHeader("Authorization", "Bearer: " + key);

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Succesfully added room to stun server");
            return true;
        }

        Debug.Log("Failed to add room to stun server");
        return false;
    }

    public static async Task<bool> RemoveRoomToActiveRooms(string roomID)
    {
        if (roomID == "") { return false; }

        UnityWebRequest request = UnityWebRequest.Post("https://touhou99stun.vercel.app/removeRoom", "{\"roomID\":\"" + roomID + "\"}", "application/json");
        //request.SetRequestHeader("Authorization", "Bearer: " + key);

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Succesfully removed room from stun server");
            return true;
        }

        Debug.Log("Failed to remove room from stun server");
        return false;
    }
}
