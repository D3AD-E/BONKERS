using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupLogic : NetworkBehaviour
{
    [SerializeField] WeaponType m_weapon;
    [SerializeField] bool m_infinite = false;
    [SerializeField] bool m_refreshable = false;
    [SerializeField] float m_refreshTime = 5f;
    //refresh amount?
    float m_time = 100f;
    NetworkVariableBool m_isWeaponTaken = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.ServerOnly, ReadPermission = NetworkVariablePermission.Everyone }, false);
   
    SpriteRenderer m_renderer;
    private void Awake()
    {
        m_renderer = transform.Find("Weapon").GetComponent<SpriteRenderer>();
        m_isWeaponTaken.OnValueChanged += OnWeaponTaken;
    }

    private void OnWeaponTaken(bool previousValue, bool newValue)
    {
        if(IsClient)
        {
            m_renderer.enabled = !newValue;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(m_infinite)
        {
            var light = transform.Find("Weapon").GetComponent<Light>();
            light.color = Color.red;
        }
        else if(m_refreshable)
        {
            var light = transform.Find("Weapon").GetComponent<Light>();
            light.color = Color.blue;

            if(IsServer)
            {
                m_renderer.enabled = false;
                m_isWeaponTaken.Value = true;
            }
            else if(IsClient)
            {
                m_renderer.enabled = !m_isWeaponTaken.Value;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(m_refreshable && m_isWeaponTaken.Value)
        {
            m_time += Time.deltaTime;
            if(m_time>m_refreshTime)
            {
                m_time = 0f;
                if (IsServer)
                {
                    m_isWeaponTaken.Value = false;
                    m_renderer.enabled = true;
                }
            }
        }
    }

    public WeaponType Pickup()
    {
        if (m_infinite)
            return m_weapon;
        else if (m_weapon == WeaponType.None || m_isWeaponTaken.Value)
            return WeaponType.None;
        else if(m_refreshable)
        {
            m_renderer.enabled = false;
            if (IsServer)
                m_isWeaponTaken.Value = true;
            else if (IsClient)
                ChangeWeponTakenServerRpc(true);
            return m_weapon;
        }
        //if (IsHost)
        //    Destroy(gameObject);
        //else if (IsClient)
            DestroyServerRpc();
        return m_weapon;
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeWeponTakenServerRpc(bool value)
    {
        m_isWeaponTaken.Value = value;
        m_renderer.enabled = !value;
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyServerRpc()
    {
        Destroy(gameObject);
    }
}
