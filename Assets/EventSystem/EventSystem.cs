using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class eventSystem
{
    public static event Action<GameObject> gameStarted; //Rename this to game start event
    public static event Action<string> ipReceived;
    public static event Action<int> numberOfJoinedPlayersChanged;

    public static void fireEvent(eventType type)
    {
        switch (type.getEventType())
        {
            case eventType.EventTypes.GAME_STARTED:
                GameStartEvent player = (GameStartEvent)(type);
                gameStarted.Invoke(player.getPlayer());    //THIS BREAKS WHEN STARTING A SCENE AND TRIGERING THE EVENT ON START FOR SOME REASON <-- sub scripting timing(most likly calling a event befor it is subscriped)
                break;
            case eventType.EventTypes.RECEIVED_IP:
                ReceiveIPEvent ip = (ReceiveIPEvent)(type);
                ipReceived.Invoke(ip.getIP());   
                break;
            case eventType.EventTypes.NUMBER_OF_PLAYERS_JOINED_CHANGED:
                PlayerCountChangedEvent newPlayerCountEvent = (PlayerCountChangedEvent)(type);
                numberOfJoinedPlayersChanged.Invoke(newPlayerCountEvent.getNewPlayerCount());
                break;
        }
    }
}
