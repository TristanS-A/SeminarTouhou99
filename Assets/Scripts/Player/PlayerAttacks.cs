using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private int defensiveBombCount;

    [Header("Offensive Bomb")]
    [SerializeField] private int maxOffensiveBombs = 3;
    private int offensiveBombCount;

    [Header("Other")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject screenObject;
    [SerializeField] private Transform leftBulletSpawn, rightBulletSpawn;

    private List<GameObject> bullets = new();
    private bool isShooting = false;

    private PlayerMovement playerMovement;

    private void Start() {
        playerMovement = GetComponent<PlayerMovement>();
        target = GameObject.FindGameObjectWithTag("Enemy").transform;

        defensiveBombCount = maxDefensiveBombs;
        offensiveBombCount = maxOffensiveBombs;
        EventSystem.DefensiveBombAttack(defensiveBombCount);
        EventSystem.OffensiveBombAttack(offensiveBombCount);
    }

    void Update() {
        if (Input.GetKeyDown(shootKey) && !isShooting) {
            StartCoroutine(ShootBullets());
        }

        HandleDefensiveBomb();
        HandleOffensiveBomb();
    }

    void FixedUpdate() {
        float screenYMin = screenObject.transform.position.y - screenObject.transform.localScale.y / 2;
        float screenYMax = screenObject.transform.position.y + screenObject.transform.localScale.y / 2;

        for (int i = bullets.Count - 1; i >= 0; i--) {
            if (bullets[i] != null) {
                bullets[i].transform.position += bulletSpeed * Time.deltaTime * Vector3.up;

                if (playerMovement.IsInFocusTime()) {
                    GameObject bullet = bullets[i];
                    Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();

                    Vector2 direction = (Vector2) target.position - bulletRB.position;
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

    private void HandleDefensiveBomb() {
        if (Input.GetKey(defensiveBombKey) && defensiveBombCount > 0) {
            defensiveBombCount--;

            var enemy = target.gameObject.GetComponent<TempEnemy>();
            enemy.GetSequencer().CleanSequencer();

            EventSystem.DefensiveBombAttack(defensiveBombCount);
        }
    }


    private void HandleOffensiveBomb() {
        if (Input.GetKey(offensiveBombKey) && offensiveBombCount > 0) {
            offensiveBombCount--;
            EventSystem.OffensiveBombAttack(offensiveBombCount);
        }
    }

    public static int GetDamageAmount() { return bulletDamage; }
    public int GetDefensiveBombCount => defensiveBombCount;
    public int GetOffensiveBombCount => offensiveBombCount;
}
