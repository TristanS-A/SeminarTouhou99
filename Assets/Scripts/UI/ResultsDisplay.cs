using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mFirstPlaceText;
    [SerializeField] private TextMeshProUGUI mSecondPlaceText;
    [SerializeField] private TextMeshProUGUI mThirdPlaceText;

    [SerializeField] private GameObject mResultsContent;
    [SerializeField] private GameObject m_ResultContentPrefab;

    struct ResultUI
    {
        public ClientHandler.PlayerSendResultData resultData;
        public GameObject resultOBJ;
    }

    private List<ResultUI> mSortedResults = new();
    private HashSet<uint> mStoredIDs = new();

    private void OnEnable()
    {
        EventSystem.onReceiveResult += InsertResult;
    }

    private void OnDisable()
    {
        EventSystem.onReceiveResult -= InsertResult;
    }

    private void Start()
    {
        SoundManager.Instance.PlayMusic(2);
    }

    private void InsertResult(ClientHandler.PlayerSendResultData result)
    {
        if (!mStoredIDs.Contains(result.playerID))
        { 
            ResultUI newResult = new();
            newResult.resultData = result;
            newResult.resultOBJ = Instantiate(m_ResultContentPrefab, mResultsContent.transform);
            TextMeshProUGUI[] resultTexts = newResult.resultOBJ.GetComponentsInChildren<TextMeshProUGUI>();

            resultTexts[0].text = result.name;
            resultTexts[1].text = result.time.ToString("0.00");
            resultTexts[2].text = result.points.ToString();
            resultTexts[3].text = result.score.ToString();

            mStoredIDs.Add(result.playerID);

            for (int i = 0; i < mSortedResults.Count; i++)
            {
                if (GetIfNewScoreIsHigher(result, mSortedResults[i].resultData))
                {
                    mSortedResults[i].resultOBJ.transform.SetSiblingIndex(i + 1);
                    newResult.resultOBJ.transform.SetSiblingIndex(i);
                    mSortedResults.Insert(i, newResult);
                    UpdateTop3();
                    return;
                }
            }

            newResult.resultOBJ.transform.SetSiblingIndex(mResultsContent.transform.childCount);
            mSortedResults.Add(newResult);
            UpdateTop3();
        }
    }

    private bool GetIfNewScoreIsHigher(ClientHandler.PlayerSendResultData newResult, ClientHandler.PlayerSendResultData storedResult)
    {
        return newResult.score > storedResult.score;
    }

    private void UpdateTop3()
    {
        try
        {
            mFirstPlaceText.text = mSortedResults[0].resultData.name;
            mSecondPlaceText.text = mSortedResults[1].resultData.name;
            mThirdPlaceText.text = mSortedResults[2].resultData.name;
        }
        catch { }
    }
}
