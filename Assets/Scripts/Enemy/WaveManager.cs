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
    List<GameObject> activeList = new List<GameObject>();
    bool shouldSpawn = true;
    int waveIndex;
    float currentTime;
    void Start()
    {
        InitWaveContainer();
        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        DoTime();
        SpawnEnemy();
    }
   
    void SpawnEnemy()
    {
        if(shouldSpawn)
        {
            WaveContainter enemyToSpawn = null;
            //get the wave we want to spawn
            if (wave.Count > 0)
            {
                enemyToSpawn = wave[waveIndex];
            }
            else
            {
                return;
            }

            //now that we have the next wave to spawn it

            GameObject obj = Instantiate(enemyToSpawn.enemy, enemyToSpawn.pos.spawnPosition, Quaternion.identity);
            //add it to the active list
            activeList.Add(obj);
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

    void InitWaveContainer()
    {
        foreach (WaveContainter waveContainter in wave)
        {
            if(waveContainter != null && waveContainter.isCustomMoveData)
            {
                Debug.Log("Assigning New Pos Data to " + waveContainter.enemy.gameObject.name);
                waveContainter.enemy.GetComponent<BaseEnemy>().SetPositionContainer(waveContainter.pos);
            }
        }
    }
    public void CleanEnemy()
    {
        activeList.Clear();
    }
}
