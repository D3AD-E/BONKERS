using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Prototyping;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : DroppableWeapon
{
    [SerializeField] protected float m_fireDelay = 0.5f;
    [SerializeField] protected float m_damage = 10f;
    [SerializeField] protected Transform m_bullets;
    [SerializeField] private bool m_holdmouse = false;
    [SerializeField] private float m_spread = 0f;

    protected float m_timeToFire = 0.0f;
    protected Transform m_firePoint;

    [SerializeField] protected NetworkVariableInt m_bulletsAmount = new NetworkVariableInt(30);
    //make variable fo rbullets

    // [SerializeField] WeaponType m_type;
    // Start is called before the first frame update
    private new void Awake()
    {
        base.Awake();
        m_firePoint = transform.Find("FirePoint");
        m_timeToFire = m_fireDelay;
    }

    
    void Start()
    {

    }

    // Update is called once per frame
    protected void Update()
    {
        if (m_isDropped.Value || !IsOwner || !m_isShooting || m_bulletsAmount.Value <= 0)
            return;
        m_timeToFire += Time.deltaTime;
        if (m_timeToFire > m_fireDelay && ((m_holdmouse && Input.GetMouseButton(0)) || (!m_holdmouse && Input.GetMouseButtonDown(0))))
        {
            if (IsHost)
                m_bulletsAmount.Value--;
            else if (IsClient)
                DecreaseBulletsServerRpc();
            Shoot();
            m_timeToFire = 0.0f;
        }
    }

    protected void Shoot()
    {
        ShootEffectServerRpc();
        if (IsHost)
        {
            var bullet = Instantiate(m_bullets, m_firePoint.position, m_firePoint.rotation, transform.parent.parent);
            bullet.transform.Rotate(0, 0, Random.Range(-m_spread, m_spread));
            bullet.GetComponent<BulletMovement>().SetDamage(m_damage);
            bullet.GetComponent<NetworkObject>().Spawn();
            //use object pooling!
            //ShootServerRpc();
            //var bullet = ObjectPool.SharedInstance.GetObject();
            //if (bullet)
            //{
            //    //bullet.GetComponent<NetworkTransform>().Teleport(m_firePoint.position, m_firePoint.rotation);
            //    bullet.transform.position = m_firePoint.position;
            //    bullet.transform.rotation = m_firePoint.rotation;
            //    bullet.GetComponent<NetworkTransform>().Teleport(m_firePoint.position, m_firePoint.rotation);
            //    bullet.transform.Rotate(0, 0, Random.Range(-m_spread, m_spread));
            //    bullet.GetComponent<BulletMovement>().SetDamage(m_damage);
            //    bullet.SetActive(true);
            //    if (!bullet.GetComponent<NetworkObject>().IsSpawned)
            //        bullet.GetComponent<NetworkObject>().Spawn();
            //    else
            //        bullet.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
            //}
        }
        else if (IsClient)
        {
            ShootServerRpc();
        }
    }

    [ServerRpc]
    void DecreaseBulletsServerRpc()
    {
        m_bulletsAmount.Value--;
    }

    [ServerRpc]
    void ShootServerRpc()
    {
        var bullet = Instantiate(m_bullets, m_firePoint.position, m_firePoint.rotation, transform.parent.parent);
        bullet.transform.Rotate(0, 0, Random.Range(-m_spread, m_spread));
        bullet.GetComponent<BulletMovement>().SetDamage(m_damage);
        bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        //var bullet = ObjectPool.SharedInstance.GetObject();
        //if (bullet)
        //{
        //    bullet.transform.position = m_firePoint.position;
        //    bullet.transform.rotation = m_firePoint.rotation;
        //    bullet.transform.Rotate(0, 0, Random.Range(-m_spread, m_spread));
        //    bullet.GetComponent<BulletMovement>().SetDamage(m_damage);
        //    bullet.SetActive(true);
        //    if (!bullet.GetComponent<NetworkObject>().IsSpawned)
        //        bullet.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        //    else
        //        bullet.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
        //}
    }

}
