using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleScript : MonoBehaviour
{
    [SerializeField] GameObject bullet;

    int radius = 3;

    Vector2 center = Vector2.zero;
    float scaler = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;

        StartCoroutine(SpawnCircle());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator SpawnCircle()
    {
        WaitForSeconds wait = new WaitForSeconds(0.25f);
        //create objects
        for (int i = 0; i < 40; i++)
        {
            float x = center.x + radius * Mathf.Cos(i);
            float y = center.y + radius * Mathf.Sin(i);

            Vector2 spawnPos = new Vector2(x, y);
            GameObject dummy = Instantiate(bullet, spawnPos, Quaternion.identity);
            Vector2 directionVector = center - spawnPos;

            Rigidbody2D rb = dummy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.AddForce(-directionVector * scaler);

            yield return wait;
        }

        yield return null;  
    }
}
