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

    [SerializeField] Sequencer sqr;
    
    private void Start() {
        currentHealth = stages[0].maxHealth;
        currentRespawnTime = stages[0].respawnTime;

        OnHealthUpdate?.Invoke(currentHealth);
        OnRespawnUpdate?.Invoke(currentRespawnTime);

        isDead = false;

        //this is to get the squecer so we can know about it
        sqr = gameObject.GetComponent<Sequencer>();
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
            Debug.Log("Checking current health "  +  currentHealth);
            Debug.Log("Stage: " + currentStage + " " + stages.Count);
            if (currentStage != stages.Count - 1) {
                Debug.Log("NEXT STAGE");

                //get rid of the current sequece of attacks
                Destroy(sqr);

                StartCoroutine(Respawn(stages[currentStage].respawnTime));
                //OnRespawnUpdate.Invoke(currentRespawnTime);
            } else if (currentStage == stages.Count - 1) {
                // KILLS ENEMY :)
                Debug.Log("Killing Enemy");
                Kill();
            }
        }
    }

    //I think this naming is wrong?
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

        OnRespawnUpdate?.Invoke(currentRespawnTime);
        //update respawn timer
        Debug.Log("RESURECTRION " + currentHealth);
    }
    
    //might want to change this to be a  flag in the update loop (this seems like a lot of overhead)
    private IEnumerator Respawn(float delay) {
        isInvincible = true;
        isDead = true;

        WaitForSeconds local = new WaitForSeconds(0);
        
        while (currentRespawnTime >= 0){

            currentRespawnTime -= Time.deltaTime;
            Debug.Log("time left" + currentRespawnTime);
            OnRespawnUpdate?.Invoke(currentRespawnTime);

            yield return local;
        }
        //yield return new WaitForSeconds(delay);
        isInvincible = false;
        Revive();
        yield return null;
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
