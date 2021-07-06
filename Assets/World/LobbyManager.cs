using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] float m_timeToSwitch = 5f;
    [SerializeField] GameObject m_popup;

    private Dictionary<ulong, bool> m_playerStatus = new Dictionary<ulong, bool>();

    private static LobbyManager _instance;

    public static LobbyManager Instance { get { return _instance; } }

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
        base.NetworkStart();
        if (!IsHost)
        {
            enabled = false;
            return;
        }
        var players = NetworkManager.Singleton.ConnectedClients.Keys.ToList();
        foreach (var player in players)
        {
            m_playerStatus.Add(player, false);
        }
        NetworkManager.Singleton.OnClientConnectedCallback += AddPlayer;

    }

    void AddPlayer(ulong id)
    {
        if (!m_playerStatus.ContainsKey(id))
        {
            m_playerStatus.Add(id, false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeReadyStatusServerRpc(ulong id, bool status)
    {
        if (m_playerStatus.ContainsKey(id))
        {
            m_playerStatus[id] = status;
            if (!status)
            {
                CancelInvoke();
                m_timeToSwitch = 5f;
                return;
            }


            foreach (var player in m_playerStatus)
            {
                if (!player.Value)
                    return;
            }


            PrepareSceneSwitch();
        }
    }

    void PrepareSceneSwitch()
    {
        InvokeRepeating("ShowCountdownClientRpc", 0f, 1f);
    }

    [ClientRpc]
    void ShowCountdownClientRpc()
    {
        if (m_timeToSwitch == 0f && IsHost)
        {
            CancelInvoke();
            GameplayManager.Instance.LoadScene();
        }
        var popup = Instantiate(m_popup, GameObject.Find("Canvas").transform);
        popup.GetComponent<GenericPopupController>().Show("Starting in " + m_timeToSwitch, 0.9f);
        m_timeToSwitch--;
    }
}
