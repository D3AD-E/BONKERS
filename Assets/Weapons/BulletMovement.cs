using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMovement : NetworkBehaviour
{
    [SerializeField] float m_speed =10f;
    [SerializeField] float m_damage = 10f;
    [SerializeField] AudioClip m_hitWallSound;
    [SerializeField] AudioClip m_hitPlayerSound;
    bool hit = false;
    float m_time = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * Time.deltaTime * m_speed);

        m_time += Time.deltaTime;
        if (m_time > 5f)
        {
            m_time = 0;
            gameObject.SetActive(false);
            DestroyServerRpc(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner || hit)
            return;
        if (!collision.enabled)
        {
            return;
        }
        if (collision.CompareTag("Sensor") || collision.CompareTag("OneWay"))
            return;
        else if (collision.CompareTag("Collectable"))
            return;
        else if (collision.CompareTag("Player"))
        {
            var network = collision.transform.GetComponent<NetworkObject>();
            if (network)
            {
                if (network.IsLocalPlayer)
                    return;
            }
        }

        bool hitPlayer = false;

        if (collision.CompareTag("DroppedWeapon"))
        {
            var grenade = collision.GetComponent<Grenade>();
            if(grenade)
                grenade.ExplodeServerRpc();
            hit = true;
            //DestroyServerRpc(false);
        }
        else if(collision.CompareTag("Player") || collision.CompareTag("Damageable"))
        {
            var controller = collision.GetComponent<IHealth>();
            if (controller != null)
            {
                controller.TakeDamage(m_damage);
                hit = true;
                hitPlayer = true;
                //DestroyServerRpc(true);
            }
        }

        gameObject.SetActive(false);
        m_time = 0;
        DestroyServerRpc(hitPlayer);
    }

    public void SetDamage(float damage)
    {
        m_damage = damage;
    }

    public void SetSpeed(float speed)
    {
        m_speed = speed;
    }

    [ServerRpc(RequireOwnership =false)]
    void DestroyServerRpc(bool hitPlayer)
    {
        //PlayHitSoundClientRpc(hitPlayer);
        Destroy(gameObject);
        // gameObject.SetActive(false);
        //gameObject.GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    void PlayHitSoundClientRpc(bool hitPlayer)
    {
        if (hitPlayer)
            AudioSource.PlayClipAtPoint(m_hitPlayerSound, transform.position);
        else
            AudioSource.PlayClipAtPoint(m_hitWallSound, transform.position);
    }
}
