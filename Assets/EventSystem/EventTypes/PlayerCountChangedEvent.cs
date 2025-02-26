using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCountChangedEvent : eventType
{
    private int mNewNumberOfPlayers;

    public PlayerCountChangedEvent(int newNumberOfPlayers) : base(eventType.EventTypes.NUMBER_OF_PLAYERS_JOINED_CHANGED)
    {
        mNewNumberOfPlayers = newNumberOfPlayers;
    }

    public int getNewPlayerCount()
    {
        return mNewNumberOfPlayers;
    }
}