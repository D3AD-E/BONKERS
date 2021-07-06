using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Prototyping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : DroppableWeapon
{
    [SerializeField] float m_cooldown = 5f;
    [SerializeField] float m_damage = 40f;
    [SerializeField] float m_rangeRadius = 40f;
    [SerializeField] Camera m_camera;
    [SerializeField] GameObject m_effect;
    Vector2 m_playerHitBoxSize;
    NetworkVariableFloat m_timeSinceUseDrop = new NetworkVariableFloat(5f);
    float m_timeSinceUse = 0;
    private new void Awake()
    {
        base.Awake();
        m_timeSinceUse = m_cooldown;
        OnDropped += new DroppableWeaponHandler(DropWeaponEventFired);
        OnPickedUp += new DroppableWeaponHandler(PickupWeaponEventFired);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || transform.parent == null)
            return;
        if (m_playerHitBoxSize == null)
        {
            var player = transform.parent.parent;
            var controller = player.GetComponent<PlayerController>();
            if (controller.m_rolling)
            {
                var box = player.GetComponent<BoxCollider2D>().size;
                m_playerHitBoxSize = new Vector2(box.x, (box.y - PlayerController.m_fixOffset) * 2);
            }
            else
            {
                m_playerHitBoxSize = player.GetComponent<BoxCollider2D>().size;
            }
        }
        m_timeSinceUse += Time.deltaTime;
        if (m_timeSinceUse >= m_cooldown && Input.GetMouseButtonDown(0))
        {
            var player = transform.parent.parent;
            m_camera = player.Find("Camera").GetChild(0).GetComponent<Camera>();
            var destination = m_camera.ScreenToWorldPoint(Input.mousePosition);
            var destinationFix = new Vector3(destination.x, destination.y);
            var diff = player.position - destinationFix;
            var diff2D = new Vector3(diff.x, diff.y);
            if(diff2D.sqrMagnitude<=m_rangeRadius)
            {
                var colliders = Physics2D.OverlapBoxAll(destinationFix, m_playerHitBoxSize, 0);
                List<Collider2D> playersHit = new List<Collider2D>();
                foreach (var collider in colliders)
                {
                    if (collider.gameObject.layer == LayerMask.NameToLayer("Midground"))
                        return;
                    if(collider.tag =="Player" && collider.GetComponent<NetworkObject>().OwnerClientId !=OwnerClientId)
                    {
                        playersHit.Add(collider);
                    }
                }
                foreach (var otherPlayer in playersHit)
                {
                    var health = otherPlayer.GetComponent<IHealth>();
                    health.TakeDamage(m_damage);
                }
                ShootEffectServerRpc();
                ShowEffectServerRpc(transform.position);
                ShowEffectServerRpc(destinationFix);

                player.GetComponent<NetworkTransform>().ForceSend(destinationFix);
                player.position = destinationFix;
                m_timeSinceUse = 0f;
            }
        }
    }

    public void DropWeaponEventFired()
    {
        if (!IsOwner)
            return;
        RequestDropServerRpc(m_timeSinceUse);
    }
    public void PickupWeaponEventFired()
    {
        if (!IsOwner)
            return;
        m_timeSinceUse = m_timeSinceUseDrop.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestDropServerRpc(float time)
    {
        m_timeSinceUseDrop.Value = time;
    }

    [ServerRpc(RequireOwnership = false)]
    void ShowEffectServerRpc(Vector3 position)
    {
        ShowEffectClientRpc(position);
    }

    [ClientRpc]
    void ShowEffectClientRpc(Vector3 position)
    {
        var effect = Instantiate(m_effect, position, Quaternion.identity);
        Destroy(effect, 0.5f);
    }
}
