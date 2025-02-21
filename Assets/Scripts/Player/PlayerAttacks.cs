using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour {
    [Header("Keybinds")]
    public KeyCode shootKey = KeyCode.Z;
    public KeyCode bombKey = KeyCode.X;

    [Header("Bullet Stats")]
    [Range(1, 8)]
    public float bulletSpeed = 4.5f;
    public float bulletDelay = 0.1f;
    public int bulletDamage = 1;

    [Header("Homing Missle")]
    public Transform target;
    [Range(1, 8)]
    public float rotateSpeed = 2f;
    public bool isHomingBullet = false;

    [Header("Other")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject screenObject;
    [SerializeField] private Transform leftBulletSpawn, rightBulletSpawn;

    private List<GameObject> bullets = new();
    private bool isShooting = false/*, isHomingBullet = false*/;

    private PlayerMovement playerMovement;

    private void Start() {
        playerMovement = GetComponent<PlayerMovement>();
        target = GameObject.FindGameObjectWithTag("Enemy").transform;
    }

    void Update() {
        if (Input.GetKeyDown(shootKey) && !isShooting) {
            StartCoroutine(ShootBullets());
        }
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

    public int GetDamageAmount() { return bulletDamage; }

    private void OnTriggerEnter2D(Collider2D collision) {
    }
}
