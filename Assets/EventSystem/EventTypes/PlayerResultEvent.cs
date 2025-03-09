using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResultEvent : eventType
{
    private int mTime;
    private int mPoints;

    public PlayerResultEvent(int time, int points) : base(eventType.EventTypes.PLAYER_RESULT_RECEIVED)
    {
        mTime = time;
        mPoints = points;
    }

    public int getTime() { return mTime; }

    public int getPoints() {  return mPoints; }
}