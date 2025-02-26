using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StarPattern : Pattern
{
    [SerializeField] float waitTime;
    [SerializeField] float ammount;
    [SerializeField] float radius;
    [SerializeField] float scaler;
    float k = 5;

    Vector2 center;
    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;

        StartCoroutine(SpawnStar());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SpawnStar()
    {
        WaitForSeconds wait = new WaitForSeconds(waitTime);
        //create objects
        for (int i = 0; i < ammount; i++)
        {
            //star equation https://en.wikipedia.org/wiki/Hypocycloid
            float R = k * radius;
            //get the x and y position based on the position of the center
            //float x = center.x + (R - radius) * Mathf.Cos(i) + radius * Mathf.Cos((k-1)*i);
            //float y = center.y + (R - radius) * Mathf.Sin(i) + radius * Mathf.Sin((k - 1) * i);
            float x = center.x + radius * (k-1) * Mathf.Cos(i) +radius * Mathf.Cos((k-1)*i);
            float y = center.y + radius * (k - 1) * Mathf.Sin(i) - radius * Mathf.Sin((k - 1) * i);

            Vector2 spawnPos = new Vector2(x, y);
            GameObject dummy = Instantiate(bullet, spawnPos, Quaternion.identity);
            Vector2 directionVector = center - spawnPos;


            var bul = dummy.GetComponent<BaseBullet>();

            //if we want to modify the scaler then do so
            if (scaler > 0)
            {
                bul.scaler = scaler;
            }

            bul.initProj(center, transform);

            bullets.Add(bul);

            yield return wait;

        }

        yield return null;
    
    }


}
