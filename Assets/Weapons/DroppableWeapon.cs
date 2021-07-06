using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Prototyping;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppableWeapon : NetworkBehaviour
{
    Rigidbody2D m_body;
    Collider2D m_collider;
    Light m_halo;
    protected bool m_isShooting = true;
    protected bool m_isPickupable = true;
    [SerializeField] WeaponType m_type;
    [SerializeField] AudioClip m_shootSound;
    [SerializeField] AudioClip m_equipSound;
    protected AudioSource m_audio;
    //public bool m_isDropped { get; protected set; }
    protected NetworkVariableBool m_isDropped = new NetworkVariableBool(false);

    public delegate void DroppableWeaponHandler();

    public event DroppableWeaponHandler OnDropped;
    public event DroppableWeaponHandler OnPickedUp;
    protected virtual void Awake()
    {
        m_audio = GetComponent<AudioSource>();
        m_body = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();
        m_halo = GetComponent<Light>();

        OnPickedUp += new DroppableWeaponHandler(OnPickedUpEffect);
        m_audio.clip = m_shootSound;
        if (m_audio && m_equipSound)
            m_audio.PlayOneShot(m_equipSound);
        //m_isDropped.OnValueChanged += OnDroppedValueChanged;
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.GetComponent<PlayerController>().enabled)
            {
                transform.SetParent(player.transform.Find("Arm"));
                break;
            }
        }
    }

    void OnPickedUpEffect()
    {
        if(m_audio && m_equipSound)
            m_audio.PlayOneShot(m_equipSound, 0.9f);
    }

    [ServerRpc(RequireOwnership =false)]
    protected void ShootEffectServerRpc()
    {
        ShootEffectClientRpc();
    }

    [ClientRpc]
    void ShootEffectClientRpc()
    {
        if (m_audio && m_shootSound)
            m_audio.PlayOneShot(m_shootSound);
    }

    //[ClientRpc]
    //protected void PlaySound()
    //{

    //}
    //private void OnDroppedValueChanged(bool previousValue, bool newValue)
    //{
    //    if(newValue)
    //    {
    //        m_isShooting = false;
    //        if (m_halo != null)
    //            m_halo.enabled = true;
    //        if (m_body != null)
    //        {
    //            //m_body.bodyType = RigidbodyType2D.Dynamic;
    //            //transform.localPosition += new Vector3(0.3f, 0, 0);
    //            //m_body.AddRelativeForce(Vector2.right * 10, ForceMode2D.Impulse);
    //        }
    //        if (m_collider != null)
    //            m_collider.enabled = true;
    //        transform.parent = null;
    //        tag = "DroppedWeapon";
    //        OnDropped?.Invoke();
    //    }
    //    else
    //    {
    //        m_body.velocity = Vector2.zero;
    //        m_isShooting = true;
    //        if (m_halo != null)
    //            m_halo.enabled = false;
    //        if (m_body != null)
    //            m_body.bodyType = RigidbodyType2D.Kinematic;
    //        if (m_collider != null)
    //            m_collider.enabled = false;
    //        //transform.parent = parent;
    //        tag = "Weapon";
    //        OnPickedUp?.Invoke();
    //    }
    //}


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DropWeapon()
    {
        if (!m_isDropped.Value)
        {
            if(transform.parent)
            {
                transform.parent.parent.GetComponent<PlayerController>().m_currentWeapon = WeaponType.None;
            }
            if (IsHost)
            {
                m_isDropped.Value = true;

                //m_body.bodyType = RigidbodyType2D.Dynamic;
                //transform.localPosition += new Vector3(0.3f, 0, 0);
                //m_body.AddRelativeForce(Vector2.right * 10, ForceMode2D.Impulse);
                DropClientRpc();
            }
            else if (IsClient)
                DroppedStateServerRpc(true);
            //m_isShooting = false;
            //if(m_halo!=null)
            //    m_halo.enabled = true;
            //if (m_body != null)
            //{
            //    m_body.bodyType = RigidbodyType2D.Dynamic;
            //    transform.localPosition += new Vector3(0.3f , 0, 0);
            //    m_body.AddRelativeForce(Vector2.right * 10, ForceMode2D.Impulse);
            //}
            //if(m_collider !=null)
            //   m_collider.enabled = true;
            //transform.parent = null;
            //tag = "DroppedWeapon";
        }
    }

    public void PickupWeapon(Transform parent)
    {
        if (m_isDropped.Value && m_isPickupable)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                if (player.GetComponent<PlayerController>().enabled)
                {
                    transform.SetParent(player.transform.Find("Arm"));
                    break;
                }
            }
            //m_body.velocity = Vector2.zero;
            if (IsHost)
            {
                m_isDropped.Value = false;
                PickupClientRpc();
            }
            else if (IsClient)
                DroppedStateServerRpc(false);
            //m_isShooting = true;
            //if (m_halo != null)
            //    m_halo.enabled = false;
            //if (m_body != null)
            //    m_body.bodyType = RigidbodyType2D.Kinematic;
            //if (m_collider != null)
            //    m_collider.enabled = false;
            transform.parent = parent;
            //tag = "Weapon";
        }
    }
    [ClientRpc]
    private void PickupClientRpc()
    {
        m_body.velocity = Vector2.zero;
        m_isShooting = true;
        if (m_halo != null)
            m_halo.enabled = false;
        if (m_body != null)
            m_body.bodyType = RigidbodyType2D.Kinematic;
        if (m_collider != null)
            m_collider.enabled = false;
        //transform.parent = parent;
        tag = "Weapon";
        OnPickedUp?.Invoke();
    }

    public WeaponType GetWeaponType()
    {
        return m_type;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //var network = collision.transform.GetComponent<NetworkObject>();
        //if (network)
        //{
        //    if (network.OwnerClientId == OwnerClientId)
        //    {
        //        Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        //        return;
        //    }
        //}

        if (collision.gameObject.tag == "Player")
        {
            if (collision.relativeVelocity.sqrMagnitude > 17f)
            {
                var player = collision.gameObject.GetComponent<PlayerController>();
                if(player!=null)
                    player.DropWeapon();
            }
        }
    }

    [ServerRpc(RequireOwnership =false)]
    void DroppedStateServerRpc(bool state)
    {
        m_isDropped.Value = state;
        if (state)
        {
            DropClientRpc();
        }
        else
            PickupClientRpc();
    }

    [ClientRpc]
    void DropClientRpc()
    {
        m_isShooting = false;
        if (transform.parent)
        {
            transform.parent.parent.GetComponent<PlayerController>().m_currentWeapon = WeaponType.None;
        }
        StartCoroutine(EnableAfter(0.02f));
        if (m_halo != null)
            m_halo.enabled = true;
        if (m_body != null)
        {
            m_body.bodyType = RigidbodyType2D.Dynamic;
            transform.localPosition += new Vector3(0.3f, 0, 0);
            //m_body.AddRelativeForce(Vector2.right * 10, ForceMode2D.Impulse);
            m_body.AddForce(transform.parent.right* 10, ForceMode2D.Impulse);
        }
        transform.parent = null;
        tag = "DroppedWeapon";
        OnDropped?.Invoke();
    }

    private IEnumerator EnableAfter(float time)
    {
        yield return new WaitForSeconds(time);
        if (m_collider != null)
            m_collider.enabled = true;
    }

    [ServerRpc]
    protected void DestroyServerRpc()
    {
        Destroy(gameObject);
    }
}
