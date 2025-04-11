using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour {
    [SerializeField] private GameObject m_GraveStone;
    [SerializeField] private Material m_GraveMat;
    public int maxHealth = 3;
    public float invinciblityTime = 1.5f;

    public event UnityAction<int> OnHealthUpdate;
    public bool isDead { get; private set; }

    private int currentHealth;
    private bool isInvincible = false;

    [Header("Sounds")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip deathSound;

    private void Start() {
        currentHealth = maxHealth;
        EventSystem.HealthUpdate(currentHealth);
        isDead = false;
    }

    public void TakeDamage(int damage) {
        // CHECKING IF PLAYER IS ALREADY DEAD
        if (isDead || isInvincible) return;

        Debug.Log("Took Dmanadge");
        // DEALS DAMAGE & KEEPS IN APPROPIATE RANGE
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        SoundManager.Instance.PlaySFXClip(damageSound, transform, 1f);

        // CALLS EVENT FOR UI
        EventSystem.HealthUpdate(currentHealth);

        // KILLS PLAYER :(
        if (currentHealth <= 0) {
            SoundManager.Instance.PlaySFXClip(deathSound, transform, 1f);
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
        SoundManager.Instance.PlaySFXClip(healSound, transform, 1f);

        // CALLS EVENT FOR UI
        EventSystem.HealthUpdate(currentHealth);
    }

    private void KillPlayer() {
        isDead = true;
        //Fires event to handle other on player death stuff
        EventSystem.SendPlayerDeathData(true, new Vector3(transform.position.x, transform.position.y, 1));
        EventSystem.SendPlayerResultData(ServerHandler.ResultContext.PLAYER_DIED);

        Debug.Log("Player died");
        Destroy(gameObject, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (isDead | isInvincible) return;

        if (collision.CompareTag("EnemyBullet")) {
            TakeDamage(1);
        }
    }
}
