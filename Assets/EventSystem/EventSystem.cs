using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class eventSystem
{
    public static event Action<GameObject> playerJoined;
    public static event Action<string> ipReceived;

    public static void fireEvent(eventType type)
    {
        switch (type.getEventType())
        {
            case eventType.EventTypes.PLAYER_JOINED:
                PlayerJoinedEvent player = (PlayerJoinedEvent)(type);
                playerJoined.Invoke(player.getPlayer());    //THIS BREAKS WHEN STARTING A SCENE AND TRIGERING THE EVENT ON START FOR SOME REASON <-- sub scripting timing(most likly calling a event befor it is subscriped)
                break;
            case eventType.EventTypes.RECEIVED_IP:
                ReceiveIPEvent ip = (ReceiveIPEvent)(type);
                ipReceived.Invoke(ip.getIP());    //THIS BREAKS WHEN STARTING A SCENE AND TRIGERING THE EVENT ON START FOR SOME REASON
                break;
        }
    }
}
