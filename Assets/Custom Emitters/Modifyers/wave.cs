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

        //local space transformation
        transform.Translate(new Vector3(1 * scaler, wave * scaler, 0), Space.Self);

        //get angle 
        if(!angleCalculated)
        transform.parent.rotation *= Quaternion.Euler(0, 0, getAngle());
      


    }
    //make sure this is calculated once
    float getAngle()
    {
        if (!angleCalculated)
        {
            angleFromSource = Vector2.SignedAngle(direction, parentTrans.position);
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
