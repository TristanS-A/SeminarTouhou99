using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SingleShotPattern : MonoBehaviour
{

    [SerializeField] Vector2 diretion;
    [SerializeField] GameObject bullet;
    List<BaseBullet> bullets = new List<BaseBullet>();
    [SerializeField] int ammountToSpawn = 1;
    //in seconds
    [SerializeField] float timeInbetweenShot = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        GetDirection();
        StartCoroutine(spawnBulellets());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        foreach (BaseBullet bullet in bullets)
        {
            bullet.UpdatePorjectile();
        }
    }
    Vector3 GetDirection()
    {
        Vector3 center = transform.position;

        center += (Vector3)diretion;

        return center;
    }
    IEnumerator spawnBulellets()
    {
        WaitForSeconds wait = new WaitForSeconds(timeInbetweenShot);

        for (int i = 0; i < ammountToSpawn; i++)
        {
            //float offsetX = this.transform.position.x - diretion.x;
            //float offsetY = this.transform.position.y - diretion.y;

            //var obj = Instantiate(bullet, new Vector3(offsetX, offsetY, 0), Quaternion.identity);

            
            //BaseBullet baseBullet = obj.AddComponent<Wave>();
            //baseBullet.rb = obj.AddComponent<Rigidbody2D>();
            //baseBullet.rb.gravityScale = 0.0f;

            //baseBullet.initProj(this.transform.position);

            //baseBullet.scaler = 0.05f;
            //bullets.Add(baseBullet);

            var obj = Instantiate(bullet, diretion, Quaternion.identity);

            BaseBullet bul = obj.AddComponent<Wave>();
            bul.rb = bul.AddComponent<Rigidbody2D>();
            bul.rb.gravityScale = 0.0f;
            bul.initProj(this.transform.position);
         
            bul.scaler = 0.05f;
            bullets.Add(bul);


            yield return wait;
        }

        yield return null;
    }
}
