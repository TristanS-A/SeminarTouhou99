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
    public class SequeceContainer {
        public List<AttackData> attacks;
    }

    public List<BossStage> stages;
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

        eventSystem.HealthUpdate(currentHealth);
        eventSystem.RespawnUpdate(currentRespawnTime);

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
        eventSystem.HealthUpdate(currentHealth);

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
        eventSystem.OnDeath();
    }

    public void Revive() {
        if (!isDead) return;

        isDead = false;
        currentStage++;
        currentHealth = stages[currentStage].maxHealth;
        currentRespawnTime = stages[currentStage].respawnTime;

        conIndex++;
        sequencer.SetSequeceList(containter[conIndex].attacks);

        eventSystem.RespawnUpdate(currentRespawnTime);
    }

    //might want to change this to be a  flag in the update loop (this seems like a lot of overhead)
    private IEnumerator Respawn() {
        isInvincible = true;
        isDead = true;

        WaitForSeconds local = new(0);

        while (currentRespawnTime >= 0) {
            currentRespawnTime -= Time.deltaTime;
            eventSystem.RespawnUpdate(currentRespawnTime);
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
            TakeDamage(currentStage, PlayerAttacks.GetDamageAmount());
        }
    }

    public int GetCurrentMaxHealth() => stages[currentStage].maxHealth;
    public float GetCurrentRespawnTime() => stages[currentStage].respawnTime;
    public Sequencer GetSequencer() { return sequencer; }
}
