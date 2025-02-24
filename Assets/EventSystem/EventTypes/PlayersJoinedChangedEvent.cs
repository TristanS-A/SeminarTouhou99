using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersJoinedChangedEvent : eventType
{
    private int mNewNumberOfPlayers;

    public PlayersJoinedChangedEvent(int newNumberOfPlayers) : base(eventType.EventTypes.NUMBER_OF_PLAYERS_JOINED_CHANGED)
    {
        mNewNumberOfPlayers = newNumberOfPlayers;
    }

    public int getNewPlayerCount()
    {
        return mNewNumberOfPlayers;
    }
}