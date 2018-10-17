using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerFire : NetworkBehaviour
{
    // Weapons, public for testing purposes
    [SyncVar]
    public bool gauntlet = true;
    [SyncVar]
    public bool plasma;
    [SyncVar]
    public bool lg;
    [SyncVar]
    public bool rail;
    [SyncVar]
    public bool rl;
    //[SyncVar(hook = "OnWeaponChange")]        //!!Hooked syncvar does not update automatically!!
    [SyncVar]
    public string currentWeapon = "Gauntlet";

    // Stats
    [SyncVar]
    int damagePerShot = 14;                         // The damage inflicted by each shot or impact damage for projectiles                        lg = 7, rail = 30, plasma = 20, rl = 100
    [SyncVar]
    float timeBetweenShots = 0.055f;              // The time between each shot.                          lg = 0.055f, rail = 1.5f, plasma = 0.11, rl  = 0.8
    [SyncVar]
    float range = 2;                             // The distance the gun can fire.
    Vector3 knockback;
    [SyncVar]
    float knockbackForce = 5f;

    // Projectiles
    [SyncVar]
    bool projectile = false;
    [SyncVar]
    int splashDamage;                           // Maximum amount of splash damage projectile can deal
    GameObject projectilePrefab;             // Prefab of the projectile
    float spawnDistance = 0.0f;                          // How far from the player projectile should spawn                         // Spawn distance is pointless with projectiles igonring the shooter. Might be needed for better visuals?
    [SyncVar]
    int projectileSpeed = 25;
    [SyncVar]
    float splashRadius;                                                                                                
    float projectileLifeTime = 2.0f;                // For deleting projectiles that hit nothing

    public GameObject rocketPrefab;     // Rocket projectile
    public GameObject plasmaPrefab;     // Plasma projectile

    // SFX
    [SyncVar]
    bool useBeam;                                   // Does this weapon use beam sfx?
    [SyncVar]
    bool beamActive;
    //[SyncVar]
    Vector3 beamStartPos;
    //[SyncVar]
    Vector3 beamEndPos;

    float effectDisplayTime = 0.25f;                     // For how long beam is displayed
    //public ParticleSystem impactEffect;
    public AudioClip[] m_HitSounds;

    private AudioSource m_AudioSource;
    private LineRenderer beamLine;

    Transform camera;                                       //camera location for shooting

    [SyncVar]
    public float timer;                                    // A timer to determine when to fire.
    Ray shootRay = new Ray();                       // A ray from the gun end forwards.
    RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
    int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.

    int numberOfPlayers;


    void Start()
    //public override void OnStartLocalPlayer()
    {
        //get camera
        camera = transform.GetChild(0);

        // Get line renderer component
        beamLine = GetComponent<LineRenderer>();

        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy", "Environment");

        if (!isLocalPlayer)
        {
            return;
        }

        gameObject.name = "Local Player";

        //Set layer of local player to "Player" (Layer 9) instead of "Enemy" layer              // Stops host from taking damage? But this should apply only to local player and not enemies?
        //gameObject.layer = 9;

        // Create a layer mask for the Shootable layer.
        //shootableMask = LayerMask.GetMask("Enemy", "Environment");

        // Get audiosource
        m_AudioSource = GetComponent<AudioSource>();

        // Get line renderer component
        //beamLine = GetComponent<LineRenderer>();                        // Cannot sync .enabled/.disabled between clients? Using [Syncvar] start/end pos instead (setting beam lenght to 0 when its disabled)
        //beamLine.enabled = false;

        timer = timeBetweenShots; // Start without cooldown

        //Starting weapon (gauntlet) stats
        CmdChangeWeapon("Gauntlet", 14, 0.055f, 2, false, false, null, 0, 0, 0);
    }

    /*
    // Called when string currentWeapon changes                         TESTING
    void OnWeaponChange(string currentWeapon)
    {
        //Debug.Log(currentWeapon);
    }
    */

    /*
    void OnPlayerConnected(NetworkPlayer player)                                // Not called?
    {
        Debug.Log("new player connected");

        // Sync current state when new player joins
        CmdSyncWeapon(currentWeapon);
    }
    */

    void OnPlayerConnect()
    {
        // When new player joins
        if (numberOfPlayers < NetworkManager.singleton.numPlayers)
        {
            numberOfPlayers = NetworkManager.singleton.numPlayers;
            Debug.Log("Players; " +numberOfPlayers);

            // Sync weapon model
            CmdSyncWeaponModel(currentWeapon);
        }
        // dc
        if(numberOfPlayers > NetworkManager.singleton.numPlayers)
        {
            numberOfPlayers = NetworkManager.singleton.numPlayers;
            Debug.Log("Players; " +numberOfPlayers);
        }
    }

    // Get beam Start/End Pos.
    void RenderBeam(bool enabled)                                                                //bool rapidFire        TODO: update positions once per shot for singleshot weapons (rail)
    {
        beamStartPos = transform.GetChild(0).GetChild(0).position;  // Weapon position

        // Disabled
        if (!enabled)
        {
            beamEndPos = transform.GetChild(0).GetChild(0).position;
            RenderBeam();
            return;
        }

        // Shoot ray forward from camera
        shootRay.origin = camera.transform.position;
        shootRay.direction = camera.transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
        {
            // Set the second position of the line renderer to the point the raycast hit.
            beamEndPos = shootHit.point;
        }
        // If the raycast didn't hit anything on the shootable layer...
        else
        {
            // ... set the second position of the line renderer to the fullest extent of the gun's range.
            beamEndPos = shootRay.origin + shootRay.direction * range;
        }

        RenderBeam();
    }

    void RenderBeam() // (Vector3 startpos, vector3 endpos)
    {
        // Render beams locally for each player
        beamLine.SetPosition(0, beamStartPos);
        beamLine.SetPosition(1, beamEndPos);
    }


    void Update()
    {
        // Update beam SFX each frame
        RenderBeam(beamActive);

        if (!isLocalPlayer)
        {
            return;
        }

        OnPlayerConnect();
        SelectWeapon();

        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        // If the Fire1 button is being pressed and it's time to fire...
        if (Input.GetButton("Fire1") && timer >= timeBetweenShots)
        {
            // Reset the timer.                                                                                         Note that resetting timer in [Command] would not reset timer for local player.
            timer = 0f;

            // ... shoot the gun.
            if (projectile)
            {
                CmdFireProjectile();
            }
            else
            {
                if (useBeam)
                {
                    CmdToggleBeam(true);
                }

                FireHitScan();
            }
        }

        if (useBeam)
        {
            // Disable beam SFX
            if (currentWeapon == "Rail" && timer >= effectDisplayTime)
            {
                CmdToggleBeam(false);
            }
            if (currentWeapon == "LG" && !Input.GetButton("Fire1"))
            {
                CmdToggleBeam(false);
            }
        }
    }

    [Command]
    void CmdToggleBeam(bool enabled)
    {
        if (enabled)
        {
            beamActive = true;
        }
        else
        {
            beamActive = false;
        }
    }

    [Command]
    void CmdFireProjectile()
    {
        // Spawn point
        Vector3 projectileSpawn = camera.position + camera.transform.forward.normalized * spawnDistance;

        // Create the projectile from the Prefab
        //var projectile = (GameObject)Instantiate(projectilePrefab, projectileSpawn, camera.rotation);                     //Cannot syncvar a gameobject
        GameObject projectile = null;
        if (currentWeapon == "RocketLauncher")
        {
            projectile = (GameObject)Instantiate(rocketPrefab, projectileSpawn, camera.rotation);
        }
        if (currentWeapon == "Plasma")
        {
            projectile = (GameObject)Instantiate(plasmaPrefab, projectileSpawn, camera.rotation);
        }

        // Add velocity to the projectile
        projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileSpeed;

        // Other projectile stats
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.damagePerShot = damagePerShot;
        projectileScript.knockbackForce = knockbackForce;
        projectileScript.splashDamage = splashDamage;
        projectileScript.splashRadius = splashRadius;

        // Link this script
        projectileScript.parentScript = GetComponent<PlayerFire>();
        // Link player gameobject
        projectileScript.parentGameObject = transform.gameObject;

        // Spawn the projectile on the Clients
        NetworkServer.Spawn(projectile);

        // Destroy the projectile after x seconds                                   // Should this be on projectile script instead? Destroy on impact or with delay. What happens when this attempts to destroy already destroyed projectile?
        Destroy(projectile, projectileLifeTime);
    }

    void FireHitScan()                                                              //TODO damage networking
    { 
        // Shoot ray forward from camera
        shootRay.origin = camera.transform.position;
        shootRay.direction = camera.transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
        {
            // Hits CapsuleCollider of player
            if (shootHit.collider.tag == "Player" && shootHit.collider is CapsuleCollider)
            {
                CmdHitDummy();     // For testing

                // Deal damage
                CmdHitPlayer();

                // Impact effect
                //Impact();
            }

        }
    }

    [Command]
    void CmdHitPlayer()
    {
        // If the Health component exists...
        PlayerHealth playerHealth = shootHit.collider.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Calculate knockback
            knockback = camera.transform.forward.normalized * knockbackForce;

            // ... the enemy should take damage.
            playerHealth.TakeDamage(damagePerShot, knockback);

            // Hp drops to 0 after taking damage
            if (playerHealth.currentHealth <= 0)
            {
                // Last hit
                PlayHitSounds(true);
            }
            else
            {
                PlayHitSounds(false);
            }
        }
    }


    // Woops. can "change" to already selected weapon. Will be problem later on with animations and whatnot         <-- FIX
    void SelectWeapon()
    {
        if (timer < timeBetweenShots || Input.GetButton("Fire1"))            // To change a weapon, current weapon must be ready to fire and player cannot be shooting
        {
            return;
        }

        // Input to select this weapon and player has it available
        if (Input.GetButton("Gauntlet") && gauntlet)
        {
            CmdChangeWeapon("Gauntlet", 14, 0.055f, 2, false, false, null, 0, 0, 0);
        }

        if (Input.GetButton("Plasma") && plasma)
        {
            CmdChangeWeapon("Plasma", 5, 0.11f, 0, false, true, plasmaPrefab, 25, 15, 0.5f);     // 5 impact, 15 splash. Previously 20/15, splash ignoring directly hit targets
        }

        if (Input.GetButton("RL") && rl)
        {
            CmdChangeWeapon("RocketLauncher", 50, 0.8f, 0, false, true, rocketPrefab, 25, 50, 2);  // 50 impact damage, 50 maximum splash. Previously 100/100, splash ignoring directly hit targets
        }

        if (Input.GetButton("LG") && lg)
        {
            CmdChangeWeapon("LG", 7, 0.055f, 50, true, false, null, 0, 0, 0);
        }

        if (Input.GetButton("Rail") && rail)
        {
            CmdChangeWeapon("Rail", 90, 1.5f, 50, true, false, null, 0, 0, 0);
        }
    }

    /*
    // Quick fix for syncing weapon states when new player joins
    [Command]
    void CmdSyncWeapon(string currentWeapon)
    {
        if(currentWeapon == "Gauntlet")
            CmdChangeWeapon("Gauntlet", 14, 0.055f, 2, false, false, null, 0, 0);
        if (currentWeapon == "Plasma")
            CmdChangeWeapon("Plasma", 20, 0.11f, 0, false, true, plasmaPrefab, 25, 15);
        if (currentWeapon == "RL")
            CmdChangeWeapon("RocketLauncher", 100, 0.8f, 0, false, true, rocketPrefab, 25, 100);
        if (currentWeapon == "LG")
            CmdChangeWeapon("LG", 7, 0.055f, 50, true, false, null, 0, 0);
        if (currentWeapon == "Rail")
            CmdChangeWeapon("Rail", 90, 1.5f, 50, true, false, null, 0, 0);
    }
    */

    [Command]
    void CmdSyncWeaponModel(string weapon)
    {
        RpcChangeWeaponModel(weapon);
    }

    [Command]
    void CmdChangeWeapon(string weapon, int damage, float firerate, float maxRange, bool beam, bool isProjectile, GameObject setProjectilePrefab, int setProjectileSpeed, int splashDmg, float splashRad)
    {
        currentWeapon = weapon;
        useBeam = beam;
        projectile = isProjectile;
        damagePerShot = damage;
        timeBetweenShots = firerate;
        timer = firerate;
        range = maxRange;
        //knockbackForce        // same for all weapons. for now atleast

        projectilePrefab = setProjectilePrefab;
        projectileSpeed = setProjectileSpeed;
        splashDamage = splashDmg;
        splashRadius = splashRad;


        // Show correct weapon model
        RpcChangeWeaponModel(weapon);
    }

    [ClientRpc]
    void RpcChangeWeaponModel(string weapon)
    {
        // Disable every weapon model
        for (int i = 0; i < transform.GetChild(0).GetChild(0).childCount; i++)
        {
            transform.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(false);
        }

        // Enable selected weapon model
        transform.GetChild(0).GetChild(0).Find(weapon).gameObject.SetActive(true);

    }

    public void PlayHitSounds(bool kill)
    {
        if (!isLocalPlayer)
        {
            //return;
        }

        if (kill) //killshot
        {
            m_AudioSource.clip = m_HitSounds[1];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
        }
        else // normal hit
        {
            m_AudioSource.clip = m_HitSounds[0];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
        }
    }

    [Command]
    public void CmdRespawn()
    {
        //Starting weapon (gauntlet) stats
        CmdChangeWeapon("Gauntlet", 14, 0.055f, 2, false, false, null, 0, 0, 0);

        // Disable other weapons
        plasma = false;
        lg = false;
        rl = false;
        rail = false;

        // Disable beam sfx
        beamActive = false;
    }

    /*
    private void Impact()
    {
        // effect location & rotation
        impactEffect.transform.position = shootHit.point;
        Vector3 dir = transform.position - shootHit.point;
        impactEffect.transform.rotation = Quaternion.LookRotation(dir);
        impactEffect.Play();
    }
    */

    public void GiveWeapon(string weaponName)
    {
        if(weaponName == "Plasma")
        {
            plasma = true;
        }
        if (weaponName == "LG")
        {
            lg = true;
        }
        if(weaponName == "RL")
        {
            rl = true;
        }
        if(weaponName == "Rail")
        {
            rail = true;
        }
    }

    [Command]
    void CmdHitDummy()
    {
        // Try and find an Health script on the gameobject hit.
        TargetDummy targetDummy = shootHit.collider.GetComponent<TargetDummy>();
        if (targetDummy != null)
        {
            // Calculate knockback
            knockback = camera.transform.forward.normalized * knockbackForce;

            // ... the enemy should take damage.
            targetDummy.TakeDamage(damagePerShot, knockback);

            // Hp drops to 0 after taking damage
            if (targetDummy.currentHealth <= 0)
            {
                PlayHitSounds(true);
            }
            else
            {
                PlayHitSounds(false);
            }
        }
    }
}
