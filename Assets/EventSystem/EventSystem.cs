using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class eventSystem
{
    public static event Action<GameObject> playerJoined;

    public static void fireEvent(eventType type)
    {
        switch (type.getEventType())
        {
            case eventType.EventTypes.PLAYER_JOINED:
                PlayerJoinedEvent player = (PlayerJoinedEvent)(type);
                playerJoined.Invoke(player.getPlayer());    //THIS BREAKS WHEN STARTING A SCENE AND TRIGERING THE EVENT ON START FOR SOME REASON
                break;
        }
    }
}
