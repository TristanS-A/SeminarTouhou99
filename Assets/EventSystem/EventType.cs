using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eventType
{
    public enum EventTypes
    {
        PLAYER_JOINED,
        RECEIVED_IP,
        NUMBER_OF_PLAYERS_JOINED_CHANGED,
        START_GAME,
        GAME_STARTED,
        PLAYER_RESULT_RECEIVED
    }

    private EventTypes _type;

    public eventType(EventTypes type)
    {
        _type = type;
    }

    public EventTypes getEventType()
    {
        return _type;
    }

    public static explicit operator eventType(EventTypes v)
    {
        throw new NotImplementedException();
    }
}
