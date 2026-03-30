using UnityEngine;

public class LAN_DiscoveryHandler : MonoBehaviour
{
    void Start()
    {
        LAN_DiscoveryClient.StartClient(true);
    }

    private void OnApplicationQuit()
    {
        LAN_DiscoveryClient.CloseClient();
    }
}
