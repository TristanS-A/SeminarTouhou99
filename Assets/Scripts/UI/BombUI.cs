using UnityEngine;

public class BombUI : MonoBehaviour {
    public GameObject offensivePrefab, defensivePrefab;
    public Transform offensiveContainer, defensiveContainer;
    public float xOffset;

    [SerializeField] private PlayerAttacks playerAttacks;

    private void Start() {
        if (playerAttacks != null) {
            UpdateDefensiveBomb(playerAttacks.GetDefensiveBombCount);
            UpdateOffensiveBomb(playerAttacks.GetOffensiveBombCount);
        }
       
    }
    private void TranslateEvent(DropType type, float ammount)
    {
        switch (type)
        {
            case DropType.OFF_BOMB:
                //only if we have room to add
                if (playerAttacks.GetOffensiveBombCount < playerAttacks.GetMaxDefensiveBombs)
                {
                    playerAttacks.SetOffensiveBombCount(playerAttacks.GetOffensiveBombCount + (int)ammount);
                    UpdateOffensiveBomb(playerAttacks.GetOffensiveBombCount);
                }
                break;
            case DropType.DEF_BOMB:
                //only if we have room to add
                if (playerAttacks.GetDefensiveBombCount < playerAttacks.GetMaxDefensiveBombs)
                {
                    playerAttacks.SetDefensiveBombCount(playerAttacks.GetDefensiveBombCount + (int)ammount);
                    UpdateDefensiveBomb(playerAttacks.GetDefensiveBombCount);
                }
                break;
        }
    }
    private void UpdateDefensiveBomb(int amount) 
    {
        HandleUI(amount, defensivePrefab, defensiveContainer);
    }

    private void UpdateOffensiveBomb(int amount) 
    {
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

    private void OnEnable() {
        EventSystem.OnDefensiveBombAttack += UpdateDefensiveBomb;
        EventSystem.OnOffensiveBombUI += UpdateOffensiveBomb;
        EventSystem.OnPickUpUpdate += TranslateEvent;
    }

    // OFFLOADS UI FROM EVENT
    private void OnDisable() {
        EventSystem.OnDefensiveBombAttack -= UpdateDefensiveBomb;
        EventSystem.OnOffensiveBombUI -= UpdateOffensiveBomb;
        EventSystem.OnPickUpUpdate -= TranslateEvent;
    }
}
