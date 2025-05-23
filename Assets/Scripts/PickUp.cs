﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    //make them move and destroy after a while
    [SerializeField] private DropType dropType;
    // Will asked me to do this, idk what for ¯\_(ツ)_/¯ <- thank you jerry :)
    [SerializeField] private float amount = 1;
    float lifeTime = 10.0f;
    private Rigidbody2D rb;
    // Start is called before the first frame update

    [SerializeField] private List<AudioClip> pickupSFX;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }
    private void FixedUpdate()
    {
        transform.position += Vector3.down * Time.deltaTime;
        if(lifeTime <= 0)
        {
            Destroy(gameObject);
        }
        lifeTime -= Time.deltaTime; 
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            EventSystem.OnPickUp(dropType, amount);
            SoundManager.Instance.PlayRandomSFXClip(pickupSFX, transform, 0.2f);
            Debug.Log("Picked Up " + dropType.ToString());
            Destroy(gameObject, 0.1f);
        }

    }
}
