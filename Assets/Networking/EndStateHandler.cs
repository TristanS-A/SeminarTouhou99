using UnityEngine;

public class EndStateHandler : MonoBehaviour
{
    //Serialized endstate objects for other players
    [SerializeField] private GameObject m_GraveStone;
    [SerializeField] private Material m_GraveMat;
    [SerializeField] private Material m_HoloGraveMat;

    private void OnEnable()
    {
        EventSystem.OnPlayerDie += HandlePlayerDies;
    }

    private void OnDisable()
    {
        EventSystem.OnPlayerDie -= HandlePlayerDies;
    }


    public void HandlePlayerDies(bool isOwningPlayer, Vector3 deathPos)
    {
        Material graveMat;
        if (isOwningPlayer)
        {
            graveMat = m_GraveMat;
        }
        else 
        {
            graveMat = m_HoloGraveMat;
        }

        //The z = 1 makes the grave show up behin the bullets
        GameObject grave = Instantiate(m_GraveStone, deathPos, Quaternion.identity);
        grave.GetComponentInChildren<Renderer>().material = graveMat;
    }
}
