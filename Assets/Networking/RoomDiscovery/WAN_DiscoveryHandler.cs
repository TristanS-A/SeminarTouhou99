using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WAN_DiscoveryHandler : MonoBehaviour
{
    [Serializable]
    public struct RoomData
    {
        public string roomName;
        public string roomIP;
    }

    [Serializable]
    public struct RoomListDataHolder
    {
        public RoomData[] roomDataList;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RetreiveAllActiveRooms();
    }

    private async Task<RoomListDataHolder> RetreiveAllActiveRooms()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://touhou99stun.vercel.app/retreiveAllRooms");

        await request.SendWebRequest();

        RoomListDataHolder roomListData;

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                roomListData = GetStringToRoomDataList(request.downloadHandler.text);
                foreach (RoomData roomData in roomListData.roomDataList)
                {
                    Debug.Log("Room Data: Name: " + roomData.roomName + " IP: " + roomData.roomIP);
                    EventSystem.ReceiveIP(roomData.roomIP, roomData.roomName);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return new RoomListDataHolder();
            }

            return roomListData;
        }

        return new RoomListDataHolder();
    }

    private RoomListDataHolder GetStringToRoomDataList(string jsonString)
    {
        string wrappedJSON = "{\"roomDataList\":" + jsonString + "}";
        return JsonUtility.FromJson<RoomListDataHolder>(wrappedJSON);
    }
}
