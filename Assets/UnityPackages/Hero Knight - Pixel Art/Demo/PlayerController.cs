using UnityEngine;
using System.Collections;
using MLAPI;
using System;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Linq;
using MLAPI.Prototyping;
using MLAPI.SceneManagement;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] float      m_rollForce = 6.0f;
    [SerializeField] float      m_slideDecrease = 0.35f;
    [SerializeField] float m_pickupRange = 1f;
    [SerializeField] float m_wallJumpDelay = 0.3f;
    [SerializeField] float m_wallJumpLoseControlDelay = 0.2f;
    [SerializeField] float m_wallJumpMultiplier = 2f;
    [SerializeField] GameObject m_slideDust;
    [SerializeField] GameObject m_scoreTable;
    [SerializeField] Transform m_cam;
    [SerializeField] float m_killThreshhold = -25f;
    private bool m_disableControls = false;
    public WeaponType m_currentWeapon = WeaponType.None;
    //GameObject m_weapon;
    BoxCollider2D m_hitBox;
    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_HeroKnight   m_groundSensor;
    private Sensor_HeroKnight   m_wallSensorR1;
    private Sensor_HeroKnight   m_wallSensorR2;
    private Sensor_HeroKnight   m_wallSensorL1;
    private Sensor_HeroKnight   m_wallSensorL2;
    private bool                m_grounded = false;
    public bool                m_rolling = false;
    public int m_facingDirection { get; private set; }
    private int                 m_currentAttack = 0;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private float               m_timeSinceWallJump =0f;
    private float m_slidingTime = 0f;
    Transform m_arm;
    public const float m_fixOffset = 0.03f;

    public float inputX;

    public string UserName = "DEBUG_NAME";
    bool m_showingScores = false;
    // Use this for initialization
    void Start ()
    {
        m_arm = transform.Find("Arm");
        m_facingDirection = 1;
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_hitBox = GetComponent<BoxCollider2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
    }

    public override void NetworkStart()
    {
        // TODO Don't use NetworkBehaviour for just NetworkStart [GOMPS-81]
        if (!IsLocalPlayer)
        {
            m_cam.GetComponent<Camera>().enabled = false;
            m_cam.GetComponent<AudioListener>().enabled = false;
            transform.Find("Arm").GetComponent<Rotation>().enabled = false;
            enabled = false;
            // dont need to do anything else if not the owner
            return;
        }
        UserName = GlobalData.UserName;
        //why netwrokstart is not called???
        GameObject.Find("SetupController").GetComponent<SpawnGCManager>().NetworkStart();

        GameplayManager.Instance.SetPlayerNameServerRpc(UserName, OwnerClientId);
        NetworkSceneManager.OnSceneSwitchStarted += OnSceneSwitchStarted;
        NetworkSceneManager.OnSceneSwitched += OnSceneSwitched;
        if(IsHost)
        {
            SpawnManager.Instance.ServerTeleport(gameObject);
        }
        else if(IsClient)
        {
            //GameplayManager.Instance.enabled = false;
            SpawnManager.Instance.TeleportToSpawnServerRpc(OwnerClientId);
        }
    }

    private void OnSceneSwitched()
    {
        if (IsHost)
        {
            SpawnManager.Instance.ServerTeleport(gameObject);
        }
        else if (IsClient)
        {
            SpawnManager.Instance.TeleportToSpawnServerRpc(OwnerClientId);
        }
        StartCoroutine(Restore());
    }

    private void OnSceneSwitchStarted(AsyncOperation operation)
    {
        GetComponent<PlayerHealthController>().SetEnabledServerRpc(false);
        m_hitBox.enabled = false;
        m_body2d.velocity = Vector2.zero;
        m_body2d.bodyType = RigidbodyType2D.Static;
        this.enabled = false;
    }

    [ClientRpc]
    public void PlayerDiedClientRpc(ulong id)
    {
        if (id == OwnerClientId && IsLocalPlayer)
        {
            DropWeapon();
            m_disableControls = true;
            m_body2d.bodyType = RigidbodyType2D.Kinematic;
            m_body2d.velocity = Vector2.zero;
            m_hitBox.enabled = false;
            GetComponent<CircleCollider2D>().enabled = false;
        }
    }


    private IEnumerator Restore()
    {
        m_facingDirection = 1;
        if (m_animator != null)
        {
            m_animator.SetTrigger("ReturnToNormal");
            m_animator.SetBool("Enabled", true);
        }
            
        m_grounded = false;
        m_rolling = false;
        m_timeSinceAttack = 0.0f;
        m_delayToIdle = 0.0f;
        m_timeSinceWallJump = 0f;
        m_slidingTime = 0f;
        m_groundSensor.Restore();
        m_wallSensorR1.Restore();
        m_wallSensorR2.Restore();
        m_wallSensorL1.Restore();
        m_wallSensorL2.Restore();
        m_currentWeapon = WeaponType.None;
        if (transform.Find("Arm").childCount > 0)
            Destroy(transform.Find("Arm").GetChild(0));

        yield return new WaitForSeconds(0.2f);
        enabled = true;
        m_disableControls = false;
        m_hitBox.enabled = true;
        GetComponent<CircleCollider2D>().enabled = true;
        m_body2d.bodyType = RigidbodyType2D.Dynamic;
        m_body2d.velocity = Vector2.zero;

        Destroy(GameObject.Find("Canvas").GetComponent<CanvasController>());
        GameObject.Find("Canvas").AddComponent<CanvasController>();

        GetComponent<PlayerHealthController>().SetEnabledServerRpc(true);
        GetComponent<PlayerHealthController>().Heal(100f);
    }
    // Update is called once per frame
    void Update ()
    {
        if(transform.position.y < m_killThreshhold)
        {
            GetComponent<PlayerHealthController>().Kill();
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            if (!m_showingScores)
            {
                var table = Instantiate(m_scoreTable, GameObject.Find("Canvas").transform);
                table.GetComponent<ScoreTableManager>().UpdateTable();
                m_showingScores = true;
            }
        }
        else if (!Input.GetKeyDown(KeyCode.Tab))
        {
            if (m_showingScores)
            {
                var table = GameObject.FindGameObjectWithTag("ScoreTable");
                if (table)
                    Destroy(table);
                m_showingScores = false;
            }
        }
       
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;
        m_timeSinceWallJump += Time.deltaTime;
        //Check if character just landed on the ground
        
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        if (m_disableControls)
            return;

        // -- Handle input and movement --
        inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        //but dont flip if we are sliding
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            if(IsServer)
                GetComponent<FlipHelper>().FlipClientRpc(false);
            else if(IsClient)
                GetComponent<FlipHelper>().FlipServerRpc(false);
            m_facingDirection = 1;
        } 
        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            if (IsServer)
                GetComponent<FlipHelper>().FlipClientRpc(true);
            else if (IsClient)
                GetComponent<FlipHelper>().FlipServerRpc(true);
            m_facingDirection = -1;
        }

        

        // Move
        if (!m_rolling && m_timeSinceWallJump>m_wallJumpLoseControlDelay && inputX!=0)
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // -- Handle Animations --
        //Wall Slide
        bool isSliding = (m_wallSensorR1.State() && m_wallSensorR2.State() && inputX > 0) || (m_wallSensorL1.State() && m_wallSensorL2.State() && inputX < 0) && m_body2d.velocity.y<-1f;
        m_animator.SetBool("WallSlide", isSliding);

        //if we are sliding make fall slower
        if (isSliding)
        {
            m_body2d.AddForce(Vector2.up * m_body2d.mass * m_slideDecrease);
            m_slidingTime += Time.deltaTime;
            if (Input.GetKeyDown("space") && m_timeSinceWallJump > m_wallJumpDelay && m_slidingTime > m_wallJumpDelay)
            {
                m_timeSinceWallJump = 0f;
                if (m_wallSensorR1.State() && m_wallSensorR2.State())
                {
                    //touchinbg on right
                    m_body2d.velocity = new Vector2(m_body2d.velocity.x - m_wallJumpMultiplier * m_speed, m_jumpForce);
                }
                else if (m_wallSensorL1.State() && m_wallSensorL2.State())
                {
                    m_body2d.velocity = new Vector2(m_body2d.velocity.x + m_wallJumpMultiplier * m_speed, m_jumpForce);
                }
                m_animator.SetTrigger("Jump");
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
                m_groundSensor.Disable(0.2f);
            }
        }
        else
            m_slidingTime = 0f;


        if (Input.GetKeyDown("e"))
        {
            if (m_currentWeapon == WeaponType.None)
            {
                var center = transform.GetComponent<Renderer>().bounds.center;
                //Collider2D[] colliders = Physics2D.OverlapCircleAll(center, m_pickupRange);
                Collider2D[] colliders = Physics2D.OverlapBoxAll(center, new Vector2(m_pickupRange, 2.2f), 0);
                foreach (var collider in colliders)
                {
                    if (collider.tag == "Collectable")
                    {
                        var pickup = collider.GetComponent<PickupLogic>();
                        if (pickup != null)
                        {
                            var weaponType = pickup.Pickup();
                            if (weaponType == WeaponType.None)
                                continue;
                            if(Physics2D.Linecast(center, collider.transform.position, 14))
                            {
                                //there is something in the way
                                continue;
                            }
                            m_currentWeapon = weaponType;
                            if (IsHost)
                            {
                                var weapon = WeaponManager.Instance.GetWeapon(weaponType);
                                var arm = transform.Find("Arm");
                                var weaponInstance = Instantiate(weapon, arm.position, arm.rotation, transform.Find("Arm"));
                                weaponInstance.transform.localRotation = Quaternion.identity;
                                weaponInstance.transform.localPosition = Vector3.zero;

                                weaponInstance.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
                                SetParentServerClientRpc(weaponInstance.GetComponent<NetworkObject>().NetworkObjectId, NetworkObjectId);
                            }
                            else if(IsClient)
                            {
                                SpawnWeaponServerRpc(weaponType, transform.Find("Arm").rotation, OwnerClientId);
                            }
                            break;
                        }
                    }
                    else if(collider.tag == "DroppedWeapon")
                    {
                        var pickup = collider.GetComponent<DroppableWeapon>();
                        if (pickup != null)
                        {
                            var weaponType = pickup.GetWeaponType();
                            if (weaponType == WeaponType.None)
                                continue;
                            if (Physics2D.Linecast(center, collider.transform.position, 14))
                            {
                                //there is something in the way
                                continue;
                            }
                            m_currentWeapon = weaponType;
                            var arm = transform.Find("Arm");
                            pickup.PickupWeapon(arm);
                            pickup.transform.position = arm.position;
                            pickup.transform.localRotation = new Quaternion();
                            if(IsHost)
                            {
                                collider.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
                                collider.GetComponent<NetworkTransform>().Teleport(arm.position, arm.rotation);
                                SetParentServerClientRpc(collider.GetComponent<NetworkObject>().NetworkObjectId, NetworkObjectId);
                            }
                            else if(IsClient)
                            {
                                SetParentServerRpc(collider.GetComponent<NetworkObject>().NetworkObjectId, weaponType);
                            }

                            break;
                        }
                    }
                }
            } 
        }
        else if (Input.GetKeyDown("q"))
        {
            DropWeapon();
        }
        

        //Attack
        else if(Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Block
        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }

        else if (Input.GetMouseButtonUp(1))
            m_animator.SetBool("IdleBlock", false);

        // Roll
        else if (Input.GetKeyDown("left shift") && !m_rolling && m_grounded)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_hitBox.enabled = false;
            //m_hitBox.size = new Vector2(m_hitBox.size.x, m_hitBox.size.y / 2);

            //m_hitBox.offset = new Vector2(m_hitBox.offset.x, m_hitBox.offset.y/2f+ m_fixOffset);
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
            
            m_arm.localPosition= new Vector3(m_arm.localPosition.x, m_arm.localPosition.y / 2 + m_fixOffset, m_arm.localPosition.z);
        }
            

        //Jump
        else if (Input.GetKeyDown("space") &&!Input.GetKey(KeyCode.S) && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            
            m_groundSensor.Disable(0.2f);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
                if(m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnWeaponServerRpc(WeaponType weaponType, Quaternion rotation, ulong id)
    {
        var weapon = WeaponManager.Instance.GetWeapon(weaponType);
        var arm = transform.Find("Arm");
        var weaponInstance = Instantiate(weapon, arm.position, rotation, transform.Find("Arm"));
        var net = weaponInstance.GetComponent<NetworkObject>();
        net.SpawnWithOwnership(id);
        SetParentClientRpc(net.NetworkObjectId, weaponType);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetParentServerRpc(ulong idNum, WeaponType weaponType)
    {
       // GameObject player = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.gameObject;
        var net = GetNetworkObject(idNum);
        if (net)
        {
            net.ChangeOwnership(OwnerClientId);
        }
        SetParentClientRpc(idNum, weaponType);
    }

    [ClientRpc]
    public void SetParentClientRpc(ulong idNum, WeaponType weaponType)
    {
        //if (IsServer)
        //   return;
        //GameObject player = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.gameObject;
        var net = GetNetworkObject(idNum);
        
        //if(net)
        {
            var weapon = net.gameObject;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            weapon.transform.SetParent(transform.Find("Arm").transform);
            GetComponent<PlayerController>().m_currentWeapon = weaponType;
        }
    }

    [ClientRpc]
    public void SetParentServerClientRpc(ulong weaponId, ulong serverId)
    {
        var weapon = GetNetworkObject(weaponId);
        if (weapon)
        {
            var player = GetNetworkObject(serverId);
            if(player)
            {
                weapon.transform.position = player.transform.Find("Arm").position;
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;
                weapon.transform.parent = player.transform.Find("Arm");
            }
        }
    }

    public void ExplosionDisable()
    {
        StartCoroutine(DisableFor(0.8f));
    }

    private IEnumerator DisableFor(float time)
    {
        m_disableControls = true;
        yield return new WaitForSeconds(time);
        m_disableControls = false;
    }

    // Animation Events
    // Called in end of roll animation.
    void AE_ResetRoll()
    {
        if (!IsLocalPlayer)
            return;
        m_rolling = false;
        m_hitBox.enabled = true;
        //m_hitBox.size = new Vector2(m_hitBox.size.x, m_hitBox.size.y * 2);
        
        //m_hitBox.offset = new Vector2(m_hitBox.offset.x, (m_hitBox.offset.y - m_fixOffset)*2);
        m_arm.localPosition = new Vector3(m_arm.localPosition.x, (m_arm.localPosition.y - m_fixOffset) *2, m_arm.localPosition.z);
    }
    public void DropWeapon()
    {
        if (m_currentWeapon != WeaponType.None && IsLocalPlayer)
        {
            m_currentWeapon = WeaponType.None;
            var weapon = transform.Find("Arm").GetChild(0);
            weapon.GetComponent<DroppableWeapon>().DropWeapon();
        }
    }

    // Called in slide animation.
    void AE_SlideDust()
    {
        if (!IsLocalPlayer)
            return;
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

}
