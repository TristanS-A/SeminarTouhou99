using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sequencer : MonoBehaviour
{
    [SerializeField] List<AttackData> attacks;
    [SerializeField] bool shouldLoop;
    private int currentAttackIndex = 0;
    private List<GameObject> activeEmmiter;

    float timeTillNextAttack;
    private void Update()
    {

    }
    private void FixedUpdate()
    {

    }
    // Start is called before the first frame update
    private AttackData GetNextAttackEmmiter()
    {
        //we tried to move past the attacks that we have in the list
        if (currentAttackIndex > attacks.Count)
        {
            //just retrun a null
            if (shouldLoop)
            {
                currentAttackIndex = 0;
            }
            return null;

        }

        AttackData returnObj = attacks[currentAttackIndex];
        currentAttackIndex++;

        return returnObj;
    }
    private void SpawnEmmiter()
    {
        var nextAttackToSpawn = GetNextAttackEmmiter();

        if (activeEmmiter == null)
        {
            //no new attacks to spawn
            return;
        }

        Vector2 spawnPos = Vector2.zero;

        //attack centerd on the enemy 
        if (nextAttackToSpawn.IsCenterd())
        {
            spawnPos = this.transform.position;

        }
        else
        {
            spawnPos = (Vector2)this.transform.position + nextAttackToSpawn.GetDirction();
        }

        var obj = Instantiate(nextAttackToSpawn.GetEmitter(), spawnPos, Quaternion.identity);
     
        //before returning we need to clean the list for dead referecnes
        CleanList();

        return;
    }
    private void CleanList()
    {
        List<int> deadList = new List<int>();
        int index = 0;

        foreach (var emitter in activeEmmiter)
        {
            if(emitter.Equals(null))
            {
                deadList.Add(index);
            }
            index++;
        }

        foreach (var dead in deadList)
        {
            activeEmmiter.RemoveAt(dead);
        }
    }
}
