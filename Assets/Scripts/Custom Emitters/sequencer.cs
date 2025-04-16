using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class Sequencer : MonoBehaviour
{
    [SerializeField] List<AttackData> attacks;
    [SerializeField] bool shouldLoop;
    public int currentAttackIndex = 0;
    private List<GameObject> activeEmmiter = new();

    List<float> timeTillNextAttack = new();
    private Nullable<Vector2> overridenSpawnPos = null;

    [SerializeField] private AudioClip bulletSpawnSFX;

    bool isCustomTime;
    private void Start()
    {
        //SpawnEmmiter();
    }
    private void Update()
    {

    }
    private void FixedUpdate()
    {
        if(CheckIfShouldSpawn())
        {
            SpawnEmmiter();
        }
        UpdateLifeTimes();

    }
    // Start is called before the first frame update
    private AttackData GetNextAttackEmmiter()
    {
        //we tried to move past the attacks that we have in the list
        if (currentAttackIndex >= attacks.Count)
        {
            //just retrun a null
            if (shouldLoop && attacks.Count != 0)
            {
                currentAttackIndex = 0;
                return attacks[currentAttackIndex];

            }
            return null;

        }

        AttackData returnObj = attacks[currentAttackIndex];
        currentAttackIndex++;

        return returnObj;
    }

    public void SpawnEmmiter()
    {
        var nextAttackToSpawn = GetNextAttackEmmiter();

        if (nextAttackToSpawn == null)
        {
            //no new attacks to spawn
            return;
        }

        //check to see if we are spawing with a custom time
        isCustomTime = nextAttackToSpawn.IsCustomTime();

        //chace the time 
        if (isCustomTime)
        {
            timeTillNextAttack.Add(nextAttackToSpawn.GetCustomLifeTime());

            //sort for faster accsesing later
            timeTillNextAttack.Sort();
        }

       
        Vector2 spawnPos = Vector2.zero;

        if (overridenSpawnPos == null) {
            //attack centerd on the enemy 
            if (nextAttackToSpawn.IsCenterd())
            {
                spawnPos = this.transform.position;

            }
            else
            {
                spawnPos = (Vector2)this.transform.position + nextAttackToSpawn.GetDirction();
            }
        } else {
            spawnPos = overridenSpawnPos.Value;
        }

        bool isAttached = nextAttackToSpawn.ShouldAttach();

        GameObject obj = null;
        //attaches to parent
        if (isAttached)
        {
            obj = Instantiate(nextAttackToSpawn.GetEmitter(), spawnPos, Quaternion.identity, gameObject.transform);
        }
        else
        {
            obj = Instantiate(nextAttackToSpawn.GetEmitter(), spawnPos, Quaternion.identity);
        }
        SoundManager.Instance.PlaySFXClip(bulletSpawnSFX, transform, 0.25f, 1.1f);

        activeEmmiter.Add(obj);
        //before returning we need to clean the list for dead referecnes
        CleanList();

        return;
    }
    private void CleanList()
    {

        activeEmmiter.RemoveAll(x => x == null);
    }

    //determains if the emmiter is using custom time or  default time
    private bool CheckIfShouldSpawn()
    {
        if (activeEmmiter.Count == 0)
        {
            //nothing is active run the we dont have to do any other checking
            return true;
        }

        if(isCustomTime)
        {
            //check all active timers
            bool isZero = false;

            foreach(var time in timeTillNextAttack)
            {
                //we found a time that is less then zero so nothing else is releent and we should spawn somthing
                if(time <= 0)
                {
                    isZero = true;
                    break;
                }
            }

            //if zero is true then we want to clean out the list to see if just incase somthing else was zero
            if(isZero)
            {
                CleanTimeList();
            }

            //return to spawn somthing if we found a zero
            return isZero;
            
        }

        //if there was no custom time we need to check to see if our newest emmiter is alive
        if (activeEmmiter[activeEmmiter.Count - 1]  == null)
        {
            return true;
        }

        return false;
       
    }
    private void UpdateLifeTimes()
    {   
        //skip if we have no timers to manage
        if (timeTillNextAttack.Count < 0) { return; }

        for(int i= 0; i < timeTillNextAttack.Count; i++)
        {
            timeTillNextAttack[i] -= Time.deltaTime;
        }
    }
    private void CleanTimeList()
    {
        //removes stuff from the list if it is <=0 should be fast becasue we sorted the list
        timeTillNextAttack.RemoveAll(t => t <= 0);

    }

    public void CleanSequencer()
    {
        foreach (var emitter in activeEmmiter)
        {
            Destroy(emitter);
        }
    }
    public void ClearAttackList()
    {
        attacks.Clear();
    }

    private void OnDestroy()
    {
        CleanSequencer();
    }

    public void SetSequeceList(List<AttackData> sequeceList)
    {
        this.attacks = sequeceList;
    }

    public List<AttackData> GetAttacks => attacks;
    public int GetAttackLength => attacks.Count;
    public void SetSpawnPos(Nullable<Vector2> newSpawnPos) => overridenSpawnPos = newSpawnPos;
    public bool GetIfAtEnd()
    {
        if(activeEmmiter.Count <= 0)
        {
            return true;
        }
        return false;
    }
}
