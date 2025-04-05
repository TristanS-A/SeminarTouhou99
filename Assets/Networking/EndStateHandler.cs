using Unity.VisualScripting;
using UnityEngine;

public class EndStateHandler : MonoBehaviour
{
    //Serialized endstate objects for other players
    [SerializeField] private GameObject m_GraveStone;
    [SerializeField] private Material m_SpriteMat;
    [SerializeField] private Material m_HoloMat;

    [SerializeField] private GameObject m_Confetti;

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
