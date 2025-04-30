using System;
using TMPro;
using UnityEngine;

public class AttackUI : MonoBehaviour {
    public TextMeshProUGUI attackText;

    private void Awake() {
        attackText.text = PlayerAttacks.bulletDamage.ToString();
    }

    private void UpdateAttackUI(DropType type, float amount) {
        switch (type) {
            case DropType.POWER:
                attackText.text = ((float)Convert.ToDouble(attackText.text) + amount).ToString();
                break;
            default:
                break;
        }
    }

    private void OnEnable() {
        EventSystem.OnPickUpUpdate += UpdateAttackUI; 
    }

    private void OnDisable() {
        EventSystem.OnPickUpUpdate -= UpdateAttackUI;
    }
}
