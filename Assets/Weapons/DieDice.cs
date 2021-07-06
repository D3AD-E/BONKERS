using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieDice : DroppableWeapon
{
    [SerializeField] float m_damage = 8f;
    [SerializeField] int m_destroyAfter = 3;
    [SerializeField] AudioClip m_badLuck;
    bool hit = false;

    Transform m_parent;
    private bool m_isActivated = false;
    int m_collisionAmount = 0;
    int m_rollNumber;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!transform.parent || !IsOwner)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            ShootEffectServerRpc();
            m_isActivated = true;
            m_isPickupable = false;
            m_parent = transform.parent.parent;
            m_rollNumber = Random.Range(1, 7);
            if(m_rollNumber == 1)
            {
                if(m_parent.tag == "Player")
                {
                    PlayBadLuckEffectServerRpc();
                    var health = m_parent.GetComponent<IHealth>();
                    health.TakeDamage(m_damage * 5);
                    DropWeapon();
                    DestroyServerRpc();
                }
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner || hit)
            return;
        if (!m_isActivated || !m_isDropped.Value)
            return;

        m_collisionAmount++;
        if (collision.transform.tag == "Player")
        {
            var network = collision.transform.GetComponent<NetworkObject>();
            if (network.OwnerClientId == OwnerClientId)
                 return;
            var health = collision.gameObject.GetComponent<IHealth>();
            health.TakeDamage(m_rollNumber * m_damage);
            DestroyServerRpc();
            hit = true;
            return;
        }

        if (m_collisionAmount > m_destroyAfter)
        {
            DestroyServerRpc();
            return;
        }
    }

    [ServerRpc]
    private void PlayBadLuckEffectServerRpc()
    {
        PlayBadLuckEffectClientRpc();
    }

    [ClientRpc]
    private void PlayBadLuckEffectClientRpc()
    {
        AudioSource.PlayClipAtPoint(m_badLuck, transform.position, 10); ;
    }
}
