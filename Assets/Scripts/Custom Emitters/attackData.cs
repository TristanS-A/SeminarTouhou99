using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "Touhou/AttackData")]
public class AttackData : ScriptableObject
{
    [SerializeField] bool isCenterd = true;

    [SerializeField] bool isCustomTime = false;
    [Tooltip("This will only work if isCustomTime is Checked")]
    [SerializeField] float customLifeTime;

    [SerializeField] Vector2 dirction;

    Pattern attackPattern;
    [SerializeField] GameObject bulletType;
    [SerializeField] GameObject emmiter;

    [SerializeField] float angle;

    [SerializeField] bool shouldAttach = false;


    //getters dont really want code to change this object but we need to read from it

    private void Awake()
    {
        emmiter.GetComponent<Pattern>().bullet = bulletType;
    }
    public bool IsCenterd()
    {
        return isCenterd;
    }
    public Pattern GetPattern() { return attackPattern; }

    public GameObject GetBullet() { return bulletType; }

    public GameObject GetEmitter() { return emmiter; }
    public float GetAngle() { return angle; }

    public Vector2 GetDirction() { return dirction; }
    public bool IsCustomTime() { return isCustomTime; }
    public float GetCustomLifeTime() { return customLifeTime; }
    public bool ShouldAttach() { return shouldAttach;  }
}
