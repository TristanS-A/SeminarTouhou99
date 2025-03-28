using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    // Start is called before the first frame update
    int score;
    private void OnEnable()
    {
        EventSystem.OnPickUpUpdate += UpdateDrop;
    }
    void UpdateDrop(DropType type, int ammount)
    {
        
        score += ammount;
        PlayerInfo.PlayerPoints = score;
        text.text = score.ToString();

    }
    private void OnDisable()
    {
        EventSystem.OnPickUpUpdate -= UpdateDrop;
    }
}
