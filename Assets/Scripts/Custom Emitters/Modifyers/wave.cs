using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave : BaseBullet
{
    int sinDirection = 1;
    [SerializeField] public float amplitude = 6.0f;
    [SerializeField] public float frequency = 20.0f;

    [SerializeField] public float angleFromSource = 180.0f;

    bool angleCalculated = false;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }

    public override void UpdatePorjectile()
    {
        float wave = Mathf.Sin((Time.time) * frequency) * amplitude;
        float waveX = Mathf.Cos((Time.time) * frequency) * amplitude;

        //local space transformation (move up and down)
        transform.Translate(new Vector3(0, wave * scaler, 0), Space.Self);

        //move parent in a direction
        transform.parent.Translate(new Vector3(direction.x * scaler, direction.y * scaler, 0), Space.World);
        
        //get angle and rotate parent
        if (!angleCalculated)
        transform.parent.rotation *= Quaternion.Euler(0, 0, getAngle());
      


    }
    //make sure this is calculated once
    float getAngle()
    {
        //this get angle function is wrong :(((((
        if (!angleCalculated)
        {
            //for the angle I need to 
            //1. get the postion of the object
            //2. get the postion of my direction relitive to the spawned emitter
            //3. get the angle
            angleFromSource = Vector2.Angle((Vector2)this.transform.parent.right + (Vector2)this.transform.position, (Vector2)parentTrans.position + direction);

            angleCalculated = true;
        }
        //angleFromSource = Vector2.Angle(parentTrans.position, direction);
        
        return angleFromSource; 
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
