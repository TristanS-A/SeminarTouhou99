using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringEvent : EventType
{
    private string mString;

    public StringEvent(string str, EventType.EventTypes eventType) : base(eventType)
    {
        mString = str;
    }

    public string getString()
    {
        return mString;
    }
}