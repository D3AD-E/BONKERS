using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTableManager : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject m_tableEntry;
    bool m_isWaitingForData = false;
    void Start()
    {
        
    }

    public void UpdateTable()
    {
        if (IsHost)
        {
            SetupTable(GameplayManager.Instance.GetScores());
        }
        else if (IsClient)
        {
            m_isWaitingForData = true;
            GameplayManager.Instance.GetScoresServerRpc();
        }
    }
    void SetupTable(Dictionary<GameplayManager.PlayerInfo, int> scores)
    {
        var tableBody = transform.Find("TableBody");
        foreach (var score in scores)
        {
            var entry = Instantiate(m_tableEntry, tableBody);
            entry.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = score.Key.Name;
            entry.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = score.Value.ToString();
        }
    }

    public void SetupTable(string name, int score)
    {
        if (!m_isWaitingForData)
            return;
        var tableBody = transform.Find("TableBody");

        var entry = Instantiate(m_tableEntry, tableBody);
        entry.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = name;
        entry.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = score.ToString();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
