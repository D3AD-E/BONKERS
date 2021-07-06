using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Prototyping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : DroppableWeapon
{
    // Start is called before the first frame update
    [SerializeField] GameObject m_explosion;
    [SerializeField] float m_explosionDelay = 2f;
    [SerializeField] float m_explosionRadius = 15f;
    [SerializeField] float m_explosionForce = 20f;
    [SerializeField] float m_damage = 50f;
    [SerializeField] AudioClip m_explosionSound;
    [SerializeField] AudioClip m_pinSound;
    private float m_time = 0;
    NetworkVariableBool m_isActivated = new NetworkVariableBool(false);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(m_isActivated.Value)
        {
            m_time += Time.deltaTime;
            if (m_time > m_explosionDelay)
            {
                if (IsHost)
                    ExplodeClientRpc();
                m_explosionDelay = 10000f;
                return;
            }
        }
        else if (IsOwner && Input.GetMouseButtonDown(0) && transform.parent != null)
        {
            m_audio.PlayOneShot(m_pinSound);
            if(IsHost)
                m_isActivated.Value = true;
            else if(IsClient)
            {
                ActivateServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExplodeServerRpc()
    {
        ExplodeClientRpc();
    }

    [ClientRpc]
    public void ExplodeClientRpc()
    {
        var explosion = Instantiate(m_explosion, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(m_explosionSound, transform.position);
        var particles = explosion.GetComponent<ParticleSystem>();
        particles.Play();
        if (!IsOwner)
            return;
        var overlap = Physics2D.OverlapCircleAll(transform.position, m_explosionRadius);
        var alreadyHit = new List<Rigidbody2D>();
        foreach (var obj in overlap)
        {
            if (obj.CompareTag("Player") && !alreadyHit.Contains(obj.GetComponent<Rigidbody2D>()))
            {
                var body = obj.GetComponent<Rigidbody2D>();
                alreadyHit.Add(body);

                var health = obj.GetComponent<IHealth>();
                health.TakeDamage(m_damage);

                obj.GetComponent<PlayerController>().ExplosionDisable();
                body.AddRelativeForce(((obj.transform.position + Vector3.up) - this.transform.position).normalized * m_explosionForce, ForceMode2D.Impulse);
            }
            else if (obj.CompareTag("Damageable"))
            {
                var health = obj.GetComponent<IHealth>();
                health.TakeDamage(m_damage);
            }
        }
        Destroy(explosion, 0.5f);
        if (transform.parent != null)
            transform.parent.parent.GetComponent<PlayerController>().m_currentWeapon = WeaponType.None;
        if (IsHost)
            Destroy(gameObject);
        else if (IsClient && IsOwner)
            DestroyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void ActivateServerRpc()
    {
        m_isActivated.Value = true;
    }

}
