using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEnemy : MonoBehaviour {
    [Serializable]
    public class BossStage {
        public int maxHealth = 100;
        public float respawnTime = 1.5f;
    }

    public List<BossStage> stages;
    public event Action<int> OnHealthUpdate;
    public event Action<float> OnRespawnUpdate;
    public event Action OnPlayerDeath;
    public bool isDead { get; private set; }

    private int currentHealth;
    private int currentStage = 0;
    private float currentRespawnTime;
    private bool isInvincible = false;

    private void Start() {
        currentHealth = stages[0].maxHealth;
        currentRespawnTime = stages[0].respawnTime;

        OnHealthUpdate?.Invoke(currentHealth);
        OnRespawnUpdate?.Invoke(currentRespawnTime);
        isDead = false;
    }

    public void TakeDamage(int stage, int damage) {
        // CHECKING IF PLAYER IS ALREADY DEAD
        if (isDead || isInvincible) return;

        // DEALS DAMAGE & KEEPS IN APPROPIATE RANGE
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, stages[stage].maxHealth);

        // CALLS EVENT FOR UI
        OnHealthUpdate?.Invoke(currentHealth);

        if (currentHealth <= 0) {
            if (currentStage != stages.Count - 1) {
                Debug.Log("NEXT STAGE");
                OnRespawnUpdate.Invoke(currentRespawnTime);
                StartCoroutine(Respawn(stages[currentStage].respawnTime));
            } else if (currentStage == stages.Count) {
                // KILLS ENEMY :)
                Kill();
            }
        }
    }

    private void Kill() {
        isDead = true;
        OnPlayerDeath?.Invoke();
        Debug.Log("Enemy Died");
    }

    public void Revive() {
        if (!isDead) return;

        isDead = false;
        currentStage++;
        currentHealth = stages[currentStage].maxHealth;
        currentRespawnTime = stages[currentStage].respawnTime;
    }

    private IEnumerator Respawn(float delay) {
        isInvincible = true;
        Revive();
        yield return new WaitForSeconds(delay);
        isInvincible = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (isDead | isInvincible) return;

        if (collision.CompareTag("Bullet")) {
            Destroy(collision.gameObject);
            TakeDamage(currentStage, 1);
        }
    }

    public int GetCurrentMaxHealth() => stages[currentStage].maxHealth;
    public float GetCurrentRespawnTime() => stages[currentStage].respawnTime;
}
