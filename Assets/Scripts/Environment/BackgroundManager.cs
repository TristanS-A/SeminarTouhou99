using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private float mBGScrollSpeed;
    [SerializeField] private float mCloudScrollSpeed;
    [SerializeField] private float mCloudShadowsScrollSpeed;
    [SerializeField] private GameObject m_BGPanelPrefab;
    [SerializeField] private GameObject m_CloudPanelPrefab;
    [SerializeField] private Sprite mBackground1;
    [SerializeField] private Sprite mClouds;
    [SerializeField] private Sprite mCloudShadows;
    private GameObject mBGPanel1;
    private GameObject mBGPanel2;

    private GameObject mCloudPanel1;
    private GameObject mCloudPanel2;

    private GameObject mCloudShadowsPanel1;
    private GameObject mCloudShadowsPanel2;

    // Start is called before the first frame update
    void Start()
    {
        ////Trying to automatically rotate the background to the camera (not work right)
        //Camera camera = FindAnyObjectByType<Camera>();
        //Vector3 originalRot = transform.eulerAngles;
        //transform.LookAt(camera.transform);
        //transform.eulerAngles = new Vector3(-originalRot.x, transform.eulerAngles.y, transform.eulerAngles.z);

        mBGPanel1 = Instantiate(m_BGPanelPrefab, transform);
        mBGPanel2 = Instantiate(m_BGPanelPrefab, transform);

        mBGPanel1.GetComponent<SpriteRenderer>().sprite = mBackground1;
        mBGPanel2.GetComponent<SpriteRenderer>().sprite = mBackground1;

        mCloudPanel1 = Instantiate(m_CloudPanelPrefab, transform);
        mCloudPanel2 = Instantiate(m_CloudPanelPrefab, transform);

        SpriteRenderer cloudSR1 = mCloudPanel1.GetComponent<SpriteRenderer>();
        SpriteRenderer cloudSR2 = mCloudPanel2.GetComponent<SpriteRenderer>();

        cloudSR1.sprite = mClouds;
        cloudSR2.sprite = mClouds;

        mCloudShadowsPanel1 = Instantiate(m_CloudPanelPrefab, transform);
        mCloudShadowsPanel2 = Instantiate(m_CloudPanelPrefab, transform);

        SpriteRenderer cloudShadowsSR1 = mCloudShadowsPanel1.GetComponent<SpriteRenderer>();
        SpriteRenderer cloudShadowsSR2 = mCloudShadowsPanel2.GetComponent<SpriteRenderer>();

        cloudShadowsSR1.sprite = mCloudShadows;
        cloudShadowsSR2.sprite = mCloudShadows;
        cloudShadowsSR1.sortingOrder = cloudSR1.sortingOrder - 1;
        cloudShadowsSR2.sortingOrder = cloudSR2.sortingOrder - 1;
    }

    // Update is called once per frame
    void Update()
    {
        mBGPanel1.transform.localPosition = new Vector3(mBGPanel1.transform.localPosition.x, -Mathf.Repeat(Time.time * mBGScrollSpeed, mBGPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y), mBGPanel1.transform.localPosition.z);
        mBGPanel2.transform.localPosition = new Vector3(mBGPanel2.transform.localPosition.x, -Mathf.Repeat(Time.time * mBGScrollSpeed, mBGPanel2.GetComponent<SpriteRenderer>().sprite.bounds.size.y) + mBGPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y, mBGPanel2.transform.localPosition.z);

        mCloudPanel1.transform.localPosition = new Vector3(mCloudPanel1.transform.localPosition.x, -Mathf.Repeat(Time.time * mCloudScrollSpeed, mCloudPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y), mCloudPanel1.transform.localPosition.z);
        mCloudPanel2.transform.localPosition = new Vector3(mCloudPanel2.transform.localPosition.x, -Mathf.Repeat(Time.time * mCloudScrollSpeed, mCloudPanel2.GetComponent<SpriteRenderer>().sprite.bounds.size.y) + mCloudPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y, mCloudPanel2.transform.localPosition.z);

        mCloudShadowsPanel1.transform.localPosition = new Vector3(mCloudShadowsPanel1.transform.localPosition.x, -Mathf.Repeat(Time.time * mCloudShadowsScrollSpeed, mCloudShadowsPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y), mCloudShadowsPanel1.transform.localPosition.z);
        mCloudShadowsPanel2.transform.localPosition = new Vector3(mCloudShadowsPanel2.transform.localPosition.x, -Mathf.Repeat(Time.time * mCloudShadowsScrollSpeed, mCloudShadowsPanel2.GetComponent<SpriteRenderer>().sprite.bounds.size.y) + mCloudShadowsPanel2.GetComponent<SpriteRenderer>().sprite.bounds.size.y, mCloudShadowsPanel1.transform.localPosition.z);
    }
}


