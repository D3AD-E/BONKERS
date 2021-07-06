using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesLogic : MonoBehaviour
{
    [SerializeField] float m_damage = 100f;
    [SerializeField] bool m_isInstaKill = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            var health = collision.gameObject.GetComponent<IHealth>();
            if(health != null)
            {
                if(collision.gameObject.GetComponent<NetworkObject>().IsLocalPlayer)
                {
                    if (m_isInstaKill)
                    {
                        health.Kill();
                    }
                    else
                    {
                        health.TakeDamage(m_damage);
                    }
                }
            }
        }
    }
}
