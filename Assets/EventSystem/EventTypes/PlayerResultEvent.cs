using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResultEvent : EventType
{
    private int mTime;
    private int mPoints;

    public PlayerResultEvent(int time, int points) : base(EventType.EventTypes.PLAYER_RESULT_RECEIVED)
    {
        mTime = time;
        mPoints = points;
    }

    public int getTime() { return mTime; }

    public int getPoints() {  return mPoints; }
}