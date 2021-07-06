using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostMenu : MonoBehaviour
{
    TMPro.TMP_InputField m_username;
    TMPro.TMP_InputField m_port;
    [SerializeField] GameObject m_messageBox;
    private void Awake()
    {
        m_username = transform.Find("NickName").GetComponent<TMPro.TMP_InputField>();
        m_port = transform.Find("Port").GetComponent<TMPro.TMP_InputField>();
    }
    public void StartHost()
    {
        if(string.IsNullOrEmpty(m_port.text))
        {
            var box = Instantiate(m_messageBox, this.transform);
            var message = box.GetComponent<MessageBoxPopup>();
            message.SetText("Port is empty");
            return;
        }
        if (string.IsNullOrEmpty(m_username.text))
        {
            var box = Instantiate(m_messageBox, this.transform);
            var message = box.GetComponent<MessageBoxPopup>();
            message.SetText("Username is empty");
            return;
        }

        GlobalData.IsHost = true;
        GlobalData.UserName = m_username.text;
        GlobalData.Port = m_port.text;

        SceneManager.LoadScene("Lobby");
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
