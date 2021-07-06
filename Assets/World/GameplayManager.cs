using MLAPI;
using MLAPI.Messaging;
using MLAPI.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameplayManager : NetworkBehaviour
{
    private List<string> m_scenes = new List<string>();
    private Dictionary<PlayerInfo, int> m_score = new Dictionary<PlayerInfo, int>();

    [SerializeField] float m_timeTillNextScene = 10f;
    [SerializeField] GameObject m_popup;
    private List<ulong> m_alivePlayers = new List<ulong>();
    bool m_sceneSwitching = false;
    public SceneSwitchProgress m_progress;

    string m_currentSceneName;

    private static GameplayManager _instance;

    public static GameplayManager Instance { get { return _instance; } }

    //score, scene switching, player spawning
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
        Random.InitState((int)System.DateTime.Now.Ticks);
        m_scenes.Add("ShadowZXC");
        m_scenes.Add("SniperTeleport");
        m_scenes.Add("Pistols");
        m_scenes.Add("TestYourLuck");
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        m_currentSceneName = currentScene.name;
    }


    private void OnSceneSwitched()
    {
        m_sceneSwitching = false;
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        m_currentSceneName = currentScene.name;
    }

    [ServerRpc]
    public void SetPlayerNameServerRpc(string name, ulong id)
    {
        foreach (var item in m_score.Keys)
        {
            if (item.Id == id)
                item.Name = name;
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
        m_alivePlayers = NetworkManager.Singleton.ConnectedClients.Keys.ToList();
        foreach (var player in m_alivePlayers)
        {
            m_score.Add(new PlayerInfo(player, NetworkManager.Singleton.ConnectedClients[player].PlayerObject.GetComponent<PlayerController>().UserName), 0);
        }
        NetworkManager.Singleton.OnClientConnectedCallback += AddPlayer;
        NetworkSceneManager.OnSceneSwitched += OnSceneSwitched;

    }

    void AddPlayer(ulong id)
    {
        if(!m_score.ContainsKey(new PlayerInfo { Id = id }))
        {
            m_alivePlayers.Add(id);
            m_score.Add(new PlayerInfo(id, NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerController>().UserName), 0);
        }
    }
    public void PlayerDied(ulong id)
    {
        if (IsHost)
        {
            if(m_alivePlayers.Contains(id))
            {
                m_alivePlayers.Remove(id);
                NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerController>().PlayerDiedClientRpc(id);
                if (m_alivePlayers.Count <= 1)
                {
                    if (m_alivePlayers.Count == 1)
                        m_score[new PlayerInfo { Id = m_alivePlayers[0] }]++;
                    StartCoroutine(PrepareSceneSwitch());
                    ShowWinPopupClientRpc(NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerController>().UserName);
                }
            }
        }
    }

    [ClientRpc]
    void ShowWinPopupClientRpc(string name)
    {
        var popup = Instantiate(m_popup, GameObject.Find("Canvas").transform);
        popup.GetComponent<GenericPopupController>().Show("Ez for " + name, m_timeTillNextScene);
    }

    private IEnumerator PrepareSceneSwitch()
    {
        yield return new WaitForSeconds(m_timeTillNextScene);
        LoadScene();
    }

    public void LoadScene()
    {
        if(IsHost &&!m_sceneSwitching)
        {
            m_sceneSwitching = true;
            m_alivePlayers = NetworkManager.Singleton.ConnectedClients.Keys.ToList();

            int sceneNum = Random.Range(0, m_scenes.Count);
            while(m_scenes[sceneNum] == m_currentSceneName)
                sceneNum = Random.Range(0, m_scenes.Count);
            m_progress = NetworkSceneManager.SwitchScene(m_scenes[sceneNum]);
        }
    }

    public Dictionary<PlayerInfo,int> GetScores()
    {
        return m_score;
    }

    [ServerRpc(RequireOwnership =false)]
    public void GetScoresServerRpc()
    {
        foreach (var player in m_score)
        {
            GetScoresClientRpc(player.Key.Name, player.Value);
        }
    }

    [ClientRpc]
    void GetScoresClientRpc(string name, int score)
    {
        var table = GameObject.FindGameObjectWithTag("ScoreTable");
        if(table)
            table.GetComponent<ScoreTableManager>().SetupTable(name, score);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class PlayerInfo
    {
        public ulong Id;
        public string Name;

        public override int GetHashCode() => Id.GetHashCode();
        public override bool Equals(object other) => (other as PlayerInfo)?.Id == Id;
        public PlayerInfo()
        {
            Name = string.Empty;
            Id = 0;
        }
        public PlayerInfo(ulong id, string name)
        {
            Name = name;
            Id = id;
        }
    }

        

}
