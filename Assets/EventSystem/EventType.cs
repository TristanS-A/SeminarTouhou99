using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventType
{
    public enum EventTypes
    {
        PLAYER_JOINED,
        RECEIVED_IP,
        NUMBER_OF_PLAYERS_JOINED_CHANGED,
        START_GAME,
        PLAYER_RESULT_RECEIVED,
        PLAYER_DIED,
        GAME_FINISHED
    }

    private EventTypes _type;

    public EventType(EventTypes type)
    {
        _type = type;
    }

    public EventTypes getEventType()
    {
        return _type;
    }

    public static explicit operator EventType(EventTypes v)
    {
        throw new NotImplementedException();
    }
}
