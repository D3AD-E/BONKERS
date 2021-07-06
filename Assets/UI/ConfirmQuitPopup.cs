using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmQuitPopup : MonoBehaviour
{
    [SerializeField] GameObject m_popup;
    public void Quit()
    {
        var box = Instantiate(m_popup, this.transform);
        var message = box.GetComponent<MessageBoxPopup>();
        message.SetText("Quitting");
        message.ToggleOkButton(false);
        Application.Quit();
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
