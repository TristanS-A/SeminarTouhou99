using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerAttacks : MonoBehaviour {
    [Header("Keybinds")]
    public KeyCode shootKey = KeyCode.Z;
    public KeyCode defensiveBombKey = KeyCode.X;
    public KeyCode offensiveBombKey = KeyCode.C;

    [Header("Bullet Stats")]
    [Range(1, 8)]
    public float bulletSpeed = 4.5f;
    public float bulletDelay = 0.1f;
    public static int bulletDamage = 1;

    [Header("Homing Missle")]
    public Transform target;
    [Range(1, 8)]
    public int rotateSpeed = 2;

    [Header("Defensive Bomb")]
    [SerializeField] private int maxDefensiveBombs = 3;
    [SerializeField] private float defensiveBombDelay = 1;
    private int defensiveBombCount;
    private bool isDefensiveBombDelayed = false;

    [Header("Offensive Bomb")]
    [SerializeField] private int maxOffensiveBombs = 3;
    [SerializeField] private float offensiveBombDelay = 1;
    private int offensiveBombCount;
    private int attackIndex = 0;
    private bool isOffensiveBombDelayed = false;


    private static int BOMB_COST = 1;

    [Header("Other")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject screenObject;
    [SerializeField] private Transform leftBulletSpawn, rightBulletSpawn;
    [SerializeField] private Sequencer playerSequencer;

    private List<GameObject> bullets = new();
    private bool isShooting = false;

    private PlayerMovement playerMovement;

    private void Start() {
        playerMovement = GetComponent<PlayerMovement>();
        target = GameObject.FindGameObjectWithTag("Enemy").transform;

        defensiveBombCount = maxDefensiveBombs;
        offensiveBombCount = maxOffensiveBombs;
        EventSystem.DefensiveBombAttack(defensiveBombCount);
        EventSystem.OffensiveBombAttackUI(offensiveBombCount);
    }

    void Update() {
        if (Input.GetKeyDown(shootKey) && !isShooting) {
            StartCoroutine(ShootBullets());
        }


        if (target == null) {
            //this will need to be fixed
            var list = GameObject.FindGameObjectsWithTag("Enemy");

            if (list == null) {
                return;
            }

            float closePosition = float.PositiveInfinity;
            Transform obj = null;

            foreach (var item in list) {
                Transform test = item.transform;

                if (Vector2.Distance((Vector2)test.position, (Vector2)transform.position) < closePosition) {
                    obj = test;
                    closePosition = Vector2.Distance((Vector2)test.position, (Vector2)transform.position);
                }
            }

            target = obj;
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

                if (playerMovement.IsInFocusTime() && target != null) {
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
            StartCoroutine(DelayOffensiveBomb(offensiveBombDelay, cost));
        }
    }

    public void SpawnOffensiveBomb(Vector2 pos) {
        playerSequencer.SetSpawnPos(pos);
        playerSequencer.enabled = true;
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

    public static int GetDamageAmount() { return bulletDamage; }

    // Waits for the lifetime of the AttackData, disables the sequencer, cleans and increments to next index
    private IEnumerator TurnOffSequencer(float lifetime) {
        yield return new WaitForSeconds(lifetime);
        playerSequencer.enabled = false;
        playerSequencer.CleanSequencer();
        attackIndex++;
    }

    private IEnumerator DelayOffensiveBomb(float delay, int cost) {
        isOffensiveBombDelayed = true;

        if (Input.GetKeyDown(offensiveBombKey) && offensiveBombCount > 0) { 
            offensiveBombCount -= cost;
            EventSystem.OffensiveBombAttackUI(offensiveBombCount);
            EventSystem.FireOffensiveBomb(transform.position);
            yield return new WaitForSeconds(delay);
        }
        isOffensiveBombDelayed = false;
    }

    private IEnumerator DelayDefensiveBomb(float delay, int cost) {
        isDefensiveBombDelayed = true;

        if (Input.GetKeyDown(defensiveBombKey) && defensiveBombCount > 0) {
            defensiveBombCount -= cost;
            var enemy = target.gameObject.GetComponent<TempEnemy>();
            enemy.GetSequencer().CleanSequencer();

            EventSystem.DefensiveBombAttack(defensiveBombCount);
            yield return new WaitForSeconds(delay);
        }
        isDefensiveBombDelayed = false;
    }

    public int GetDefensiveBombCount => defensiveBombCount;
    public int GetOffensiveBombCount => offensiveBombCount;
    public static int GetBombCost => BOMB_COST;
    private void OnEnable() => EventSystem.OnOffensiveBombAttack += SpawnOffensiveBomb;
    private void OnDisable() => EventSystem.OnOffensiveBombAttack -= SpawnOffensiveBomb;

}
