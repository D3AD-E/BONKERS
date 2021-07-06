using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCoil : DroppableWeapon
{
    [SerializeField] float m_damage = 35f;
    [SerializeField] float m_coil0CooldownTime = 10f;
    [SerializeField] float m_coil1CooldownTime = 10f;
    [SerializeField] float m_coil2CooldownTime = 10f;
    [SerializeField] float m_coilRadius = 0.8947368f;
    [SerializeField] float m_coilDistance = 1.3f;
    [SerializeField] float m_coilShowTime = 1f;
    [SerializeField] float m_pauseDuration = 3f;
    [SerializeField] float m_coilDebuffStackTime = 8f;
    [SerializeField] GameObject m_coil;
    [SerializeField] GameObject m_pauseDisplay;
    float m_timeSinceCoil0;
    float m_timeSinceCoil1;
    float m_timeSinceCoil2;
    NetworkVariableBool m_isPaused = new NetworkVariableBool(false);
    NetworkVariableFloat m_timeSinceCoil0Other = new NetworkVariableFloat(10f);
    NetworkVariableFloat m_timeSinceCoil1Other = new NetworkVariableFloat(10f);
    NetworkVariableFloat m_timeSinceCoil2Other = new NetworkVariableFloat(10f);

    float m_timeSinceCoilHit =0f;
    float m_pauseTime;
    int hitCoils = 0;
    private new void Awake()
    {
        m_isPaused.OnValueChanged += OnFrozenChanged;
        OnDropped += new DroppableWeaponHandler(DropWeaponEventFired);
        OnPickedUp += new DroppableWeaponHandler(PickupWeaponEventFired);
        base.Awake();
        m_timeSinceCoil0 = m_coil0CooldownTime;
        m_timeSinceCoil1 = m_coil1CooldownTime;
        m_timeSinceCoil2 = m_coil2CooldownTime;
    }

    private void OnFrozenChanged(bool previousValue, bool newValue)
    {
        if(newValue)
        {
            PauseSetup();
            hitCoils = 0;
            Time.timeScale = 0;
            m_pauseTime = Time.realtimeSinceStartup;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_isPaused.Value && Time.realtimeSinceStartup-m_pauseTime>= m_pauseDuration)
        {
            if(IsHost)
                m_isPaused.Value = false;
            Time.timeScale = 1;
        }
        if (!IsOwner || m_isDropped.Value)
            return;

        if (hitCoils>0)
            m_timeSinceCoilHit += Time.deltaTime;
        m_timeSinceCoil0 += Time.deltaTime;
        m_timeSinceCoil1 += Time.deltaTime;
        m_timeSinceCoil2 += Time.deltaTime;


        if(m_timeSinceCoilHit> m_coilDebuffStackTime)
        {
            hitCoils = 0;
            m_timeSinceCoilHit = 0;
        }

        if (m_coil0CooldownTime < m_timeSinceCoil0 && Input.GetKeyDown("z"))
        {
            m_timeSinceCoil0 = 0f;
            var player = transform.parent.parent;
            var direction = player.GetComponent<PlayerController>().m_facingDirection;
            SendCoil(transform.position + new Vector3(direction * m_coilDistance, 0, 0));
        }
        else if (m_coil1CooldownTime < m_timeSinceCoil1 && Input.GetKeyDown("x"))
        {
            m_timeSinceCoil1 = 0f;
            var player = transform.parent.parent;
            var direction = player.GetComponent<PlayerController>().m_facingDirection;
            SendCoil(transform.position + new Vector3(direction * m_coilDistance*2, 0, 0));
        }
        else if (m_coil2CooldownTime < m_timeSinceCoil2 && Input.GetKeyDown("c"))
        {
            m_timeSinceCoil2 = 0f;
            var player = transform.parent.parent;
            var direction = player.GetComponent<PlayerController>().m_facingDirection;
            SendCoil(transform.position + new Vector3(direction * m_coilDistance*3, 0, 0));
        }
    }

    private void SendCoil(Vector3 pos)
    {
        CreateCoilServerRpc(pos);
        ShootEffectServerRpc();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, m_coilRadius);
        List<ulong> alreadyHit = new List<ulong>();

        foreach (var collider in colliders)
        {
            if(collider.CompareTag("Player") || collider.CompareTag("Damageable"))
            {
                var net = collider.GetComponent<NetworkObject>();
                if (net.OwnerClientId == OwnerClientId)
                    continue;
                if(alreadyHit.Contains(OwnerClientId))
                {
                    return;
                }
                alreadyHit.Add(OwnerClientId);
                var health = collider.GetComponent<IHealth>();
                health.TakeDamage(m_damage);
                hitCoils++;
                if (hitCoils >= 3)
                {
                    hitCoils = 0;
                    Time.timeScale = 0;
                    m_pauseTime = Time.realtimeSinceStartup;
                    if (IsHost)
                    {
                        m_isPaused.Value = true;
                    }
                    else if (IsClient)
                    {
                        RequestFreezeServerRpc();
                    }
                }
            }
            
        }
    }


    void PauseSetup()
    {
        var ui = GameObject.Find("Canvas");
        var display = Instantiate(m_pauseDisplay, ui.transform);
        var pauseController = display.GetComponent<PausePopupController>();
        pauseController.PauseDuration = m_pauseDuration;
        pauseController.StartPause();
    }

    [ServerRpc(RequireOwnership =false)]
    void CreateCoilServerRpc(Vector3 pos)
    {
        var coilInstace = Instantiate(m_coil, pos, Quaternion.identity);
        coilInstace.GetComponent<NetworkObject>().Spawn();
        Destroy(coilInstace, m_coilShowTime);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestFreezeServerRpc()
    {
        m_isPaused.Value = true;
    }
    public void DropWeaponEventFired()
    {
        if (!IsOwner)
            return;
        if(IsHost)
        {
            m_timeSinceCoil0Other.Value = m_timeSinceCoil0;
            m_timeSinceCoil1Other.Value = m_timeSinceCoil1;
            m_timeSinceCoil2Other.Value = m_timeSinceCoil2;
        }
        else if(IsClient)
        {
            RequestDropServerRpc(m_timeSinceCoil0, m_timeSinceCoil1, m_timeSinceCoil2);
        }
        
    }
    public void PickupWeaponEventFired()
    {
        if (!IsOwner)
            return;
        m_timeSinceCoil0 = m_timeSinceCoil0Other.Value;
        m_timeSinceCoil1 = m_timeSinceCoil1Other.Value;
        m_timeSinceCoil2 = m_timeSinceCoil2Other.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestDropServerRpc(float time0, float time1, float time2)
    {
        m_timeSinceCoil0Other.Value = time0;
        m_timeSinceCoil1Other.Value = time1;
        m_timeSinceCoil2Other.Value = time2;
    }
}
