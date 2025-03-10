using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCountDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mPlayerCountText;

    private void OnEnable()
    {
        EventSystem.numberOfJoinedPlayersChanged += OnPlayerCountChange;
    }

    private void OnDisable()
    {
        EventSystem.numberOfJoinedPlayersChanged -= OnPlayerCountChange;
    }

    private void OnPlayerCountChange(int newPlayerCount)
    {
        mPlayerCountText.text = "Players joined: " + newPlayerCount.ToString();
    }
}
