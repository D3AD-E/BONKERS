using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject m_pistol;
    [SerializeField] GameObject m_grenade;
    [SerializeField] GameObject m_machineGun;
    [SerializeField] GameObject m_coil;
    [SerializeField] GameObject m_sniperRifle;
    [SerializeField] GameObject m_dieDice;
    [SerializeField] GameObject m_AK47;
    [SerializeField] GameObject m_teleport;

    private static WeaponManager _instance;

    public static WeaponManager Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetWeapon(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Pistol:
                return m_pistol;
            case WeaponType.Grenade:
                return m_grenade;
            case WeaponType.MachineGun:
                return m_machineGun;
            case WeaponType.Coil:
                return m_coil;
            case WeaponType.SniperRifle:
                return m_sniperRifle;
            case WeaponType.DieDice:
                return m_dieDice;
            case WeaponType.AK47:
                return m_AK47;
            case WeaponType.Teleport:
                return m_teleport;
        }
        return null;
    }
}
