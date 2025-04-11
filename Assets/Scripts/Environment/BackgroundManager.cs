using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private float mScrollSpeed;
    [SerializeField] private GameObject mPanel1;
    [SerializeField] private GameObject mPanel2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mPanel1.transform.position = new Vector3(transform.position.x, -Mathf.Repeat(Time.time * mScrollSpeed, mPanel1.GetComponent<SpriteRenderer>().bounds.size.y), 10);
        mPanel2.transform.position = new Vector3(transform.position.x, -Mathf.Repeat(Time.time * mScrollSpeed, mPanel2.GetComponent<SpriteRenderer>().bounds.size.y) + mPanel1.GetComponent<SpriteRenderer>().bounds.size.y, 10);
    }
}
