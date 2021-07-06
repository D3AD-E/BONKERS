using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthController : NetworkBehaviour, IHealth
{
    [SerializeField] float m_maxHealth = 100;
    public NetworkVariableFloat m_currentHealth = new NetworkVariableFloat(100);
    public NetworkVariableBool m_enabled = new NetworkVariableBool(true);
    private Animator m_animator;
    // Start is called before the first frame update
    public override void NetworkStart()
    {
        m_animator = GetComponent<Animator>();
        if (IsHost)
        {
            m_currentHealth.Value = m_maxHealth;
        }
        else if (IsClient)
            SetupHealthServerRpc();

        m_currentHealth.OnValueChanged += OnHealthValueChanged;
    }

    private void OnHealthValueChanged(float previousValue, float newValue)
    {
        if (!IsLocalPlayer)
            return;
        if (previousValue>newValue)
        {
            if (m_animator != null)
            {
                if(newValue == 0)
                {
                    m_animator.SetBool("Enabled", false);
                    m_animator.SetTrigger("Death");
                }
                else
                    m_animator.SetTrigger("Hurt");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float amount)
    {
        if (!m_enabled.Value)
            return;
        if(IsHost)
        {
            if (m_currentHealth.Value <= amount)
            {
                m_currentHealth.Value = 0f;
                enabled = false;
                GameplayManager.Instance.PlayerDied(OwnerClientId);
            }
            else
            {
                m_currentHealth.Value -= amount;
            }
        }
        else if(IsClient)
        {
            Debug.Log(amount);
            Debug.Log(m_currentHealth.Value);
            TakeDamageServerRpc(amount);
        }
    }

    public void Heal(float amount)
    {
        //if (!m_enabled.Value)
        //    return;
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
    public void SetEnabledServerRpc(bool value)
    {
        m_enabled.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(float amount)
    {
        if (m_currentHealth.Value <= amount)
        {
            m_currentHealth.Value = 0;
            m_enabled.Value = false;
            GameplayManager.Instance.PlayerDied(OwnerClientId);
        } 
        else
        {
            m_currentHealth.Value -= amount;
            Debug.Log(amount);
            Debug.Log(m_currentHealth.Value);
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
