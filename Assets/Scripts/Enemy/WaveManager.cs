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
    float currentTime;
    bool shouldSpawn = true;
    int waveIndex = 0;

    void Start()
    {
        SpawnEnemy();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DoTime();
        SpawnEnemy();
        CheckActives();
    }
   
    void SpawnEnemy()
    {
        if(shouldSpawn)
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

            waveIndex++;
        }
        
    }
    void DoTime()
    {
        shouldSpawn = currentTime <= 0;
        currentTime -= Time.deltaTime;
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
    public void CleanEnemy() => activeList.Clear();
}
