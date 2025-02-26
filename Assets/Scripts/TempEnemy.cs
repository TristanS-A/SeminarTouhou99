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

    [Serializable]
    public class SequeceContainer
    {
        public List<AttackData> attacks;
    }

    public List<BossStage> stages;
    public event Action<int> OnHealthUpdate;
    public event Action<float> OnRespawnUpdate;
    public event Action OnEnemyDeath;
    public bool isDead { get; private set; }

    private int currentHealth;
    private int currentStage = 0;
    private float currentRespawnTime;
    private bool isInvincible = false;

    [SerializeField] private Sequencer sequencer;
    [SerializeField] List<SequeceContainer> containter = new List<SequeceContainer>();
    private int conIndex = 0;

    private void Start() {
        currentHealth = stages[0].maxHealth;
        currentRespawnTime = stages[0].respawnTime;

        OnHealthUpdate?.Invoke(currentHealth);
        OnRespawnUpdate?.Invoke(currentRespawnTime);

        isDead = false;

        // WILL NYE THE SCIENCE GUY
        sequencer = gameObject.GetComponent<Sequencer>();
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
                // GETS RID OF THE CURRENT SEQUENCE OF ATTACKS
                sequencer.ClearAttackList();
                sequencer.CleanSequencer();
                StartCoroutine(Respawn());
            } else if (currentStage == stages.Count - 1) {
                // KILLS ENEMY :)
                Kill();
            }
        }
    }

    //I think this naming is wrong?
    private void Kill() {
        Debug.Log("Killed");
        isDead = true;
        sequencer.ClearAttackList();
        sequencer.CleanSequencer();
        OnEnemyDeath?.Invoke();
    }

    public void Revive() {
        if (!isDead) return;

        isDead = false;
        currentStage++;
        currentHealth = stages[currentStage].maxHealth;
        currentRespawnTime = stages[currentStage].respawnTime;

        conIndex++;
        sequencer.SetSequeceList(containter[conIndex].attacks);
       
        OnRespawnUpdate?.Invoke(currentRespawnTime);
    }
    
    //might want to change this to be a  flag in the update loop (this seems like a lot of overhead)
    private IEnumerator Respawn() {
        isInvincible = true;
        isDead = true;

        WaitForSeconds local = new(0);
        
        while (currentRespawnTime >= 0) {
            currentRespawnTime -= Time.deltaTime;
            OnRespawnUpdate?.Invoke(currentRespawnTime);
            yield return local;
        }
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
