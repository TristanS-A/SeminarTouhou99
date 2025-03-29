using System;
using UnityEngine;
using UnityEngine.Events;

public static class EventSystem
{
    public static event Action<GameObject> gameStarted; //Rename this to game start event
    public static event Action<string, string> ipReceived;
    public static event Action<int> numberOfJoinedPlayersChanged;
    public static event Action<int, int> playerResultReveived;
    public static event Action<clientHandler.PlayerSendResultData> onReceiveResult;
    public static event Action onEndGameSession;
    public static event Action<DropTypes> dropEvent;

    // Player Events
    public static event UnityAction<int> OnHealthUpdate;
    public static void HealthUpdate(int health) => OnHealthUpdate?.Invoke(health);

    public static event UnityAction OnPlayerDeathUpdate;
    public static void OnPlayerDeath() => OnPlayerDeathUpdate?.Invoke();

    public static UnityEvent<float> OnRespawnUpdate;
    public static void RespawnUpdate(float health) => OnRespawnUpdate?.Invoke(health);

    // Bomb Events
    public static event UnityAction<int> OnDefensiveBombAttack;
    public static void DefensiveBombAttack(int amount) => OnDefensiveBombAttack?.Invoke(amount);

    public static event UnityAction<Vector2> OnOffensiveBombAttack;
    public static void OffensiveBombAttack(Vector2 pos) => OnOffensiveBombAttack?.Invoke(pos);

    public static event UnityAction<int> OnOffensiveBombUI;
    public static void OffensiveBombAttackUI(int amount) => OnOffensiveBombUI?.Invoke(amount);

    public static event UnityAction<Vector2> OnFireOffensiveBomb;
    public static void FireOffensiveBomb(Vector2 pos) => OnFireOffensiveBomb?.Invoke(pos);

    // Enemy Events
    public static event UnityAction<int> OnEnemyHealthUpdate;
    public static void EnemyHealthUpdate(int health) => OnEnemyHealthUpdate?.Invoke(health);

    public static event UnityAction OnEnemyDeathUpdate;
    public static void OnEnemyDeath() => OnEnemyDeathUpdate?.Invoke();

    public static UnityEvent<float> OnEnemyRespawnUpdate;
    public static void EnemyRespawnUpdate(float health) => OnEnemyRespawnUpdate?.Invoke(health);

    //score
    public static event UnityAction<DropType, int> OnPickUpUpdate;
    public static void OnPickUp(DropType type, int ammount) => OnPickUpUpdate?.Invoke(type, ammount);

    public static event UnityAction<serverHandler.ResultContext> OnSendPlayerResultData;
    public static void SendPlayerResultData(serverHandler.ResultContext resultContext) => OnSendPlayerResultData?.Invoke(resultContext);

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
                ipReceived.Invoke(ip.getIP(), ip.getConnectionName());   
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
            case EventType.EventTypes.ENEMY_KILLED:
                DropEvent drop = (DropEvent)(type);
                dropEvent.Invoke(drop.GetDropObject());
                break;
            default:
                break;
        }
    }
}
