using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotPattern : Pattern
{

    [SerializeField] Vector2 diretion;
    //[SerializeField] GameObject bullet;
    //List<BaseBullet> bullets = new List<BaseBullet>();
    [SerializeField] int ammountToSpawn = 1;
    //in seconds
    [SerializeField] float timeInbetweenShot = 1.0f;
    [SerializeField] float modifyScaler = 0;
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
        List<int> removalIndex = new List<int>();
        int index = 0;
        foreach (BaseBullet bullet in bullets)
        {
            bullet.UpdatePorjectile();

            //check bullet life
            if (bullet.getLifeTime() <= 0)
            {
                //add it to some remove list
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
    //gets the direction that we want to fire our bullets
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
            
            var obj = Instantiate(bullet, (Vector2)this.transform.position, Quaternion.identity);
            
            //get the compenet of type
            
            var comp = obj.GetComponent<BaseBullet>();

            if (comp == null)
            {
                //try the child
                comp = obj.GetComponentInChildren<BaseBullet>();
            }
            //init the direction of the poj
            comp.initProj(diretion + (Vector2)transform.position, transform);

            if(modifyScaler > 0)
            {
                comp.scaler = modifyScaler;
            }
            //add it to manage list
            bullets.Add(comp);


            yield return wait;
        }

        yield return null;
    }
    void CleanList(List<int> indexes, List<BaseBullet>listToRemoveFrom)
    {
        foreach (int index in indexes)
        {
            Destroy(listToRemoveFrom[index].gameObject);
            listToRemoveFrom.RemoveAt(index);
        }
    }
    //public void ClearBullets()
    //{
    //    bullets.Clear();
    //}
}
