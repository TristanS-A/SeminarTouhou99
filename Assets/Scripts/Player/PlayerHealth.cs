using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    public int maxHealth = 3;
    public float invinciblityTime = 1.5f;

    public event Action<int> OnHealthUpdate;
    public bool isDead { get; private set; }

    private int currentHealth;
    private bool isInvincible = false;

    private void Start() {
        currentHealth = maxHealth;
        OnHealthUpdate?.Invoke(currentHealth);
        isDead = false;
    }

    public void TakeDamage(int damage) {
        // CHECKING IF PLAYER IS ALREADY DEAD
        if (isDead || isInvincible) return;

        Debug.Log("Took Dmanadge");
        // DEALS DAMAGE & KEEPS IN APPROPIATE RANGE
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // CALLS EVENT FOR UI
        OnHealthUpdate?.Invoke(currentHealth);

        // KILLS PLAYER :(
        if (currentHealth <= 0) {
            KillPlayer();
        }
        StartCoroutine(BufferHits(invinciblityTime));
    }

    private IEnumerator BufferHits(float delay) {
        isInvincible = true;
        yield return new WaitForSeconds(delay);
        isInvincible = false;
    }

    public void Heal(int amount) {
        // CHECKING IF PLAYER IS ALREADY DEAD
        if (isDead) return;

        // HEALS & KEEPS IN APPROPIATE RANGE
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // CALLS EVENT FOR UI
        OnHealthUpdate?.Invoke(currentHealth);
    }

    private void KillPlayer() {
        isDead = true;
        EventSystem.fireEvent(new EventType(EventType.EventTypes.PLAYER_DIED));
        Debug.Log("Player died");
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (isDead | isInvincible) return;

        if (collision.CompareTag("EnemyBullet")) {
            TakeDamage(1);

            // Do we wanna destroy these?
            // Destroy(collision.gameObject);
        }
    }
}
