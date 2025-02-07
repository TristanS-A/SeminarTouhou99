using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public float scaler = 1.0f;
    [SerializeField] public Vector2 velocity;
    public Vector2 direction;

    private void Start() 
    {
        rb.isKinematic = true;
    }
    public void initProj(Vector2 origin)
    {
        //gets the inital direction 
        direction = origin - (Vector2)this.transform.position;
        direction.Normalize();
    }
    public virtual void UpdatePorjectile() { }
}
