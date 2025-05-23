using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerAttacks : MonoBehaviour {
    [Header("Keybinds")]
    public KeyCode shootKey = KeyCode.Z;
    public KeyCode defensiveBombKey = KeyCode.X;
    public KeyCode offensiveBombKey = KeyCode.C;

    [Header("Bullet Stats")]
    [Range(1, 8)]
    public float bulletSpeed = 4.5f;
    public float bulletDelay = 0.1f;
    public float homingUnlockAmmount = 4;

    //talk about chaning this to a float for more control ;)
    public static float bulletDamage = 1;

    [Header("Homing Missle")]
    [SerializeField] private Transform target;
    [Range(1, 8)]
    [SerializeField] private int rotateSpeed = 2;

    [Header("Defensive Bomb")]
    [SerializeField] private int maxDefensiveBombs = 3;
    [SerializeField] private float defensiveBombDelay = 1;
    private int defensiveBombCount;
    private bool isDefensiveBombDelayed = false;

    [Header("Offensive Bomb")]
    [SerializeField] private int maxOffensiveBombs = 3;
    [SerializeField] private float offensiveBombDelay = 5.0f;
    private int offensiveBombCount;
    private int attackIndex = 0;
    private bool isOffensiveBombDelayed = false;

    private static int BOMB_COST = 1;

    [Header("Other")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject screenObject;
    [SerializeField] private Transform leftBulletSpawn, rightBulletSpawn;
    [SerializeField] private Sequencer playerSequencer;

    [SerializeField] private GameObject m_OffensiveBombVFX;
    [SerializeField] private GameObject m_DefensiveBombVFX;

    [Header("SFX")]
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip offensiveBombSFX;
    [SerializeField] private AudioClip defensiveBombSFX;

    private List<GameObject> bullets = new();
    private bool isShooting = false;

    private PlayerMovement playerMovement;

    private void Start() {
        playerMovement = GetComponent<PlayerMovement>();

        defensiveBombCount = maxDefensiveBombs;
        offensiveBombCount = maxOffensiveBombs;
        EventSystem.DefensiveBombAttack(defensiveBombCount);
        EventSystem.OffensiveBombAttackUI(offensiveBombCount);

        bulletDamage = 1;
    }

    void Update() {
        if (Input.GetKeyDown(shootKey) && !isShooting) {
            SoundManager.Instance.PlaySFXClip(attackSFX, transform, 1f);
            StartCoroutine(ShootBullets());
        }

        if (playerMovement.IsInFocusTime()) {
            //Cancels target if behind player or dies
            if (target != null) {
                if (target.transform.position.y < transform.position.y) {
                    target = null;
                }
            }

            //Assigns target
            if (target == null || (target.gameObject.GetComponent<BaseEnemy>() != null && (target.gameObject.GetComponent<BaseEnemy>().isAtEnd || target.gameObject.GetComponent<BaseEnemy>().isDead))) {
                GameObject waveManager = GameObject.FindGameObjectWithTag("WaveManager");
                //if the active list is empty then do another thing
                List<Tuple<GameObject, BaseEnemy>> list = waveManager.GetComponent<WaveManager>().GetActiveList();

                if (list == null || list.Count == 0) {

                    var newTargetObj = waveManager.GetComponent<WaveManager>().GetBossObject;

                    //this indicateds boss logic
                    if (newTargetObj != null && newTargetObj.Item1 != null) 
                    {
                        target = newTargetObj.Item1.transform;
                    }

                    return;
                }

                float closePosition = float.PositiveInfinity;
                Transform obj = null;

                foreach (Tuple<GameObject, BaseEnemy> item in list) {
                    if (item.Item1 != null) {
                        Transform test = item.Item1.transform;

                        if (Vector2.Distance((Vector2)test.position, (Vector2)transform.position) < closePosition && test.position.y > transform.position.y && !item.Item2.isDead) {
                            obj = test;
                            closePosition = Vector2.Distance((Vector2)test.position, (Vector2)transform.position);
                        }
                    }
                }

                target = obj;
            }
        }

        HandleDefensiveBomb(BOMB_COST);
        HandleOffensiveBomb(BOMB_COST);
    }

    void FixedUpdate() {
        float screenYMin = screenObject.transform.position.y - screenObject.transform.localScale.y / 2;
        float screenYMax = screenObject.transform.position.y + screenObject.transform.localScale.y / 2;

        for (int i = bullets.Count - 1; i >= 0; i--) {
            if (bullets[i] != null) {
                bullets[i].transform.position += bulletSpeed * Time.deltaTime * Vector3.up;

                if ((playerMovement.IsInFocusTime() && bulletDamage >= homingUnlockAmmount)&& target != null) {
                    GameObject bullet = bullets[i];
                    Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();

                    Vector2 direction = (Vector2)target.position - bulletRB.position;
                    direction.Normalize();

                    float rotation = Vector3.Cross(direction, bullet.transform.up).z;
                    bulletRB.angularVelocity = -rotation * (rotateSpeed * 100);
                    bulletRB.velocity = bullet.transform.up * bulletSpeed;
                }

                // Check if the bullet is out of the y range of the reference object
                if (bullets[i].transform.position.y < screenYMin || bullets[i].transform.position.y > screenYMax) {
                    Destroy(bullets[i]);
                    bullets.RemoveAt(i);
                }
            }
        }
    }

    private void HandleDefensiveBomb(int cost) {
        if (!isDefensiveBombDelayed) {
            StartCoroutine(DelayDefensiveBomb(defensiveBombDelay, cost));
        }
    }

    // Each bomb enables the sequencer on trigger and based on the lifetime of the attack will spawn and then disable, incrementing to the next index
    private void HandleOffensiveBomb(int cost) {
        if (!isOffensiveBombDelayed) {
            if (Input.GetKeyDown(offensiveBombKey) && offensiveBombCount > 0)
            {
                isOffensiveBombDelayed = true;
                offensiveBombCount -= cost;
                SoundManager.Instance.PlaySFXClip(offensiveBombSFX, transform, 1f);
                EventSystem.OffensiveBombAttackUI(offensiveBombCount);
                EventSystem.FireOffensiveBomb(transform.position);

                GameObject bombVFX = Instantiate(m_OffensiveBombVFX, transform.position, Quaternion.identity);
                bombVFX.transform.parent = transform;

                StartCoroutine(DelayOffensiveBomb(offensiveBombDelay, cost));
            }
        }
    }

    public void SpawnOffensiveBomb(Vector2 pos) {
        playerSequencer.SetSpawnPos(pos);
        playerSequencer.enabled = true;

        //Handle spawn offensive bomb vfx
        GameObject bombVFX = Instantiate(m_OffensiveBombVFX, pos, Quaternion.identity);
        bombVFX.GetComponent<VisualEffect>().SetBool("ShouldBeHologram", true);

        StartCoroutine(TurnOffSequencer(playerSequencer.GetAttacks[attackIndex].GetCustomLifeTime()));
    }

    private IEnumerator ShootBullets() {
        isShooting = true;

        while (Input.GetKey(shootKey)) {
            GameObject leftBullet = Instantiate(bulletPrefab, leftBulletSpawn);
            GameObject rightBullet = Instantiate(bulletPrefab, rightBulletSpawn);
            bullets.Add(leftBullet);
            bullets.Add(rightBullet);

            yield return new WaitForSeconds(bulletDelay);
        }
        isShooting = false;
    }

    public static float GetDamageAmount() { return bulletDamage; }

    // Waits for the lifetime of the AttackData, disables the sequencer, cleans and increments to next index
    private IEnumerator TurnOffSequencer(float lifetime) {
        yield return new WaitForSeconds(lifetime);
        playerSequencer.enabled = false;
        playerSequencer.CleanSequencer();
        attackIndex++;
    }

    private IEnumerator DelayOffensiveBomb(float delay, int cost) {
        yield return new WaitForSeconds(delay);
        isOffensiveBombDelayed = false;
    }

    private IEnumerator DelayDefensiveBomb(float delay, int cost) {
        isDefensiveBombDelayed = true;

        if (Input.GetKeyDown(defensiveBombKey) && defensiveBombCount > 0) {
            defensiveBombCount -= cost;
            EventSystem.DefensiveBombAttack(defensiveBombCount);
            SoundManager.Instance.PlaySFXClip(defensiveBombSFX, transform, 1f);
            BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);

            foreach (var enemy in enemies) {
                if (enemy.TryGetComponent<Sequencer>(out var enemySequencer)) {
                    enemySequencer.CleanSequencer();
                }
            }

            var waveManager = FindAnyObjectByType<WaveManager>().GetBossObject;
            waveManager?.Item2.CleanSequencer();

            GameObject bombVFX = Instantiate(m_DefensiveBombVFX, transform.position, Quaternion.identity);
            bombVFX.transform.parent = transform;

            yield return new WaitForSeconds(delay);
        }
        isDefensiveBombDelayed = false;
    }

    private void TranslateEvent(DropType drop, float ammount) {
        switch (drop) {
            //this is where the homing unlock will be (just check the damnage) - will
            case DropType.POWER:
                if (bulletDamage < 4) {
                    bulletDamage += ammount;
                }
                break;
        }
    }
    public int GetDefensiveBombCount => defensiveBombCount;
    public int GetOffensiveBombCount => offensiveBombCount;
    //might want to change this later
    public void SetDefensiveBombCount(int ammount) => defensiveBombCount = ammount;
    public void SetOffensiveBombCount(int ammount) => offensiveBombCount = ammount;
    public int GetMaxDefensiveBombs => maxDefensiveBombs;
    public int GetMaxOffensiveBombs => maxOffensiveBombs;
    public static int GetBombCost => BOMB_COST;
    private void OnEnable() {
        EventSystem.OnOffensiveBombAttack += SpawnOffensiveBomb;
        EventSystem.OnPickUpUpdate += TranslateEvent;
    }

    private void OnDisable() {
        EventSystem.OnOffensiveBombAttack -= SpawnOffensiveBomb;
        EventSystem.OnPickUpUpdate -= TranslateEvent;
    }
}
