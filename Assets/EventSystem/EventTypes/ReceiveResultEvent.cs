using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveResultEvent : EventType
{
    private ClientHandler.PlayerSendResultData mResult;

    public ReceiveResultEvent(ClientHandler.PlayerSendResultData result) : base(EventType.EventTypes.RESULT_SENT)
    {
        mResult = result;
    }

    public ClientHandler.PlayerSendResultData getResult()
    {
        return mResult;
    }
}