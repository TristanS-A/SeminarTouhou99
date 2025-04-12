using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private float mScrollSpeed;
    [SerializeField] private GameObject m_PanelPrefab;
    [SerializeField] private Sprite mBackground1;
    [SerializeField] private Sprite mBackground2;
    private GameObject mPanel1;
    private GameObject mPanel2;

    // Start is called before the first frame update
    void Start()
    {
        ////Trying to automatically rotate the background to the camera (not work right)
        //Camera camera = FindAnyObjectByType<Camera>();
        //Vector3 originalRot = transform.eulerAngles;
        //transform.LookAt(camera.transform);
        //transform.eulerAngles = new Vector3(-originalRot.x, transform.eulerAngles.y, transform.eulerAngles.z);

        mPanel1 = Instantiate(m_PanelPrefab, transform);
        mPanel2 = Instantiate(m_PanelPrefab, transform);

        mPanel1.GetComponent<SpriteRenderer>().sprite = mBackground1;
        mPanel2.GetComponent<SpriteRenderer>().sprite = mBackground1;
    }

    // Update is called once per frame
    void Update()
    {
        mPanel1.transform.localPosition = new Vector3(mPanel1.transform.localPosition.x, -Mathf.Repeat(Time.time * mScrollSpeed, mPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y), mPanel1.transform.localPosition.z);
        mPanel2.transform.localPosition = new Vector3(mPanel2.transform.localPosition.x, -Mathf.Repeat(Time.time * mScrollSpeed, mPanel2.GetComponent<SpriteRenderer>().sprite.bounds.size.y) + mPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y, mPanel2.transform.localPosition.z);
    }
}
