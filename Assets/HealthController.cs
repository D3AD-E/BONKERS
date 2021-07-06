using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : NetworkBehaviour, IHealth
{
    [SerializeField] float m_maxHealth = 100;
    public NetworkVariableFloat m_currentHealth = new NetworkVariableFloat(100);
    public bool m_enabled = true;

    // Start is called before the first frame update
    public override void NetworkStart()
    {
        if (IsHost)
        {
            m_currentHealth.Value = m_maxHealth;
        }
        //else if (IsClient)
        //    SetupHealthServerRpc();
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(float amount)
    {
        if (!m_enabled)
            return;
        if (IsHost)
        {
            m_currentHealth.Value -= amount;
            if (m_currentHealth.Value <= 0f)
            {
                m_currentHealth.Value = 0f;
                enabled = false;
                Destroy(gameObject);
            }
        }
        else if (IsClient)
        {
            TakeDamageServerRpc(amount);
        }
    }

    public void Heal(float amount)
    {
        if (!m_enabled)
            return;
        if (IsHost)
        {
            m_currentHealth.Value += amount;
            if (m_currentHealth.Value > m_maxHealth)
                m_currentHealth.Value = m_maxHealth;
        }
        else if (IsClient)
        {
            HealServerRpc(amount);
        }
    }

    public void Kill()
    {
        TakeDamage(m_maxHealth);
    }

    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(float amount)
    {
        m_currentHealth.Value -= amount;
        if (m_currentHealth.Value <= 0)
        {
            m_currentHealth.Value = 0;
            m_enabled = false;
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetupHealthServerRpc()
    {
        m_currentHealth.Value = m_maxHealth;
    }

    [ServerRpc(RequireOwnership = false)]
    void HealServerRpc(float amount)
    {
        m_currentHealth.Value += amount;
        if (m_currentHealth.Value > m_maxHealth)
            m_currentHealth.Value = m_maxHealth;
    }
}
