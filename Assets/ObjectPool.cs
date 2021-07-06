using MLAPI;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool SharedInstance;
    public List<GameObject> m_pooledObjects;
    [SerializeField] GameObject m_objToPool;
    [SerializeField] int m_amount;

    private void Awake()
    {
        SharedInstance = this;
        //NetworkSpawnManager.RegisterSpawnHandler(NetworkSpawnManager.GetPrefabHashFromGenerator("BulletTrail"), (position, rotation) =>
        //{
        //    return GetObject().GetComponent<NetworkObject>();
        //});

        //NetworkSpawnManager.RegisterDestroyHandler(NetworkSpawnManager.GetPrefabHashFromGenerator("BulletTrail"), (networkObject) =>
        //{
        //    networkObject.gameObject.SetActive(false);
        //});
    }

    private void Start()
    {
        m_pooledObjects = new List<GameObject>();
        GameObject tmp;

        for (int i = 0; i < m_amount; i++)
        {
            tmp = Instantiate(m_objToPool);
            tmp.SetActive(false);
            //tmp.GetComponent<NetworkObject>().Spawn();
            m_pooledObjects.Add(tmp);
        }
    }

    public GameObject GetObject()
    {
        for (int i = 0; i < m_amount; i++)
        {
            if(!m_pooledObjects[i].activeInHierarchy)
            {
                return m_pooledObjects[i];
            }
        }
        return null;
    }
}
