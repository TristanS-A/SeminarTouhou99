using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class TempEnemy : MonoBehaviour {
    private enum EnemyType
    {
        ENEMY,
        MID_BOSS,
        FINAL_BOSS
    }

    [Serializable]
    public class BossStage {
        public int maxHealth = 100;
        public float respawnTime = 1.5f;
        public DropTypes drops;
    }

    [Serializable]
    public class SequeceContainer {
        public List<AttackData> attacks;
    }

    public List<BossStage> stages;
    public bool IsDead { get; private set; }

    private int currentHealth;
    private int currentStage = 0;
    private float currentRespawnTime;
    private bool isInvincible = false;

    [SerializeField] private EnemyType mEnemyType = EnemyType.ENEMY;

    [SerializeField] private Sequencer sequencer;
    [SerializeField] List<SequeceContainer> containter = new List<SequeceContainer>();

    //[SerializeField] DropTypes drops = new();
    private int conIndex = 0;

    private void Start() 
    {
        Init();
    }

    protected virtual void Init()
    {
        currentHealth = stages[0].maxHealth;
        currentRespawnTime = stages[0].respawnTime;

        // EventSystem.HealthUpdate(currentHealth);
        //EventSystem.RespawnUpdate(currentRespawnTime);

        IsDead = false;

        // WILL NYE THE SCIENCE GUY <-- this is fire
        sequencer = gameObject.GetComponent<Sequencer>();
        sequencer.SpawnEmmiter();
    }

    public void TakeDamage(int stage, int damage) {
        // CHECKING IF PLAYER IS ALREADY DEAD
        if (IsDead || isInvincible) return;

        // DEALS DAMAGE & KEEPS IN APPROPIATE RANGE
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, stages[stage].maxHealth);
        Debug.Log(" TAKE DAMADGE " + currentHealth);
        // CALLS EVENT FOR UI
        //EventSystem.EnemyHealthUpdate(currentHealth);
        if (currentHealth <= 0) {
            Debug.Log("LESS THEN CURRNET HEALTH");
            if (currentStage != stages.Count - 1) {
                // GETS RID OF THE CURRENT SEQUENCE OF ATTACKS
                sequencer.ClearAttackList();
                sequencer.CleanSequencer();

                //SPAWN DROPS IF THERE ARE DROPS
                var itemToDrop = stages[currentStage].drops;
                if (itemToDrop != null)
                {
                    itemToDrop.SetLocation(gameObject.transform.position);
                    DropEvent evt = new(itemToDrop);
                    EventSystem.fireEvent(evt);
                }

                StartCoroutine(Respawn());
            } else if (currentStage == stages.Count - 1) {
                // KILLS ENEMY :)
                Kill();
            }
        }
    }

    //I think this naming is wrong?
    protected virtual void Kill() {
        Debug.Log("Killed");
        IsDead = true;
        sequencer.ClearAttackList();
        sequencer.CleanSequencer();
        EventSystem.OnEnemyDeath();

        if (mEnemyType == EnemyType.FINAL_BOSS)
        {
            //Lazy
            GameObject playerOBJ = GameObject.FindGameObjectWithTag("Player");

            //Handles sending win data event for other handling of a player win (from client)
            //The z = 2 makes the grave show up in front the bullets
            EventSystem.SendPlayerWinData(true, new Vector3(playerOBJ.transform.position.x, playerOBJ.transform.position.y, -2));

            //Finishes the level and triggers the sending of result data
            EventSystem.SendPlayerResultData(ServerHandler.ResultContext.PLAYER_WON);
        }

        else if (mEnemyType == EnemyType.MID_BOSS)
        {

        }
        

    }

    public void Revive() {
        if (!IsDead) return;

        IsDead = false;
        currentStage++;
        currentHealth = stages[currentStage].maxHealth;
        currentRespawnTime = stages[currentStage].respawnTime;

        conIndex++;
        sequencer.SetSequeceList(containter[conIndex].attacks);
        sequencer.SpawnEmmiter();
        EventSystem.EnemyRespawnUpdate(currentRespawnTime);
    }

    private IEnumerator Respawn() {
        isInvincible = true;
        IsDead = true;

        WaitForSeconds local = new(0);

        while (currentRespawnTime >= 0) {
            currentRespawnTime -= Time.deltaTime;
            EventSystem.EnemyRespawnUpdate(currentRespawnTime);
            yield return local;
        }
        isInvincible = false;
        Revive();
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (IsDead | isInvincible) return;

        if (collision.CompareTag("Bullet")) {
            Destroy(collision.gameObject);
            TakeDamage(currentStage, PlayerAttacks.GetDamageAmount());
        }
    }

    public int GetCurrentMaxHealth() => stages[currentStage].maxHealth;
    public float GetCurrentRespawnTime() => stages[currentStage].respawnTime;
    public Sequencer GetSequencer() { return sequencer; }
}
