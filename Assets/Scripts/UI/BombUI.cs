using UnityEngine;

public class BombUI : MonoBehaviour {
    public GameObject offensivePrefab, defensivePrefab;
    public Transform offensiveContainer, defensiveContainer;
    public float xOffset;

    [SerializeField] private PlayerAttacks playerAttacks;

    private bool wasRemoved = false;

    private void Start() {
        if (playerAttacks != null) {
            EventSystem.OnDefensiveBombAttack += UpdateDefensiveBomb;
            EventSystem.OnOffensiveBombAttack += UpdateOffensiveBomb;
            UpdateDefensiveBomb(playerAttacks.GetDefensiveBombCount);
            UpdateOffensiveBomb(playerAttacks.GetOffensiveBombCount);
        }
    }

    private void UpdateDefensiveBomb(int amount) {
        HandleUI(amount, defensivePrefab, defensiveContainer);
    }

    private void UpdateOffensiveBomb(int amount) {
        HandleUI(amount, offensivePrefab, offensiveContainer);
    }

    private void HandleUI(int amount, GameObject prefab, Transform container) {
        // Clear existing icons in the container
        foreach (Transform child in container) {
            Destroy(child.gameObject);
        }

        // Add the exact number of icons needed
        for (int i = 0; i < amount; i++) {
            GameObject icon = Instantiate(prefab, container);
            RectTransform rect = icon.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(i * xOffset, 0);
        }
    }

    // OFFLOADS UI FROM EVENT
    private void OnDestroy() {
        if (playerAttacks != null) {
            EventSystem.OnDefensiveBombAttack -= UpdateDefensiveBomb;
            EventSystem.OnOffensiveBombAttack -= UpdateDefensiveBomb;
        }
    }
}
