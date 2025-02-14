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

    [SerializeField] float angle;
    
}
