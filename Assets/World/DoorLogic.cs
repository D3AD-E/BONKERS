using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLogic : NetworkBehaviour
{
    [SerializeField] bool m_isOpen = false;
    [SerializeField] BoxCollider2D m_hitbox;
    [SerializeField] Sprite m_doorClosed;
    [SerializeField] Sprite m_doorOpened;
    [SerializeField] SpriteRenderer m_renderer;
    [SerializeField] AudioClip m_doorOpenSound;
    [SerializeField] AudioClip m_doorCloseSound;
    AudioSource m_source;
    // Start is called before the first frame update
    public override void NetworkStart()
    {
        base.NetworkStart();
        m_source = GetComponent<AudioSource>();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_isOpen)
            return;
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerController>();
            //var net = collision.GetComponent<Network>
            if (player.inputX > 0 && transform.position.x > collision.transform.position.x)
            {
                OpenDoor(false);
                if (IsHost)
                {
                    ChangeDoorStateClientRpc(true, false);
                }
                else if(IsClient)
                {
                    ChangeDoorStateServerRpc(true, false);
                }
            }
            else if (player.inputX < 0 && transform.position.x < collision.transform.position.x)
            {
                OpenDoor(true);
                if (IsHost)
                {
                    ChangeDoorStateClientRpc(true, true);
                }
                else if (IsClient)
                {
                    ChangeDoorStateServerRpc(true, true);
                }
            }
        }
    }

    void OpenDoor(bool toLeft)
    {
        m_source.PlayOneShot(m_doorOpenSound);
        m_isOpen = true;
        m_hitbox.enabled = false;
        m_renderer.sprite = m_doorOpened;
        m_renderer.flipX = toLeft;
    }

    void CloseDoor()
    {
        m_source.PlayOneShot(m_doorCloseSound);
        m_renderer.flipX = false;
        m_renderer.sprite = m_doorClosed;
        m_hitbox.enabled = true;
        m_isOpen = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!m_isOpen)
            return;
        if (collision.CompareTag("Player"))
        {
            if (IsHost)
            {
                ChangeDoorStateClientRpc(false, true);
            }
            else if (IsClient)
            {
                ChangeDoorStateServerRpc(false, true);
            }
        }
    }

    [ServerRpc(RequireOwnership =false)]
    void ChangeDoorStateServerRpc(bool state, bool isFromLeft)
    {
        ChangeDoorStateClientRpc(state, isFromLeft);
    }

    [ClientRpc]
    void ChangeDoorStateClientRpc(bool state, bool isFromLeft)
    {
        if (state == m_isOpen)
            return;
        if (state)
        {
            OpenDoor(isFromLeft);
        }   
        else
        {
            CloseDoor();
        }
    }
}
