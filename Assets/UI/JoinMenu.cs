using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinMenu : MonoBehaviour
{
    TMPro.TMP_InputField m_username;
    TMPro.TMP_InputField m_port;
    TMPro.TMP_InputField m_ip;
    [SerializeField] GameObject m_messageBox;
    private void Awake()
    {
        m_username = transform.Find("NickName").GetComponent<TMPro.TMP_InputField>();
        m_port = transform.Find("Port").GetComponent<TMPro.TMP_InputField>();
        m_ip = transform.Find("IP").GetComponent<TMPro.TMP_InputField>();
    }
    public void StartClient()
    {
        if (string.IsNullOrEmpty(m_port.text))
        {
            var box = Instantiate(m_messageBox, this.transform);
            var message = box.GetComponent<MessageBoxPopup>();
            message.SetText("Port is empty");
            return;
        }
        if (string.IsNullOrEmpty(m_ip.text))
        {
            var box = Instantiate(m_messageBox, this.transform);
            var message = box.GetComponent<MessageBoxPopup>();
            message.SetText("IP is empty");
            return;
        }
        if (string.IsNullOrEmpty(m_username.text))
        {
            var box = Instantiate(m_messageBox, this.transform);
            var message = box.GetComponent<MessageBoxPopup>();
            message.SetText("Username is empty");
            return;
        }

        GlobalData.IsClient = true;
        GlobalData.UserName = m_username.text;
        GlobalData.Port = m_port.text;
        GlobalData.IP = m_ip.text;

        SceneManager.LoadScene("Lobby");
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
