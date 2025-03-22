using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropEvent :  eventType
{
    DropTypes _dropTypes;
   public DropEvent(DropTypes drop):base(EventTypes.ENEMY_KILLED)
   {
        _dropTypes = drop;
   }
   public DropTypes GetDropObject()
   {
       return _dropTypes;
   }
}
