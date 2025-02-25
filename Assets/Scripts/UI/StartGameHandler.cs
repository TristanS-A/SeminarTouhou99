using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartGameHandler : MonoBehaviour
{
    [SerializeField] private Button mStartButton;

    private void OnPlayerCountChange(int newPlayerCount)
    {
        //mPlayerCountText.text = "Players joined: " + newPlayerCount.ToString();
    }
}
