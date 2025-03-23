using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveIPEvent : EventType
{
    private string mIP;
    private string mConnectionName;

    public ReceiveIPEvent(string ip, string connectionName) : base(EventType.EventTypes.RECEIVED_IP)
    { 
        mIP = ip;
        mConnectionName = connectionName;
    }

    public string getIP()
    {
        return mIP;
    }

    public string getConnectionName()
    {
        return mConnectionName;
    }
}