using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveIPEvent : EventType
{
    private string mIP;

    public ReceiveIPEvent(string ip) : base(EventType.EventTypes.RECEIVED_IP)
    { 
        mIP = ip;
    }

    public string getIP()
    {
        return mIP;
    }
}