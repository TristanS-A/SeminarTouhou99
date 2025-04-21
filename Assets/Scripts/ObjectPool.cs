using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
    public static Dictionary<string, Component> poolLookUp = new Dictionary<string, Component>();
    public static Dictionary<string, Queue<Component>> poolDic = new Dictionary<string, Queue<Component>>();

    public static void EnqeueObject<T>(T item , string name) where T : Component
    {
        if(item.gameObject.activeSelf)
        {
            return;
        }

        item.transform.position = Vector3.zero;
        poolDic[name].Enqueue(item);

        item.gameObject.SetActive(false);
    }
    public static T DequeueObject<T>(string key) where T : Component  
    {
        if (poolDic[key].TryDequeue(out var item))
        {
            return (T)item;
        }

        return (T)EnqeueNewInstance(poolLookUp[key], key);
    }
    public static T EnqeueNewInstance<T>(T item, string key) where T : Component
    {
        T pooledInstance = Object.Instantiate(item);
        pooledInstance.gameObject.SetActive(false);
        pooledInstance.transform.position = Vector3.zero; 
        poolDic[key].Enqueue(pooledInstance);

        return pooledInstance;
    }
    public static void SetUpPool<T>(T pooledSizePrefab, int ammount, string dicEntry) where T : Component
    {
        poolDic.Add(dicEntry, new Queue<Component>());
        poolLookUp.Add(dicEntry, pooledSizePrefab);

        for (int i = 0; i < ammount; i++)
        {
            T pooledInstance = Object.Instantiate(pooledSizePrefab);
            pooledInstance.gameObject.SetActive(false);
            pooledInstance.transform.position = Vector3.zero;
            poolDic[dicEntry].Enqueue(pooledInstance);
        }
    }
}
