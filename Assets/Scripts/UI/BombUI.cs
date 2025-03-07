using UnityEngine;

public class BombUI : MonoBehaviour {
    public GameObject offensivePrefab, defensivePrefab;
    public Transform offensiveContainer, defensiveContainer;
    public float xOffset;

    private PlayerAttacks playerAttacks;

    private void Start() {
        playerAttacks = FindObjectOfType<PlayerAttacks>();

        //if (playerAttacks != null) {
        //    playerAttacks.OnHealthUpdate += UpdateUI;
        //    UpdateUI(playerAttacks.GetDefensiveBombCount);
        //}
    }

    // UPDATES UI ON EVENT INVOKE
    private void UpdateUI(int currentHealth) {
        //// IF THERE ARE ANY PREFABS LEFT OVER FROM RUNTIME, WILL REMOVE 
        //foreach (Transform t in healthContainer) {
        //    Destroy(t.gameObject);
        //}

        //// OFFSETS PREFABS ON X-AXIS BY 75
        //for (int i = 0; i < currentHealth; i++) {
        //    GameObject icon = Instantiate(healthImagePrefab, healthContainer);
        //    RectTransform rect = icon.GetComponent<RectTransform>();

        //    rect.anchoredPosition = new Vector2(i * xOffset, 0);
        //}
    }

    // OFFLOADS UI FROM EVENT
    private void OnDestroy() {
        //if (playerAttacks != null) {
        //    eventSystem.OnOffensiveBulletAttack -= UpdateUI;
        //}
    }
}
