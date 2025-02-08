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

        //transform.forward = -direction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void UpdatePorjectile()
    {
        //float wave = Mathf.Sin((Time.time) * frequency) * amplitude;

        //float x =  -direction.x * scaler;
        //float y =  -direction.y * scaler;

        //this.transform.position += new Vector3(x, y, 0) * wave;

        //this.transform.rotation = Quaternion.Euler(0, 0, 90);

        float wave = Mathf.Sin((Time.time) * frequency) * amplitude;

        rb.velocity = new Vector2((-direction.x + transform.position.x) * scaler, wave);
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
