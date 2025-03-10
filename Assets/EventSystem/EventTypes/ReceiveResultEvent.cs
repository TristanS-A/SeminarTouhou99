using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveResultEvent : EventType
{
    private clientHandler.PlayerSendResultData mResult;

    public ReceiveResultEvent(clientHandler.PlayerSendResultData result) : base(EventType.EventTypes.RESULT_SENT)
    {
        mResult = result;
    }

    public clientHandler.PlayerSendResultData getResult()
    {
        return mResult;
    }
}