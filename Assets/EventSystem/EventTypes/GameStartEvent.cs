using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartEvent : eventType
{
    private GameObject mPlayer;

    public GameStartEvent(GameObject player) : base(eventType.EventTypes.GAME_STARTED)
    { 
        mPlayer = player;
    }

    public GameObject getPlayer()
    {
        return mPlayer;
    }
}