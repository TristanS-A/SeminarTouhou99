using System;
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
    [SerializeField] private Sprite[] m_BackgroundSprites;
    [SerializeField] private Sprite[] m_CloudLevelSprites;
    [SerializeField] private Sprite[] m_CloudShadowSprites;

    [Tooltip("This builds a hashmap that uses the filenames of the 'from' bg and the 'to' bg to retreive the transition bg between the two")]
    [SerializeField] private TransitionImageData[] mTransitions;

    private GameObject mBGPanel1;
    private GameObject mBGPanel2;

    private GameObject mCloudPanel1;
    private GameObject mCloudPanel2;

    private GameObject mCloudShadowsPanel1;
    private GameObject mCloudShadowsPanel2;

    private float mPanelYSize = 0;

    private Dictionary<string, Sprite> mTransitionMap = new();

    [Serializable]
    public struct TransitionImageData
    {
        public Sprite transitionImage;
        public Sprite to;
        public Sprite from;
    }

    private void OnEnable()
    {
        EventSystem.OnTransitionBG += HandleTransition;
    }

    private void OnDisable()
    {
        EventSystem.OnTransitionBG -= HandleTransition;
    }

    // Start is called before the first frame update
    void Start()
    {
        ////Trying to automatically rotate the background to the camera (not work right)
        //Camera camera = FindAnyObjectByType<Camera>();
        //Vector3 originalRot = transform.eulerAngles;
        //transform.LookAt(camera.transform);
        //transform.eulerAngles = new Vector3(-originalRot.x, transform.eulerAngles.y, transform.eulerAngles.z);

        ParalaxSetUp();

        BuildTransitionConnections();

        //Starts level music
        SoundManager.Instance.PlayMusic(1);

        //StartCoroutine(Co_TriggerTransition());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateBackgroundLevel();

        UpdateCloudLevel();

        UpdateCloudShadowLevel();
    }

    private void UpdateBackgroundLevel()
    {
        mBGPanel1.transform.localPosition = new Vector3(mBGPanel1.transform.localPosition.x, -Mathf.Repeat(Time.fixedTime * mBGScrollSpeed, mPanelYSize), mBGPanel1.transform.localPosition.z);
        mBGPanel2.transform.localPosition = new Vector3(mBGPanel2.transform.localPosition.x, -Mathf.Repeat(Time.fixedTime * mBGScrollSpeed, mPanelYSize) + mPanelYSize, mBGPanel2.transform.localPosition.z);
    }

    private void UpdateCloudLevel()
    {
        mCloudPanel1.transform.localPosition = new Vector3(mCloudPanel1.transform.localPosition.x, -Mathf.Repeat(Time.fixedTime * mCloudScrollSpeed, mPanelYSize), mCloudPanel1.transform.localPosition.z);
        mCloudPanel2.transform.localPosition = new Vector3(mCloudPanel2.transform.localPosition.x, -Mathf.Repeat(Time.fixedTime * mCloudScrollSpeed, mPanelYSize) + mPanelYSize, mCloudPanel2.transform.localPosition.z);
    }

    private void UpdateCloudShadowLevel()
    {
        mCloudShadowsPanel1.transform.localPosition = new Vector3(mCloudShadowsPanel1.transform.localPosition.x, -Mathf.Repeat(Time.fixedTime * mCloudShadowsScrollSpeed, mPanelYSize), mCloudShadowsPanel1.transform.localPosition.z);
        mCloudShadowsPanel2.transform.localPosition = new Vector3(mCloudShadowsPanel2.transform.localPosition.x, -Mathf.Repeat(Time.fixedTime * mCloudShadowsScrollSpeed, mPanelYSize) + mPanelYSize, mCloudShadowsPanel1.transform.localPosition.z);
    }

    private void ParalaxSetUp()
    {
        mBGPanel1 = Instantiate(m_BGPanelPrefab, transform);
        mBGPanel2 = Instantiate(m_BGPanelPrefab, transform);

        mBGPanel1.GetComponent<SpriteRenderer>().sprite = m_BackgroundSprites[0];
        mBGPanel2.GetComponent<SpriteRenderer>().sprite = m_BackgroundSprites[0];

        mCloudPanel1 = Instantiate(m_CloudPanelPrefab, transform);
        mCloudPanel2 = Instantiate(m_CloudPanelPrefab, transform);

        SpriteRenderer cloudSR1 = mCloudPanel1.GetComponent<SpriteRenderer>();
        SpriteRenderer cloudSR2 = mCloudPanel2.GetComponent<SpriteRenderer>();

        cloudSR1.sprite = m_CloudLevelSprites[0];
        cloudSR2.sprite = m_CloudLevelSprites[0];

        mCloudShadowsPanel1 = Instantiate(m_CloudPanelPrefab, transform);
        mCloudShadowsPanel2 = Instantiate(m_CloudPanelPrefab, transform);

        SpriteRenderer cloudShadowsSR1 = mCloudShadowsPanel1.GetComponent<SpriteRenderer>();
        SpriteRenderer cloudShadowsSR2 = mCloudShadowsPanel2.GetComponent<SpriteRenderer>();

        cloudShadowsSR1.sprite = m_CloudShadowSprites[0];
        cloudShadowsSR2.sprite = m_CloudShadowSprites[0];
        cloudShadowsSR1.sortingOrder = cloudSR1.sortingOrder - 1;
        cloudShadowsSR2.sortingOrder = cloudSR2.sortingOrder - 1;

        mPanelYSize = mBGPanel1.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
    }

    private void BuildTransitionConnections()
    {
        foreach(TransitionImageData data in mTransitions)
        {
            mTransitionMap.Add(data.from.name + data.to.name, data.transitionImage);
        }
    }

    private bool BackgroundLooped()
    {
        return mBGPanel1.transform.localPosition.y >= -mBGScrollSpeed * Time.fixedDeltaTime;
    }

    private bool CloudLevelLooped()
    {
        return mCloudPanel1.transform.localPosition.y >= -mCloudScrollSpeed * Time.fixedDeltaTime;
    }

    private bool CloudShadowLevelLooped()
    {
        return mCloudShadowsPanel1.transform.localPosition.y >= -mCloudShadowsScrollSpeed * Time.fixedDeltaTime;
    }

    private IEnumerator Co_StartBackgroundTransition(Sprite transitionBackground, Sprite newBackground)
    {
        yield return new WaitUntil(BackgroundLooped);
        mBGPanel2.GetComponent<SpriteRenderer>().sprite = transitionBackground;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        StartCoroutine(Co_ContinueBackgroundTransition(transitionBackground, newBackground));
    }

    private IEnumerator Co_ContinueBackgroundTransition(Sprite transitionBackground, Sprite newBackground)
    {
        yield return new WaitUntil(BackgroundLooped);
        mBGPanel1.GetComponent<SpriteRenderer>().sprite = transitionBackground;
        mBGPanel2.GetComponent<SpriteRenderer>().sprite = newBackground;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        StartCoroutine(Co_FinishBackgroundTransition(newBackground));
    }

    private IEnumerator Co_FinishBackgroundTransition(Sprite newBackground)
    {
        yield return new WaitUntil(BackgroundLooped);
        mBGPanel1.GetComponent<SpriteRenderer>().sprite = newBackground;
    }

    private IEnumerator Co_StartCloudLevelTransition(Sprite newLevelSprite, int loops)
    {
        //This is to give the clouds additional loops before it transitions to not
        //have clouds fully disapear before cloud shadows
        for (int i = 0; i < loops; i++)
        {
            yield return new WaitUntil(CloudLevelLooped);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitUntil(CloudLevelLooped);
        mCloudPanel2.GetComponent<SpriteRenderer>().sprite = newLevelSprite;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        StartCoroutine(Co_FinishCloudLevelTransition(newLevelSprite));
    }

    private IEnumerator Co_FinishCloudLevelTransition(Sprite newLevelSprite)
    {
        yield return new WaitUntil(CloudLevelLooped);
        mCloudPanel1.GetComponent<SpriteRenderer>().sprite = newLevelSprite;
    }

    private IEnumerator Co_StartCloudShadowLevelTransition(Sprite newShadowLevelSprite)
    {
        yield return new WaitUntil(CloudShadowLevelLooped);
        mCloudShadowsPanel2.GetComponent<SpriteRenderer>().sprite = newShadowLevelSprite;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        StartCoroutine(Co_FinishCloudShadowLevelTransition(newShadowLevelSprite));
    }

    private IEnumerator Co_FinishCloudShadowLevelTransition(Sprite newShadowLevelSprite)
    {
        yield return new WaitUntil(CloudShadowLevelLooped);
        mCloudShadowsPanel1.GetComponent<SpriteRenderer>().sprite = newShadowLevelSprite;
    }

    private void HandleTransition(int newBackgroundIndex, int newCloudLevelIndex, int newCloudShadowLevelIndex)
    {
        Sprite newBackground;
        Sprite newCloudLevel;
        Sprite newCloudShadowLevel;

        if (newBackgroundIndex < 0)
        {
            newBackground = null;
        }
        else
        {
            newBackground = m_BackgroundSprites[newBackgroundIndex];
        }

        if (newCloudLevelIndex < 0)
        {
            newCloudLevel = null;
        }
        else
        {
            newCloudLevel = m_CloudLevelSprites[newCloudLevelIndex];
        }

        if (newCloudShadowLevelIndex < 0)
        {
            newCloudShadowLevel = null;
        }
        else
        {
            newCloudShadowLevel = m_CloudShadowSprites[newCloudShadowLevelIndex];
        }

        Sprite transitionImage = mTransitionMap[mBGPanel1.GetComponent<SpriteRenderer>().sprite.name + newBackground.name];
        StartCoroutine(Co_StartBackgroundTransition(transitionImage, newBackground));
        StartCoroutine(Co_StartCloudLevelTransition(newCloudLevel, 1));
        StartCoroutine(Co_StartCloudShadowLevelTransition(newCloudShadowLevel));
    }

    private IEnumerator Co_TriggerTransition()
    {
        yield return new WaitForSeconds(2);
        HandleTransition(1, -1, -1);
    }
}