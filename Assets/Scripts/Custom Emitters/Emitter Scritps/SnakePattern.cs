using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SnakePattern : Pattern
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

       spawning = StartCoroutine(SpawnSnake());
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
                bullet.gameObject.SetActive(false);
                ObjectPool.EnqeueObject<BaseBullet>(bullet, "BaseBullet");
                //Destroy(bullet.gameObject);
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

    IEnumerator SpawnSnake()
    {
        WaitForSeconds wait = new WaitForSeconds(waitTime);
        //create objects
        for (int i = 0; i < ammount; i++)
        { 
            float x = center.x + Mathf.Sin(i * 0.6f) * 2;
            float y = center.y - i * 0.6f - 1;

            Vector2 spawnPos = new Vector2(x, y);
            GameObject dummy = ObjectPool.DequeueObject<BaseBullet>("BaseBullet").gameObject; //Instantiate(bullet, spawnPos, Quaternion.identity);

            Vector2 directionVector = Vector2.down;

            dummy.transform.position = spawnPos;
            dummy.transform.rotation = Quaternion.identity;

            dummy.SetActive(true);

            var bul = dummy.GetComponent<BaseBullet>();

            //if we want to modify the scaler then do so
            if (scaler > 0)
            {
                bul.scaler = scaler;
            }

            bul.initProj(center, transform);

            bul.direction = - new Vector2(x - center.x, y - center.y).normalized;

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
            //Destroy(listToRemoveFrom[index].gameObject);
           // listToRemoveFrom.RemoveAt(index);
        }
        listToRemoveFrom.RemoveAll(x => x.gameObject.activeSelf == false);
        indexes.Clear();
    }

}
