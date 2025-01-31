using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class testForTimeLines : MonoBehaviour
{
    [SerializeField] TimelineAsset timeline;
    // Start is called before the first frame update
    void Start()
    {
        //testFun();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void testFun()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(0,-1.0f);
       
        

    }
}
