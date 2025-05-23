using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    // Start is called before the first frame update

    private void OnEnable()
    {
        EventSystem.OnPickUpUpdate += UpdateDrop;
    }
    void UpdateDrop(DropType type, float ammount)
    {
        if (type == DropType.SCORE)
        {
            PlayerInfo.PlayerPoints += (int)ammount;
            text.text = PlayerInfo.PlayerPoints.ToString();
        }
       

    }
    private void OnDisable()
    {
        EventSystem.OnPickUpUpdate -= UpdateDrop;
    }
}
