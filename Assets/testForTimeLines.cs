using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class testForTimeLines : MonoBehaviour
{
    [SerializeField] PlayableDirector assest;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("test");
       
        //testFun();

    }

    // Update is called once per frame
    void Update()
    {
        assest.transform.position += new Vector3(-1, -1, 0);

    }
    public void testFun()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(0,-1.0f);
       
        

    }
}
