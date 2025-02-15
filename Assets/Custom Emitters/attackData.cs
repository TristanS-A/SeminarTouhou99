using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "Touhou/AttackData")]
public class AttackData : ScriptableObject
{
    [SerializeField] bool isCenterd = true;

    [SerializeField] Vector2 dirction;

    [SerializeField] Pattern attackPattern;
    [SerializeField] BaseBullet bulletType;
    [SerializeField] GameObject emmiter;

    [SerializeField] float angle;
    
    //getters dont really want code to change this object but we need to read from it
    public bool IsCenterd()
    {
        return isCenterd; 
    }
    public Pattern GetPattern() { return attackPattern; }

    public BaseBullet GetBullet() { return bulletType; }

    public GameObject GetEmitter() { return emmiter; }
    public float GetAngle() { return angle; }

    public Vector2 GetDirction() { return dirction; }
}
