using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TempEnemy : MonoBehaviour {
    public enum EnemyType
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

    private float currentHealth;
    protected int currentStage = 0;
    private float currentRespawnTime;
    private bool isInvincible = false;

    [SerializeField] public EnemyType mEnemyType = EnemyType.ENEMY;

    [SerializeField] private Sequencer sequencer;
    [SerializeField] List<SequeceContainer> containter = new List<SequeceContainer>();
    [SerializeField] protected GameObject m_DeathAni;
    [SerializeField] private float mDeathAniScale = 0;
    [SerializeField] private Sprite mDeathAniSprite;

    [SerializeField] private Sprite mIntroAniSprite;
    [SerializeField] private float mIntroAniScale;
    [SerializeField] private AudioClip m_BossSpawnSFX;

    protected UnityAction StageComplete;

    //[SerializeField] DropTypes drops = new();
    private int conIndex = 0;

    [Header("Sounds")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;

    //Bool to not play multiple bullet sfx sounds at once (this would make audio sound bad)
    private bool mPlayDamageSFX = true;

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

        GameObject introAni = Instantiate(m_DeathAni, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1), Quaternion.identity);

        if (mIntroAniScale == 0)
        {
            introAni.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x, transform.localScale.x);
        }
        else
        {
            introAni.transform.localScale = new Vector3(mIntroAniScale, mIntroAniScale, mIntroAniScale);
        }

        if (mIntroAniSprite != null)
        {
            SpriteRenderer[] sRenderers = introAni.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sRenderer in sRenderers)
            {
                sRenderer.sprite = mIntroAniSprite;
            }
        }

        SoundManager.Instance.PlaySFXClip(m_BossSpawnSFX, transform, 1f);
    }

    public void TakeDamage(int stage, float damage) {
        // CHECKING IF PLAYER IS ALREADY DEAD
        if (IsDead || isInvincible) return;

        // DEALS DAMAGE & KEEPS IN APPROPIATE RANGE
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, stages[stage].maxHealth);

        if (mPlayDamageSFX)
        { 
            SoundManager.Instance.PlaySFXClip(damageSound, transform, 1f);
            mPlayDamageSFX = false;
            StartCoroutine(Co_ResetPlayDamageSFX());
        }

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
                SoundManager.Instance.PlaySFXClip(deathSound, transform, 1f);

                Kill();
            }
        }
    }

    private IEnumerator Co_ResetPlayDamageSFX()
    {
        yield return new WaitForSeconds(0.05f);
        mPlayDamageSFX = true;
    }

    //I think this naming is wrong?
    protected virtual void Kill()
    {
        Debug.Log("Killed");
        IsDead = true;
        sequencer.ClearAttackList();
        sequencer.CleanSequencer();
        EventSystem.OnEnemyDeath();

        var itemToDrop = stages[currentStage].drops;
        if (itemToDrop != null)
        {
            itemToDrop.SetLocation(gameObject.transform.position);
            DropEvent evt = new(itemToDrop);
            EventSystem.fireEvent(evt);
        }

        GameObject deathAni = Instantiate(m_DeathAni, transform.position, Quaternion.identity);

        if (mDeathAniScale == 0)
        {
            deathAni.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x, transform.localScale.x);
        }
        else
        {
            deathAni.transform.localScale = new Vector3(mDeathAniScale, mDeathAniScale, mDeathAniScale);
        }

        if (mDeathAniSprite != null)
        {
            SpriteRenderer[] sRenderers = deathAni.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sRenderer in sRenderers)
            {
                sRenderer.sprite = mDeathAniSprite;
            }
        }

        deathAni.transform.eulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0, 180));

        if (mEnemyType == EnemyType.FINAL_BOSS)
        {
            Destroy(sequencer);

            Destroy(GetComponentInChildren<BoxCollider2D>());

            SpriteRenderer[] sRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sRenderer in sRenderers)
            {
                sRenderer.enabled = false;
            }

            //Lazy
            GameObject playerOBJ = GameObject.FindGameObjectWithTag("Player");

            //Handles sending win data event for other handling of a player win (from client)
            //The z = 2 makes the grave show up in front the bullets
            EventSystem.SendPlayerWinData(true, new Vector3(playerOBJ.transform.position.x, playerOBJ.transform.position.y, playerOBJ.transform.position.z));

            //Finishes the level and triggers the sending of result data
            EventSystem.SendPlayerResultData(ServerHandler.ResultContext.PLAYER_WON);

            Destroy(this.gameObject);
        }

        else if (mEnemyType == EnemyType.MID_BOSS)
        {
            Destroy(this.gameObject);
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
        StageComplete?.Invoke();
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

    private IEnumerator DelayKillForSound(float delay) {
        yield return new WaitForSeconds(delay);
        Kill();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (IsDead | isInvincible) return;

        if (collision.CompareTag("Bullet") && !IsDead) {
            Destroy(collision.gameObject);
            TakeDamage(currentStage, PlayerAttacks.GetDamageAmount());
        }
    }

    public int GetCurrentMaxHealth() => stages[currentStage].maxHealth;
    public float GetCurrentRespawnTime() => stages[currentStage].respawnTime;
    public Sequencer GetSequencer() { return sequencer; }
}
