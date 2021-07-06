using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperRifle : Weapon
{
    [SerializeField] float m_speed = 30f;
    Camera m_camera;
    [SerializeField] float m_scopeLength = 70f;
    [SerializeField] float m_scopeSpeed = 3f;
    Transform m_cameraParent;
    bool m_moved = false;
    //protected override void Awake()
    //{
    //    base.Awake();
    //    //m_firePoint = transform.Find("FirePoint");
    //    //m_timeToFire = m_fireDelay;
    //    //m_cameraParent = transform.parent.parent.Find("Camera");
    //    //m_camera = m_cameraParent.GetChild(0).GetComponent<Camera>();
    //    //OnDropped += new DroppableWeaponHandler(DropWeaponEventFired);
    //    //OnPickedUp += new DroppableWeaponHandler(PickupWeaponEventFired);
    //}

    public override void NetworkStart()
    {
        base.NetworkStart();
        m_firePoint = transform.Find("FirePoint");
        m_timeToFire = m_fireDelay;
        m_cameraParent = transform.parent.parent.Find("Camera");
        m_camera = m_cameraParent.GetChild(0).GetComponent<Camera>();
    }

    private void OnTransformParentChanged()
    {
        if(transform.parent)
        {
            m_cameraParent = transform.parent.parent.Find("Camera");
            m_camera = m_cameraParent.GetChild(0).GetComponent<Camera>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if (!IsOwner || m_isDropped.Value)
            return;
        if (Input.GetMouseButton(1))
        {
            if(m_camera)
            {
                m_moved = true;
                var diff = m_camera.ScreenToWorldPoint(Input.mousePosition) - m_cameraParent.position;
                diff.z = 0f;

                var lookAt = m_camera.ScreenToWorldPoint(Input.mousePosition);
                lookAt.z = m_camera.transform.position.z;
                if (diff.magnitude<m_scopeLength)
                {
                    m_camera.transform.position = Vector3.Lerp(m_camera.transform.position, lookAt, m_scopeSpeed * Time.deltaTime);
                }
                else
                {
                    diff.Normalize();
                    lookAt = diff * m_scopeLength;
                    lookAt.z = m_camera.transform.localPosition.z;
                    m_camera.transform.localPosition = Vector3.Lerp(m_camera.transform.localPosition, lookAt, m_scopeSpeed * Time.deltaTime);
                }
            }
        }
        else if(m_moved)
        {
            if (m_camera)
            {
                var lookAt = m_cameraParent.position;
                lookAt.z = m_camera.transform.position.z;

                m_camera.transform.position = Vector3.Lerp(m_camera.transform.position, lookAt, m_scopeSpeed * Time.deltaTime);
                if((m_camera.transform.position- lookAt).sqrMagnitude==0f)
                {
                    m_moved = false;
                }
            }
        }
    }

    new protected void Shoot()
    {
        if (IsHost)
        {
            var bullet = Instantiate(m_bullets, m_firePoint.position, m_firePoint.rotation);
            var movement = bullet.GetComponent<BulletMovement>();
            movement.SetDamage(m_damage);
            movement.SetSpeed(m_speed);
            bullet.GetComponent<NetworkObject>().Spawn();
        }
        else if (IsClient)
        {
            ShootServerRpc();
        }
    }

    [ServerRpc]
    private void ShootServerRpc()
    {
        var bullet = Instantiate(m_bullets, m_firePoint.position, m_firePoint.rotation);
        var movement = bullet.GetComponent<BulletMovement>();
        movement.SetDamage(m_damage);
        movement.SetSpeed(m_speed);
        bullet.GetComponent<NetworkObject>().Spawn();
    }
}
