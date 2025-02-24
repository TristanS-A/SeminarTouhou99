using TMPro;
using UnityEngine;

public class EnemyUI : MonoBehaviour {
    public TextMeshProUGUI healthText, reviveText;

    private TempEnemy enemyHealth;

    private void Awake() {
        enemyHealth = FindObjectOfType<TempEnemy>();

        if (enemyHealth != null) {
            enemyHealth.OnHealthUpdate += UpdateHealthUI;
            enemyHealth.OnRespawnUpdate += UpdateRespawnUI;
            UpdateHealthUI(enemyHealth.GetCurrentMaxHealth());
            UpdateRespawnUI(enemyHealth.GetCurrentRespawnTime());
        }
    }

    private void Update() {
        enemyHealth.OnRespawnUpdate += UpdateRespawnUI;
    }

    // UPDATES UI ON EVENT INVOKE
    private void UpdateHealthUI(int currentHealth) {
        healthText.text = currentHealth.ToString();
    }

    private void UpdateRespawnUI(float respawnTime) {
        reviveText.text = respawnTime.ToString("F2");
    }

    // OFFLOADS UI FROM EVENT
    private void OnDestroy() {
        if (enemyHealth != null) {
            enemyHealth.OnHealthUpdate -= UpdateHealthUI;
        }
    }
}
