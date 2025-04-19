using UnityEngine;

public class HealthUI : MonoBehaviour {
    public GameObject healthImagePrefab;
    public Transform healthContainer;
    public float xOffset;

    private PlayerHealth playerHealth;

    private void OnEnable()
    {
        EventSystem.OnHealthUpdate += UpdateUI;
    }

    private void OnDisable()
    {
        EventSystem.OnHealthUpdate -= UpdateUI;
    }

    private void Start() {
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null) {
            UpdateUI(playerHealth.maxHealth);
        }
    }

    // UPDATES UI ON EVENT INVOKE
    private void UpdateUI(int currentHealth) {
        // IF THERE ARE ANY PREFABS LEFT OVER FROM RUNTIME, WILL REMOVE 
        foreach (Transform t in healthContainer) {
            Destroy(t.gameObject);
        }

        // OFFSETS PREFABS ON X-AXIS BY 75
        for (int i = 0; i < currentHealth; i++) {
            GameObject icon = Instantiate(healthImagePrefab, healthContainer);
            RectTransform rect = icon.GetComponent<RectTransform>();

            rect.anchoredPosition = new Vector2(i * xOffset, 0);
        }
    }
}
