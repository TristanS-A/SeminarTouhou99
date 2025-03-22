using System;
using System.Collections.Generic;
using UnityEngine;

//this file is mid - prob over complicating it  :/
[Serializable]
public enum DropType
{
    POWER,
    SCORE,
    LIFE,
    DEF_BOMB,
    OFF_BOMB
}
//used for the list
[Serializable]
public struct DropCont
{
     public DropType dropType;
     public int ammount;
}

//to send in event
[System.Serializable]
public class DropTypes
{
     public List<DropCont> dropList = new List<DropCont>();
     private Vector3 locaiton = Vector3.zero;

    //this has no position by default MAKE SURE TO SET BEFORE SENDING!!! - will dusk
    public void SetLocation(Vector3 location)
    {
        this.locaiton = location;   
    }
    public Vector3 GetLocation()
    {
       return this.locaiton;
    }

}