using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class WaveContainter
{
    public GameObject enemy;
    public bool isCustomTime;
    public bool isCustomMoveData;
    public float timeTillSpawn;

    public PositionContainer pos;

}

public class WaveManager : MonoBehaviour
{
    [SerializeField] List<WaveContainter> wave = new();    
    [SerializeField] GameObject midBoss;
    [SerializeField] GameObject finalBoss;

    List<Tuple<GameObject, BaseEnemy>> activeList = new();

    //will be null unless we are in a boss stage
    Tuple<GameObject, Sequencer> activeBoss;

    float currentTime;
    bool shouldSpawn = true;
    bool bossState = false;
    int waveIndex = 0;

    void Start()
    {
        SpawnEnemy();
        EventSystem.WaveStateChange += this.ChangeWaveState;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DoTimer();
        //SpawnEnemy();
        CheckActives();
    }

    #region NORAML_LOGIC
    void SpawnEnemy()
    {
        if(shouldSpawn && !bossState)
        {
            WaveContainter enemyToSpawn = null;
            //get the wave we want to spawn
            if (wave.Count > waveIndex)
            {
                Debug.Log("Wave Index" + waveIndex);
                enemyToSpawn = wave[waveIndex];
            }
            else
            {
                //Finishes the level and triggers the sending of result data
                //EventSystem.SendPlayerResultData(serverHandler.ResultContext.PLAYER_WON);
                return;
            }

            //we need to see if we have a break inorder to spawn a boss/midStage
            if(enemyToSpawn.enemy == null)
            {
                bossState = true;
                Debug.Log("InBossMode");
                return;

            }
            //now that we have the next wave to spawn it

            GameObject obj = Instantiate(enemyToSpawn.enemy, enemyToSpawn.pos.spawnPosition, Quaternion.identity);
            BaseEnemy mov = obj.GetComponent<BaseEnemy>();
            mov.SetPositionContainer(enemyToSpawn.pos);

            //add it to the active list
            activeList.Add(new(obj, mov));

            Debug.Log("spawned enemy");

            if(enemyToSpawn.isCustomTime)
            {
                currentTime = enemyToSpawn.timeTillSpawn;
            }
            else
            {
                currentTime = 0;
            }

            StartCoroutine(Co_WaitForNextSpawn());

            waveIndex++;
        }
        
    }
    #endregion NORAML_LOGIC

    #region BOSS_LOGIC
    void ChangeWaveState(bool state)
    {
        bossState = state;

        if(!state)
        {
            activeBoss = null;
        }
        else
        {
            StartCoroutine(Co_WaitTillActivesEmpty());
        }
    }
    void SpawnBoss()
    {
        
    }
    
    IEnumerator Co_WaitTillActivesEmpty()
    {
        yield return new WaitUntil(CheckActivesForEmpty);
        SpawnBoss();
    }
    bool GetBossState() => bossState;
    Tuple<GameObject, Sequencer> GetBossObject => activeBoss;
    #endregion BOSS_LOGIC
    void DoTimer()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            return;
        }

        currentTime = 0;
    }

    //think about chaning this to a event using the event system :) event would be sent from the base enemy script
    void CheckActives()
    {
        foreach(var active in activeList)
        {
            if(active.Item2.ShouldDestroy())
            {
                Debug.Log("Destoryed enemy :(");
                Destroy(active.Item1);
            }
        }

        activeList.RemoveAll(x => x.Item1 == null);    
    }
    bool CheckActivesForEmpty()
    {
        if (activeList.Count == 0)
        {
            return true;
        }
        return false;
    }

    private bool ReadyToSpawnNextEnemy()
    {
        return currentTime <= 0;
    }

    private IEnumerator Co_WaitForNextSpawn()
    {
        yield return new WaitUntil(ReadyToSpawnNextEnemy);
        SpawnEnemy();
    }

    public void CleanEnemy() => activeList.Clear();

}
