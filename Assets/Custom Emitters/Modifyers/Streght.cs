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
    }
    public override void UpdatePorjectile()
    {
        this.transform.position += new Vector3(-direction.x * scaler, -direction.y * scaler, 0);
    }
        
}
