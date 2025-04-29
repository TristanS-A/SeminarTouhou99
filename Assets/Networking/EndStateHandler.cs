using Unity.VisualScripting;
using UnityEngine;

public class EndStateHandler : MonoBehaviour
{
    //Serialized endstate objects for other players
    [SerializeField] private GameObject m_GraveStone;
    [SerializeField] private Material m_SpriteMat;
    [SerializeField] private Material m_HoloMat;

    [SerializeField] private GameObject m_Confetti;

    [SerializeField] GameObject m_DeathAni;
    [SerializeField] private float mDeathAniScale = 1;
    [SerializeField] private Sprite mDeathAniSprite;

    private void OnEnable()
    {
        EventSystem.OnPlayerDie += HandlePlayerDies;
        EventSystem.OnPlayerWin += HandlePlayerWins;
    }

    private void OnDisable()
    {
        EventSystem.OnPlayerDie -= HandlePlayerDies;
        EventSystem.OnPlayerWin -= HandlePlayerWins;
    }

    public void HandlePlayerDies(bool isOwningPlayer, Vector3 deathPos)
    {
        Material graveMat;
        if (isOwningPlayer)
        {
            graveMat = m_SpriteMat;
        }
        else 
        {
            graveMat = m_HoloMat;
        }

        GameObject grave = Instantiate(m_GraveStone, deathPos, Quaternion.identity);
        grave.GetComponentInChildren<Renderer>().material = graveMat;

        GameObject deathAni = Instantiate(m_DeathAni, deathPos, Quaternion.identity);

        deathAni.transform.localScale = new Vector3(mDeathAniScale, mDeathAniScale, mDeathAniScale);

        if (mDeathAniSprite != null)
        {
            SpriteRenderer[] sRenderers = deathAni.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sRenderer in sRenderers)
            {
                sRenderer.sprite = mDeathAniSprite;
            }
        }

        deathAni.transform.eulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0, 180));

        Renderer[] sMaterials = deathAni.GetComponentsInChildren<Renderer>();
        foreach (Renderer sMaterial in sMaterials)
        {
            sMaterial.material = graveMat;
        }
    }

    public void HandlePlayerWins(bool isOwningPlayer, Vector3 winPos)
    {
        Material confettiMat;
        if (isOwningPlayer)
        {
            confettiMat = m_SpriteMat;
        }
        else
        {
            confettiMat = m_HoloMat;
            confettiMat.SetFloat("ColorTransparency", -0.8f);
        }

        GameObject confettiOBJ = Instantiate(m_Confetti, winPos, Quaternion.identity);
        confettiOBJ.GetComponentInChildren<Renderer>().material = confettiMat;
    }
}
