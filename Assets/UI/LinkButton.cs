using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkButton : MonoBehaviour
{
    [SerializeField] string m_url;
    public void OpenUrl()
    {
        if(!string.IsNullOrEmpty(m_url))
            Application.OpenURL(m_url);
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
