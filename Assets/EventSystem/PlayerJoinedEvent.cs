using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJoinedEvent : eventType
{
    private GameObject mPlayer;

    public PlayerJoinedEvent(GameObject player) : base(eventType.EventTypes.PLAYER_JOINED)
    { 
        mPlayer = player;
    }

    public GameObject getPlayer()
    {
        return mPlayer;
    }
}