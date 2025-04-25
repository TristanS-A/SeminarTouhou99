using System.Buffers;
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
    [SerializeField] float k = 5;
    bool cIsRunning;
    Coroutine spawning;

    Vector2 center;
    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;

       spawning = StartCoroutine(SpawnStar());
    }

    // Update is called once per frame
    void Update()
    {
     
    }
    private void FixedUpdate()
    {
        if (!cIsRunning)
        {
            UpdateProjectile();
        }
    }
    void UpdateProjectile()
    {
        List<int> removalIndex = new List<int>();
        int index = 0;
        foreach (BaseBullet bullet in bullets)
        {
            bullet.UpdatePorjectile();

            //check bullet life
            if (bullet.getLifeTime() <= 0)
            {
                //add it to some remove list
                //Destroy(bullet.gameObject
                removalIndex.Add(index);
            }
            //keep track of index for removal
            index++;
        }

        if (removalIndex.Count > 0)
        {
            CleanList(removalIndex, bullets);
        }

        if (bullets.Count <= 0)
        {
            //if we have no bullets destroy self
            Destroy(gameObject);
        }

    }

    IEnumerator SpawnStar()
    {
        WaitForSeconds wait = new WaitForSeconds(waitTime);
        //create objects
        cIsRunning = true;
        for (int i = 0; i < ammount; i++)
        {
            //star equation https://en.wikipedia.org/wiki/Hypocycloid
            float R = k * radius;
            //get the x and y position based on the position of the center
            //float x = center.x + (R - radius) * Mathf.Cos(i) + radius * Mathf.Cos((k-1)*i);
            //float y = center.y + (R - radius) * Mathf.Sin(i) + radius * Mathf.Sin((k - 1) * i);
            float x = center.x + radius * (k - 1) * Mathf.Cos(i) +radius * Mathf.Cos((k-1)*i);
            float y = center.y + radius * (k - 1) * Mathf.Sin(i) - radius * Mathf.Sin((k - 1) * i);

            Vector2 spawnPos = new Vector2(x, y);
            GameObject dummy = ObjectPool.DequeueObject<BaseBullet>("BaseBullet").gameObject; //Instantiate(bullet, spawnPos, Quaternion.identity);
            Vector2 directionVector = center - spawnPos;

            dummy.transform.localPosition = spawnPos;
            dummy.transform.rotation = Quaternion.identity;

            dummy.SetActive(true);

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
        cIsRunning = false;
        yield return null;
    
    }
    void CleanList(List<int> indexes, List<BaseBullet> listToRemoveFrom)
    {
        foreach (int index in indexes)
        {
            
            listToRemoveFrom[index].gameObject.SetActive(false);
            ObjectPool.EnqeueObject<BaseBullet>(listToRemoveFrom[index], "BaseBullet");
            //listToRemoveFrom.RemoveAt(index);

        }
        listToRemoveFrom.RemoveAll(x => x.gameObject.activeSelf == false);
        indexes.Clear();
        //listToRemoveFrom.Clear();


        
    }

}
