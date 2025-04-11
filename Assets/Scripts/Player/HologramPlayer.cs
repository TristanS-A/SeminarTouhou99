using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class HologramPlayer : MonoBehaviour
{
    [SerializeField] private Sequencer playerSequencer;
    [SerializeField] private GameObject m_OffensiveBombVFX;
    [SerializeField] private GameObject m_DefensiveBombVFX;
    private int attackIndex = 0;

    public void SpawnOffensiveBomb(Vector2 pos)
    {
        playerSequencer.SetSpawnPos(pos);
        playerSequencer.enabled = true;

        //Handle spawn offensive bomb vfx
        GameObject bombVFX = Instantiate(m_OffensiveBombVFX, pos, Quaternion.identity);
        bombVFX.GetComponent<VisualEffect>().SetBool("ShouldBeHologram", true);

        StartCoroutine(TurnOffSequencer(playerSequencer.GetAttacks[attackIndex].GetCustomLifeTime()));
    }

    // Waits for the lifetime of the AttackData, disables the sequencer, cleans and increments to next index
    private IEnumerator TurnOffSequencer(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        playerSequencer.enabled = false;
        playerSequencer.CleanSequencer();
        attackIndex++;
    }
}
