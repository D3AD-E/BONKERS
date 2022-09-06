using MLAPI;
using MLAPI.Messaging;
using MLAPI.Prototyping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    private static SpawnManager _instance;

    public static SpawnManager Instance { get { return _instance; } }
    List<Vector3> m_locations = new List<Vector3>();
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            m_locations.Add(transform.GetChild(i).position);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void TeleportToSpawnServerRpc(ulong id)
    {
        var player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject;
        if (m_locations.Count > 0)
        {
            int pos = Random.Range(0, m_locations.Count);
            var location = new Vector3(m_locations[pos].x, m_locations[pos].y, 0f);
            player.transform.position = location;
            //player.GetComponent<NetworkTransform>().ForceSend(location);
            TeleportToSpawnClientRpc(id, location);
            m_locations.RemoveAt(pos);
        }
        else
        {
            player.transform.position = Vector3.zero;
            //player.GetComponent<NetworkTransform>().ForceSend(Vector3.zero);
            TeleportToSpawnClientRpc(id, Vector3.zero);
        }
    }

    [ClientRpc]
    private void TeleportToSpawnClientRpc(ulong id, Vector3 pos)
    {
        GameObject player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject;
        player.transform.position = pos;
    }

    public void ServerTeleport(GameObject player)
    {
        if (m_locations.Count > 0)
        {
            int pos = Random.Range(0, m_locations.Count);
            var location = new Vector3(m_locations[pos].x, m_locations[pos].y, 0f);
            player.transform.position = location;
            player.GetComponent<NetworkTransform>().Teleport(location, Quaternion.identity);
            m_locations.RemoveAt(pos);
        }
        else
        {
            player.transform.position = Vector3.zero;
            //player.GetComponent<NetworkTransform>().ForceSend(Vector3.zero);
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
