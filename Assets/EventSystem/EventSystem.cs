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
    public static event Action onPlayerDeath;
    public static event Action<clientHandler.PlayerSendResultData> onReceiveResult;

    public static void fireEvent(EventType type)
    {
        switch (type.getEventType())
        {
            case EventType.EventTypes.START_GAME:
                GameStartEvent player = (GameStartEvent)(type);
                gameStarted.Invoke(player.getPlayer());    //THIS BREAKS WHEN STARTING A SCENE AND TRIGERING THE EVENT ON START FOR SOME REASON <-- sub scripting timing(most likly calling a event befor it is subscriped)
                break;
            case EventType.EventTypes.RECEIVED_IP:
                ReceiveIPEvent ip = (ReceiveIPEvent)(type);
                ipReceived.Invoke(ip.getIP());   
                break;
            case EventType.EventTypes.PLAYER_DIED:
                onPlayerDeath.Invoke();
                break;
            case EventType.EventTypes.NUMBER_OF_PLAYERS_JOINED_CHANGED:
                PlayerCountChangedEvent newPlayerCountEvent = (PlayerCountChangedEvent)(type);
                numberOfJoinedPlayersChanged.Invoke(newPlayerCountEvent.getNewPlayerCount());
                break;
            case EventType.EventTypes.PLAYER_RESULT_RECEIVED:
                PlayerResultEvent playerSendResultData = (PlayerResultEvent)(type);
                playerResultReveived.Invoke(playerSendResultData.getTime(), playerSendResultData.getPoints());
                break;
            case EventType.EventTypes.RESULT_SENT:
                try //Try is in case the scene with the subscribed functions is not loaded yet
                {
                    ReceiveResultEvent result = (ReceiveResultEvent)(type);
                    onReceiveResult.Invoke(result.getResult());
                }
                catch (Exception e)
                {
                }
                break;
        }
    }
}
