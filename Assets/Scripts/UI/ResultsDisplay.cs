using System.Collections;
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
        public clientHandler.PlayerSendResultData resultData;
        public GameObject resultOBJ;
    }

    private List<ResultUI> mSortedResults;

    private void OnEnable()
    {
        EventSystem.onReceiveResult += InsertResult;
    }

    private void OnDisable()
    {
        EventSystem.onReceiveResult -= InsertResult;
    }

    private void InsertResult(clientHandler.PlayerSendResultData result)
    {
        ResultUI newResult = new();
        newResult.resultData = result;
        newResult.resultOBJ = Instantiate(m_ResultContentPrefab, transform);
        newResult.resultOBJ.GetComponentInChildren<TextMeshProUGUI>().text = result.name;

        for (int i = 0; i < mSortedResults.Count; i++)
        {
            if (GetIfNewScoreIsHigher(result, mSortedResults[i].resultData))
            {
                mSortedResults.Insert(i, newResult);
                newResult.resultOBJ.transform.SetSiblingIndex(i);
                mSortedResults[i].resultOBJ.transform.SetSiblingIndex(i + 1);
                return; 
            }
        }

        newResult.resultOBJ.transform.SetSiblingIndex(-1);
        mSortedResults.Add(newResult);
    }

    private bool GetIfNewScoreIsHigher(clientHandler.PlayerSendResultData newResult, clientHandler.PlayerSendResultData storedResult)
    {
        return newResult.points + newResult.time > storedResult.points + storedResult.time;
    }
}
