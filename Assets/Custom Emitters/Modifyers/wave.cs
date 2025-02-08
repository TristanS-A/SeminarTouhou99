using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave : BaseBullet
{
    int sinDirection = 1;
    [SerializeField] public float amplitude = 10.0f;
    [SerializeField] public float frequency = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        rb.isKinematic = false;
        rb.gravityScale = 0.0f; 
    }

    public override void UpdatePorjectile()
    {
        float wave = Mathf.Sin((Time.time) * frequency) * amplitude;

        Debug.Log("transfrom " + transform.up);
        transform.position += transform.up;

    }
    //inversts the direction of the bullet
    public void InvertDirection()
    {
        if (sinDirection == 1)
        {
            sinDirection = -1;
        }
        else
        {
            sinDirection = 1;
        }
    }
}
