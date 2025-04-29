using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    [SerializeField] private GameObject m_GraveStone;
    [SerializeField] private Material m_GraveMat;
    public int maxHealth = 3;
    public float invinciblityTime = 1.5f;

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

    private void OnEnable()
    {
        EventSystem.OnPickUpUpdate += TranslateDropEvent;
    }

    private void OnDisable()
    {
        EventSystem.OnPickUpUpdate -= TranslateDropEvent;
    }

    void TranslateDropEvent(DropType drop, int ammount) {
        switch (drop) {
            case DropType.LIFE:
                Heal(ammount);
                break;
        }
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
        EventSystem.PlayerDeath();
        EventSystem.SendPlayerDeathData(true, new Vector3(transform.position.x, transform.position.y, transform.position.z));

        Debug.Log("Player died");

        //Simple way of dissabling visuals for kill delay
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        SpriteRenderer[] sRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sRenderer in sRenderers)
        {
            sRenderer.enabled = false;
        }

        EventSystem.SendPlayerResultData(ServerHandler.ResultContext.PLAYER_DIED);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (isDead | isInvincible) return;

        if (collision.CompareTag("EnemyBullet")) {
            TakeDamage(1);
        }
    }
}
