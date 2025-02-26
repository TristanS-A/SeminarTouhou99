using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBoarder : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnTriggerExit2D(Collider2D collision)
    { 
        Debug.Log("hit smth");
        if (collision != null)
        {

            ////hit a bullet turn of sprite
            //if(collision.gameObject.GetComponent<BaseBullet>())
            //{
            //    SpriteRenderer sprt = collision.gameObject.GetComponent<SpriteRenderer>();

            //    sprt.enabled = !sprt.enabled;
            //}
        }
    }
}
