using TMPro;
using UnityEngine;

public class EnemyUI : MonoBehaviour {
    public TextMeshProUGUI healthText, reviveText;

    private TempEnemy enemyHealth;

    private void Awake() {
        enemyHealth = FindObjectOfType<TempEnemy>();

        if (enemyHealth != null) {
            EventSystem.OnEnemyHealthUpdate += UpdateHealthUI;
            EventSystem.OnEnemyDeathUpdate += UpdateDeathUI;
            UpdateHealthUI(enemyHealth.GetCurrentMaxHealth());
            UpdateRespawnUI(enemyHealth.GetCurrentRespawnTime());
            EventSystem.OnEnemyRespawnUpdate.AddListener(UpdateRespawnUI);
        }
    }

    private void Update() {
        EventSystem.OnEnemyRespawnUpdate.AddListener(UpdateRespawnUI);
    }

    // UPDATES UI ON EVENT INVOKE
    private void UpdateHealthUI(int currentHealth) {
        healthText.text = currentHealth.ToString();
    }

    private void UpdateRespawnUI(float respawnTime) {
        reviveText.text = respawnTime.ToString("F2");
    }

    private void UpdateDeathUI() {
        healthText.enabled = false;
        reviveText.enabled = false;
    }

    // OFFLOADS UI FROM EVENT
    private void OnDestroy() {
        if (enemyHealth != null) {
            EventSystem.OnEnemyHealthUpdate -= UpdateHealthUI;
            EventSystem.OnEnemyRespawnUpdate.RemoveListener(UpdateRespawnUI);
            EventSystem.OnEnemyDeathUpdate -= UpdateDeathUI;
        }
    }
}
