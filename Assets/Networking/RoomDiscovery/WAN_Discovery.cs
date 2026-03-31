using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class WAN_Discovery
{
    public static async Task<bool> AddRoomToActiveRooms()
    {
        UnityWebRequest request = UnityWebRequest.Post("https://touhou99stun.vercel.app/addRoom", "{\"roomName\":\"TheRoom\", \"roomID\":\"Hi\", \"roomIP\":\"NEW ONE\"}", "application/json");
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

    public static async Task<bool> RemoveRoomToActiveRooms()
    {
        UnityWebRequest request = UnityWebRequest.Post("https://touhou99stun.vercel.app/removeRoom", "{\"roomID\":\"Hi\"}", "application/json");
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
