using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGCManager : NetworkBehaviour
{
    [SerializeField] GameObject m_gameController;

    private static SpawnGCManager _instance;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
    }
    public override void NetworkStart()
    {
        if (!IsHost)
            Destroy(this.gameObject);
        base.NetworkStart();
        if(IsHost)
        {
            var controller = Instantiate(m_gameController);
            controller.GetComponent<NetworkObject>().Spawn();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
