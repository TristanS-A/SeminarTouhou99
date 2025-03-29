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
//TODO: test this code :3
//TODO: add a way to clear active enemys
//TODO: try to make a stage with all this stuff
public class WaveManager : MonoBehaviour
{
    [SerializeField] List<WaveContainter> wave = new();    // Start is called before the first frame update
    List<Tuple<GameObject, BaseEnemy>> activeList = new List<Tuple<GameObject, BaseEnemy>>(); 
    bool shouldSpawn = true;
    int waveIndex = 0;
    float currentTime;


    void Start()
    {
       
        //InitWaveContainer();
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
                EventSystem.SendPlayerResultData(serverHandler.ResultContext.PLAYER_WON);
                return;
            }

            //now that we have the next wave to spawn it

            GameObject obj = Instantiate(enemyToSpawn.enemy, enemyToSpawn.pos.spawnPosition, Quaternion.identity);
            BaseEnemy scr = obj.GetComponent<BaseEnemy>();
            scr.SetPositionContainer(enemyToSpawn.pos);

            //add it to the active list
            activeList.Add(new(obj, scr));

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
        if(currentTime <= 0)
        {
            shouldSpawn = true;
        }
        else
        {
            shouldSpawn = false;
        }
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
    //void InitWaveContainer()
    //{
    //    foreach (WaveContainter waveContainter in wave)
    //    {
    //        if(waveContainter != null && waveContainter.isCustomMoveData)
    //        {
    //            Debug.Log("Assigning New Pos Data to " + waveContainter.enemy.gameObject.name);
    //            waveContainter.enemy.GetComponent<BaseEnemy>().SetPositionContainer(waveContainter.pos);
    //        }
    //    }
    //}
    public void CleanEnemy()
    {
        activeList.Clear();
    }
}
