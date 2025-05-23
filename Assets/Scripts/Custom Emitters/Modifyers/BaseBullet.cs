using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseBullet : MonoBehaviour
{
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public float scaler = 1.0f;
    [SerializeField] public Vector2 velocity;
    [SerializeField] protected float lifeTime;
    protected float maxLifetime = 20f;
    protected Transform parentTrans;
    public Vector2 direction;

    private void Start() 
    {
        rb = this.GetComponent<Rigidbody2D>();
        GetComponent<SpriteRenderer>().sortingOrder = -1;
        rb.isKinematic = true;
        maxLifetime = lifeTime;

    }
    private void Update()
    {
        //tick down life time
        lifeTime -= Time.deltaTime;
    }
    //gets the inial direction and sets the correct orrienction
    public void initProj(Vector2 origin, Transform parnet)
    {
        Debug.Log("Inited a bullet");
        //gets the inital direction 
        rb.velocity = Vector2.zero;
        direction = origin - (Vector2)this.transform.position;
        direction.Normalize();

        Debug.Log("Origin " + origin);

        parentTrans = parnet;

        lifeTime = maxLifetime;

        //does this need to be done here?
        //transform.up = -direction;
        //Quaternion rotation = Quaternion.LookRotation(Vector3.forward, -direction);

        //this.transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, 360);
    }
    public float getLifeTime()
    {
        return lifeTime;
    }
    public virtual void UpdatePorjectile() { }
}
