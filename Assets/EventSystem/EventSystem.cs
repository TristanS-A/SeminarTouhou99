using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class EventSystem
{
    public static event Action<GameObject> gameStarted; //Rename this to game start event
    public static event Action<string> ipReceived;
    public static event Action<int> numberOfJoinedPlayersChanged;
    public static event Action<int, int> playerResultReveived;

    public static void fireEvent(EventType type)
    {
        switch (type.getEventType())
        {
            case EventType.EventTypes.GAME_STARTED:
                GameStartEvent player = (GameStartEvent)(type);
                gameStarted.Invoke(player.getPlayer());    //THIS BREAKS WHEN STARTING A SCENE AND TRIGERING THE EVENT ON START FOR SOME REASON <-- sub scripting timing(most likly calling a event befor it is subscriped)
                break;
            case EventType.EventTypes.RECEIVED_IP:
                ReceiveIPEvent ip = (ReceiveIPEvent)(type);
                ipReceived.Invoke(ip.getIP());   
                break;
            case EventType.EventTypes.NUMBER_OF_PLAYERS_JOINED_CHANGED:
                PlayerCountChangedEvent newPlayerCountEvent = (PlayerCountChangedEvent)(type);
                numberOfJoinedPlayersChanged.Invoke(newPlayerCountEvent.getNewPlayerCount());
                break;
            case EventType.EventTypes.PLAYER_RESULT_RECEIVED:
                PlayerResultEvent playerResultData = (PlayerResultEvent)(type);
                playerResultReveived.Invoke(playerResultData.getTime(), playerResultData.getPoints());
                break;
        }
    }
}