/*
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private float mBGScrollSpeed;
    [SerializeField] private float mCloudScrollSpeed;
    [SerializeField] private float mCloudShadowsScrollSpeed;
    [SerializeField] private GameObject m_BGPanelPrefab;
    [SerializeField] private GameObject m_CloudPanelPrefab;
    [SerializeField] private Sprite mBackground1;
    [SerializeField] private Sprite mClouds;
    [SerializeField] private Sprite mCloudShadows;
    private GameObject mBGPanel1;
    private GameObject mBGPanel2;

    private GameObject mCloudPanel1;
    private GameObject mCloudPanel2;

    private GameObject mCloudShadowsPanel1;
    private GameObject mCloudShadowsPanel2;

    // Start is called before the first frame update
    void Start()
    {
        ////Trying to automatically rotate the background to the camera (not work right)
        //Camera camera = FindAnyObjectByType<Camera>();
        //Vector3 originalRot = transform.eulerAngles;
        //transform.LookAt(camera.transform);
        //transform.eulerAngles = new Vector3(-originalRot.x, transform.eulerAngles.y, transform.eulerAngles.z);

        mBGPanel1 = Instantiate(m_BGPanelPrefab, transform);
        mBGPanel2 = Instantiate(m_BGPanelPrefab, transform);

        mBGPanel1.GetComponent<SpriteRenderer>().sprite = mBackground1;
        mBGPanel2.GetComponent<SpriteRenderer>().sprite = mBackground1;

        mCloudPanel1 = Instantiate(m_CloudPanelPrefab, transform);
        mCloudPanel2 = Instantiate(m_CloudPanelPrefab, transform);

        SpriteRenderer cloudSR1 = mCloudPanel1.GetComponent<SpriteRenderer>();
        SpriteRenderer cloudSR2 = mCloudPanel2.GetComponent<SpriteRenderer>();

        cloudSR1.sprite = mClouds;
        cloudSR2.sprite = mClouds;

        mCloudShadowsPanel1 = Instantiate(m_CloudPanelPrefab, transform);
        mCloudShadowsPanel2 = Instantiate(m_CloudPanelPrefab, transform);

        SpriteRenderer cloudShadowsSR1 = mCloudShadowsPanel1.GetComponent<SpriteRenderer>();
        SpriteRenderer cloudShadowsSR2 = mCloudShadowsPanel2.GetComponent<SpriteRenderer>();

        cloudShadowsSR1.sprite = mCloudShadows;
        cloudShadowsSR2.sprite = mCloudShadows;
        cloudShadowsSR1.sortingOrder = cloudSR1.sortingOrder - 1;
        cloudShadowsSR2.sortingOrder = cloudSR2.sortingOrder - 1;
    }

    // Update is called once per frame
    void Update()
    {
        mBGPanel1.transform.localPosition = new Vector3(mBGPanel1.transform.localPosition.x, -Mathf.Repeat(Time.time * mBGScrollSpeed, mBGPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y), mBGPanel1.transform.localPosition.z);
        mBGPanel2.transform.localPosition = new Vector3(mBGPanel2.transform.localPosition.x, -Mathf.Repeat(Time.time * mBGScrollSpeed, mBGPanel2.GetComponent<SpriteRenderer>().sprite.bounds.size.y) + mBGPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y, mBGPanel2.transform.localPosition.z);

        mCloudPanel1.transform.localPosition = new Vector3(mCloudPanel1.transform.localPosition.x, -Mathf.Repeat(Time.time * mCloudScrollSpeed, mCloudPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y * mCloudPanel1.transform.localScale.y), mCloudPanel1.transform.localPosition.z);
        mCloudPanel2.transform.localPosition = new Vector3(mCloudPanel2.transform.localPosition.x, -Mathf.Repeat(Time.time * mCloudScrollSpeed, mCloudPanel2.GetComponent<SpriteRenderer>().sprite.bounds.size.y * mCloudPanel2.transform.localScale.y) + mCloudPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y * mCloudPanel1.transform.localScale.y, mCloudPanel2.transform.localPosition.z);

        mCloudShadowsPanel1.transform.localPosition = new Vector3(mCloudShadowsPanel1.transform.localPosition.x, -Mathf.Repeat(Time.time * mCloudShadowsScrollSpeed, mCloudShadowsPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y), mCloudShadowsPanel1.transform.localPosition.z);
        mCloudShadowsPanel2.transform.localPosition = new Vector3(mCloudShadowsPanel2.transform.localPosition.x, -Mathf.Repeat(Time.time * mCloudShadowsScrollSpeed, mCloudShadowsPanel2.GetComponent<SpriteRenderer>().sprite.bounds.size.y) + mCloudShadowsPanel2.GetComponent<SpriteRenderer>().sprite.bounds.size.y, mCloudShadowsPanel1.transform.localPosition.z);
    }
}

 */