using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageBoxPopup : MonoBehaviour
{
   // TMPro.TextMeshPro m_message;
    TMPro.TextMeshProUGUI m_message;
    private void Awake()
    {
        m_message = transform.Find("Message").GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void SetText(string message)
    {
        m_message.text = message;
    }

    public void ToggleOkButton(bool state)
    {
        transform.Find("OK").gameObject.SetActive(state);
    }

    public void DestroyMe()
    {
        Destroy(this.gameObject);
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
