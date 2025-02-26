using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirlcePattern : Pattern
{ 
    [SerializeField] float radius = 3;
    [SerializeField] float scaler = 0;
    [SerializeField] int ammout = 0;
    [SerializeField] float waitTime = 0;
    [SerializeField] bool invert;

    int inverted = 1;


    Vector2 center = Vector2.zero;
    

    //List<BaseBullet> bullets = new List<BaseBullet>();
    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;

        StartCoroutine(SpawnCircle());

        if(invert)
        {
            inverted = -1;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateProjectile();
    }
    private void FixedUpdate()
    {
        UpdateProjectile();
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

        if(bullets.Count <= 0)
        {
            //if we have no bullets destroy self
            Destroy(gameObject);
        }

    }
    //creation of the pattern
    IEnumerator SpawnCircle()
    {
        WaitForSeconds wait = new WaitForSeconds(waitTime);
        //create objects
        for (int i = 0; i < ammout; i++)
        {
            //get the x and y position based on the position of the center
            float x = center.x + radius * (inverted * Mathf.Cos(i));
            float y = center.y + radius * (inverted * Mathf.Sin(i));

            Vector2 spawnPos = new Vector2(x, y);
            GameObject dummy = Instantiate(bullet, spawnPos, Quaternion.identity);
            Vector2 directionVector = center - spawnPos;


            var bul = dummy.GetComponent<BaseBullet>();

            //if we want to modify the scaler then do so
            if(scaler > 0)
            {
                bul.scaler = scaler;
            }

            bul.initProj(center, transform);

            bullets.Add(bul);

            yield return wait;
        }

        yield return null;  
    }
    void CleanList(List<int> indexes, List<BaseBullet> listToRemoveFrom)
    {
        foreach (int index in indexes)
        {
            Destroy(listToRemoveFrom[index].gameObject);
            listToRemoveFrom.RemoveAt(index);
        }
    }
    //this can be used for bombs and other things consider making this a base class for emitters
    //public void ClearBullets()
    //{
    //    bullets.Clear();
    //}
}
