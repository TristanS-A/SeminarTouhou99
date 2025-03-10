using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartEvent : EventType
{
    private GameObject mPlayer;

    public GameStartEvent(GameObject player) : base(EventType.EventTypes.START_GAME)
    { 
        mPlayer = player;
    }

    public GameObject getPlayer()
    {
        return mPlayer;
    }
}