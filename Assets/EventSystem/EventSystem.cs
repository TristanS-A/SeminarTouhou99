using System;
using UnityEngine;
using UnityEngine.Events;

public static class EventSystem
{
    public static event Action<GameObject> gameStarted; //Rename this to game start event
    public static event Action<string> ipReceived;
    public static event Action<int> numberOfJoinedPlayersChanged;
    public static event Action<int, int> playerResultReveived;
    public static event Action onPlayerDeath;
    public static event Action<clientHandler.PlayerSendResultData> onReceiveResult;
    public static event Action onEndGameSession;
    public static event Action<DropTypes> dropEvent;

    // Player Events
    public static event UnityAction<int> OnHealthUpdate;
    public static void HealthUpdate(int health) => OnHealthUpdate?.Invoke(health);

    public static event UnityAction OnDeathUpdate;
    public static void OnDeath() => OnDeathUpdate?.Invoke();

    public static UnityEvent<float> OnRespawnUpdate;
    public static void RespawnUpdate(float health) => OnRespawnUpdate?.Invoke(health);

    // Bomb Events
    public static event UnityAction<int> OnDefensiveBombAttack;
    public static void DefensiveBombAttack(int amount) => OnDefensiveBombAttack?.Invoke(amount);

    public static event UnityAction<int> OnOffensiveBombAttack;
    public static void OffensiveBombAttack(int amount) => OnOffensiveBombAttack?.Invoke(amount);

    // Enemy Events
    public static event UnityAction<int> OnEnemyHealthUpdate;
    public static void EnemyHealthUpdate(int health) => OnEnemyHealthUpdate?.Invoke(health);

    public static event UnityAction OnEnemyDeathUpdate;
    public static void OnEnemyDeath() => OnEnemyDeathUpdate?.Invoke();

    public static UnityEvent<float> OnEnemyRespawnUpdate;
    public static void EnemyRespawnUpdate(float health) => OnEnemyRespawnUpdate?.Invoke(health);

    public static void fireEvent(eventType type)
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
                try
                {
                    ReceiveResultEvent result = (ReceiveResultEvent)(type);
                    onReceiveResult.Invoke(result.getResult());
                }
                catch{ }
                break;
            case EventType.EventTypes.END_GAME_SESSION:
                onEndGameSession.Invoke();
                break;
            case eventType.EventTypes.ENEMY_KILLED:
                DropEvent drop = (DropEvent)(type);
                dropEvent.Invoke(drop.GetDropObject());
                break;
            default:
                break;
        }
    }
}
