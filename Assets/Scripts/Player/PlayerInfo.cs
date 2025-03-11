using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerInfo
{
    private static string mPlayerName;
    private static float mPlayerTime;
    private static int mPlayerPoints;

    public static string PlayerName
    {
        get { return mPlayerName; }
        set { mPlayerName = value; }
    }

    public static float PlayerTime
    {
        get { return mPlayerTime; }
        set { mPlayerTime = value; }
    }

    public static int PlayerPoints
    {
        get { return mPlayerPoints; }
        set { mPlayerPoints = value; }
    }
}
