using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
struct DropPrefabRelation
{
     public DropType type;
     public GameObject prefab;
}

public class DropManager : MonoBehaviour
{
    //used to populate the realtion list
    [SerializeField] List<DropPrefabRelation> list;

    Dictionary<DropType, GameObject> relationMap = new Dictionary<DropType, GameObject>();
    [SerializeField] float spawnRad = 5.0f;

    //init random gen
    System.Random rand = new();

    private void Awake()
    {
        eventSystem.dropEvent += SpawnDrops;
    }
    void Start()
    { 
       
        //build relation table
        BuildRelation(list);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnDrops(DropTypes drops)
    {
        Debug.Log("Called event");

        var pos = drops.GetLocation();

        foreach (var drop in drops.dropList)
        {
            //get type
            var type = drop.dropType;

            Vector2 randOffset = CreateSpawnPosition(spawnRad);

            if (!relationMap.ContainsKey(type))
            {
                Debug.LogWarning("Drop Manager Does not contain a prefab for the " + type);
                return;
            }

            GameObject prefab = relationMap[type];

            StartCoroutine(SpawnObjects(pos, prefab, drop.ammount));
        }

    }
    IEnumerator SpawnObjects(Vector3 pos, GameObject prefab, int ammount)
    {
        WaitForSeconds wait = new WaitForSeconds(0.001f);


        for (int i = 0; i < ammount; i++)
        {
            //create offsets
            var offset = CreateSpawnPosition(spawnRad);

            //add offsets
            pos += new Vector3(offset.x, offset.y, 0);

            Instantiate(prefab, pos, Quaternion.identity);
            yield return wait;
        }

        yield return null;

    }
    //build the map
    void BuildRelation(List<DropPrefabRelation> rel)
    {
        foreach (var relation in rel)
        {
            relationMap.Add(relation.type, relation.prefab);
        }

    }

    Vector2 CreateSpawnPosition(float rad)
    {
        float x =  (float)rand.NextDouble() * rand.Next(-(int)rad, (int)rad);
        float y = (float)rand.NextDouble() * rand.Next(-(int)rad, (int)rad);
      
        return new Vector2(x, y);
    }
    private void OnDestroy()
    {
        eventSystem.dropEvent -= SpawnDrops;
    }
}
