using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyBeam : NetworkBehaviour
{
    [SerializeField] Color m_notReadyColor;
    [SerializeField] Color m_readyColor;
    SpriteRenderer m_sprite;

    Rigidbody2D m_body;
    Light m_light;
    private void Awake()
    {
        m_sprite = GetComponent<SpriteRenderer>();
        m_light = transform.Find("LightSource").GetComponent<Light>();
        m_sprite.color = m_notReadyColor;
        m_light.color = m_notReadyColor;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !m_body)
        {
            m_body = collision.attachedRigidbody;
            m_sprite.color = m_readyColor;
            m_light.color = m_readyColor;
            if (IsOwner)
                LobbyManager.Instance.ChangeReadyStatusServerRpc(collision.GetComponent<PlayerController>().OwnerClientId, true);
           // collision.GetComponent<PlayerController>()
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && m_body && m_body == collision.attachedRigidbody)
        {
            m_body = null;
            m_sprite.color = m_notReadyColor;
            m_light.color = m_notReadyColor;

            if (IsOwner)
                LobbyManager.Instance.ChangeReadyStatusServerRpc(collision.GetComponent<PlayerController>().OwnerClientId, false);
            // collision.GetComponent<PlayerController>()
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
