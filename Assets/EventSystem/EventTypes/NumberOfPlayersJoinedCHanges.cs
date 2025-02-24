using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveIPEvent : eventType
{
    private string mIP;

    public ReceiveIPEvent(string ip) : base(eventType.EventTypes.RECEIVED_IP)
    { 
        mIP = ip;
    }

    public string getIP()
    {
        return mIP;
    }
}