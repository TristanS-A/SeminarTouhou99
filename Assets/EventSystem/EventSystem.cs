using System;
using UnityEngine;
using UnityEngine.Events;

public static class EventSystem
{

    //Game Events
    public static event Action<GameObject> gameStarted;
    public static void StartGame(GameObject player) { gameStarted?.Invoke(player); }

    public static event Action onEndGameSession;
    public static void EndGameSession() { onEndGameSession?.Invoke(); }

    public static event Action<DropTypes> dropEvent;
    public static void TriggerDrop(DropTypes dropType) { dropEvent?.Invoke(dropType); }

    //Networking Events
    public static event Action<string, string> ipReceived;
    public static void ReceiveIP(string ip, string connectionName) { ipReceived?.Invoke(ip, connectionName); }

    // Player Events
    public static event UnityAction<int> OnHealthUpdate;
    public static void HealthUpdate(int health) => OnHealthUpdate?.Invoke(health);

    public static event UnityAction OnPlayerDeath;
    public static void PlayerDeath() => OnPlayerDeath?.Invoke();

    public static UnityEvent<float> OnRespawnUpdate;
    public static void RespawnUpdate(float health) => OnRespawnUpdate?.Invoke(health);

    public static event Action<int> numberOfJoinedPlayersChanged;
    public static void NumberOfJoinedPlayersChanged(int newNumberOfPlayers) { numberOfJoinedPlayersChanged?.Invoke(newNumberOfPlayers); }
    
    public static event Action<int, int> playerResultReveived;
    public static void RegisterPlayerResults(int time, int score) { playerResultReveived?.Invoke(time, score); }

    public static event Action<ClientHandler.PlayerSendResultData> onReceiveResult;
    public static void RegisterOtherPlayerResult(ClientHandler.PlayerSendResultData playerData) { onReceiveResult?.Invoke(playerData); }

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

    public static UnityAction<bool> WaveStateChange;
    public static void OnWaveStateChange(bool state) => WaveStateChange?.Invoke(state);

    public static UnityAction OnSendGameStart;
    public static void SendGameStartState() => OnSendGameStart?.Invoke();

    //score
    public static event UnityAction<DropType, float> OnPickUpUpdate;
    public static void OnPickUp(DropType type, float ammount) => OnPickUpUpdate?.Invoke(type, ammount);

    public static event UnityAction<ServerHandler.ResultContext> OnSendPlayerResultData;
    public static void SendPlayerResultData(ServerHandler.ResultContext resultContext) => OnSendPlayerResultData?.Invoke(resultContext);

    public static event UnityAction<bool, Vector3> OnPlayerDie;
    public static void SendPlayerDeathData(bool isOwningPlayer, Vector3 deathPos) => OnPlayerDie?.Invoke(isOwningPlayer, deathPos);

    public static event UnityAction<bool, Vector3> OnPlayerWin;
    public static void SendPlayerWinData(bool isOwningPlayer, Vector3 winPos) => OnPlayerWin?.Invoke(isOwningPlayer, winPos);

    public static event UnityAction<int, int, int> OnTransitionBG;
    public static void TransitionBGEvent(int bgIndex, int cloudLevIndex, int cloudShadowIndex) => OnTransitionBG?.Invoke(bgIndex, cloudLevIndex, cloudShadowIndex);
}
