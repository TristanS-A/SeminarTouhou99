using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Streght : BaseBullet
{
    // Start is called before the first frame update
    void Start()
    { 
        rb.isKinematic = true;
        maxLifetime = lifeTime;
        
    }
    public override void UpdatePorjectile()
    {
       direction = direction.normalized;
        this.transform.position += new Vector3(-direction.x * scaler, -direction.y * scaler, 0);
    }
        
}
