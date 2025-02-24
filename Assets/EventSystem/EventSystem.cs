using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class eventSystem
{
    public static event Action<GameObject> playerJoined;
    public static event Action<string> ipReceived;
    public static event Action<int> numberOfJoinedPlayersChanged;

    public static void fireEvent(eventType type)
    {
        switch (type.getEventType())
        {
            case eventType.EventTypes.PLAYER_JOINED:
                PlayerJoinedEvent player = (PlayerJoinedEvent)(type);
                playerJoined.Invoke(player.getPlayer());    //THIS BREAKS WHEN STARTING A SCENE AND TRIGERING THE EVENT ON START FOR SOME REASON
                break;
            case eventType.EventTypes.RECEIVED_IP:
                ReceiveIPEvent ip = (ReceiveIPEvent)(type);
                ipReceived.Invoke(ip.getIP());   
                break;
            case eventType.EventTypes.NUMBER_OF_PLAYERS_JOINED_CHANGED:
                PlayersJoinedChangedEvent newPlayerCountEvent = (PlayersJoinedChangedEvent)(type);
                numberOfJoinedPlayersChanged.Invoke(newPlayerCountEvent.getNewPlayerCount());
                break;
        }
    }
}
