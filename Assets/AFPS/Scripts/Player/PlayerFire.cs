using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerFire : NetworkBehaviour
{
    // Weapons
    bool gauntlet = true;
    bool plasma = true;
    bool lg = true;
    bool rail = true;
    bool rl = true;

    // Stats
    int damagePerShot;                         // The damage inflicted by each shot.                        lg = 7, rail = 30, plasma = 20, rl = 100
    float timeBetweenShots;              // The time between each shot.                          lg = 0.055f, rail = 1.5f, plasma = 0.11, rl  = 0.8
    float range;                             // The distance the gun can fire.
    Vector3 knockback;
    float knockbackForce = 5f;

    // Projectiles
    bool projectile;
    GameObject projectilePrefab;             // Prefab of the projectile
    float spawnDistance = 0.0f;                          // How far from the player projectile should spawn                         // Spawn distance is pointless with projectiles igonring the shooter. Might be needed for better visuals?
    int projectileSpeed = 25;
    float splashRadius;                                                                                                      // Currently using prefab size for explosion / splash size
    int maximumSplashDamage;
    float projectileLifeTime = 2.0f;                // For deleting projectiles that hit nothing

    public GameObject rocketPrefab;     // Rocket projectile
    public GameObject plasmaPrefab;     // Plasma projectile

    // SFX
    bool useBeam;                                   // Does this weapon use beam sfx?
    float effectDisplayTime = 0.1f;                     // For how long beam is displayed
    //public ParticleSystem impactEffect;
    public AudioClip[] m_HitSounds;

    private AudioSource m_AudioSource;
    private LineRenderer beamLine;

    Transform camera;                                       //camera location for shooting

    public float timer;                                    // A timer to determine when to fire.
    Ray shootRay = new Ray();                       // A ray from the gun end forwards.
    RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
    int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.

    void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy", "Environment");

        // Get audiosource
        m_AudioSource = GetComponent<AudioSource>();

        // Get line renderer component
        beamLine = GetComponent<LineRenderer>();
        beamLine.enabled = false;

        //get camera
        camera = transform.GetChild(0);

        timer = timeBetweenShots; // Start without cooldown

        //Starting weapon (gauntlet) stats
        ChangeWeapon("Gauntlet", 14, 0.55f, 2, false, false, null, 0, 0);
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        SelectWeapon();

        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        // Update beam sfx starting position (weapon origin)
        if (useBeam && Input.GetButton("Fire1"))
        {
            beamLine.SetPosition(0, transform.GetChild(0).GetChild(0).position);
        }
        //Disable beam sfx
        if (useBeam && !Input.GetButton("Fire1") && timer >= effectDisplayTime)
        {
            beamLine.enabled = false;
        }

        // If the Fire1 button is being pressed and it's time to fire...
        if (Input.GetButton("Fire1") && timer >= timeBetweenShots && Time.timeScale != 0)
        {
            // ... shoot the gun.
            if (projectile)
            {
                CmdFireProjectile();
            }
            else
            {
                CmdFireHitScan();
            }
        }
    }

    [Command]
    void CmdFireProjectile()
    {
        // Reset the timer.
        timer = 0f;

        // Spawn point
        Vector3 projectileSpawn = camera.position + camera.transform.forward.normalized * spawnDistance;

        // Create the projectile from the Prefab
        var projectile = (GameObject)Instantiate(projectilePrefab, projectileSpawn, camera.rotation);

        // Add velocity to the projectile
        projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileSpeed;

        // Other projectile stats
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.damagePerShot = damagePerShot;
        projectileScript.knockbackForce = knockbackForce;
        projectileScript.splashDamage = maximumSplashDamage;

        // Link this script
        projectileScript.parentScript = GetComponent<PlayerFire>();
        // Link player gameobject
        projectileScript.parentGameObject = transform.gameObject;

        // Spawn the projectile on the Clients
        NetworkServer.Spawn(projectile);

        // Destroy the bullet after x seconds
        Destroy(projectile, projectileLifeTime);
    }

    [Command]
    void CmdFireHitScan()
    {
        // Reset the timer.
        timer = 0f;

        // Enable line renderer
        if (useBeam)
        {
            beamLine.enabled = true;
        }

        // Shoot ray forward from camera
        shootRay.origin = camera.transform.position;
        shootRay.direction = camera.transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
        {
            // Workaround for self hits. problem atleast with jumppads                                                              //<-- FIX. allow raycast to hit multiple targets + ignore shooter? + boolean for toggling multi target since only railgun hits mutiple targets
            if (transform.root.gameObject == shootHit.transform.gameObject)                                                         // Set layer locally? Start as enemy, change into player and then this new layer is ignored? should work as long as change isnt updated for other clients
            {                                                                                                                                   // or just set starting point just outside of the players collider
                //return;
            }
            //

            HitDummy();     // For testing

            // If the Health component exists...
            PlayerHealth playerHealth = shootHit.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
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

            // Set the second position of the line renderer to the point the raycast hit.
            if (useBeam)
            {
                beamLine.SetPosition(1, shootHit.point);
            }

            // Impact effect
            //Impact();

        }
        // If the raycast didn't hit anything on the shootable layer...
        else
        {
            // ... set the second position of the line renderer to the fullest extent of the gun's range.
            if (useBeam)
            {
                beamLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
            }
        }
    }

    void SelectWeapon()
    {
        if (timer < timeBetweenShots || Input.GetButton("Fire1"))            // To change a weapon, current weapon must be ready to fire and player cannot be shooting
        {
            return;
        }

        // Input to select this weapon and player has it available
        if (Input.GetButton("Gauntlet") && gauntlet)
        {
            ChangeWeapon("Gauntlet", 14, 0.55f, 2, false, false, null, 0, 0);
        }

        if (Input.GetButton("Plasma") && plasma)
        {
            ChangeWeapon("Plasma", 20, 0.11f, 0, false, true, plasmaPrefab, 25, 15);
        }

        if (Input.GetButton("RL") && rl)
        {
            ChangeWeapon("RocketLauncher", 100, 0.8f, 0, false, true, rocketPrefab, 25, 100);
        }

        if (Input.GetButton("LG") && lg)
        {
            ChangeWeapon("LG", 7, 0.055f, 50, true, false, null, 0, 0);
        }

        if (Input.GetButton("Rail") && rail)
        {
            ChangeWeapon("Rail", 90, 1.5f, 50, true, false, null, 0, 0);
        }
    }

    void ChangeWeapon(string weapon, int damage, float firerate, float maxRange, bool beam, bool isProjectile, GameObject setProjectilePrefab, int setProjectileSpeed, int splashDmg)
    {
        useBeam = beam;
        projectile = isProjectile;
        damagePerShot = damage;
        timeBetweenShots = firerate;
        range = maxRange;
        //knockbackForce        // same for all weapons. for now atleast

        if(isProjectile)
        {
            projectilePrefab = setProjectilePrefab;
            maximumSplashDamage = 100;
            projectileSpeed = setProjectileSpeed;
            maximumSplashDamage = splashDmg;
        }

        // Show corrent weapon model
        ChangeWeaponModel(weapon);
    }

    void ChangeWeaponModel(string weapon)
    {
        // Disable every weapon model
        for (int i = 0; i < transform.GetChild(0).GetChild(0).childCount; i++)
        {
            transform.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(false);
        }

        // Enable selected weapon
        transform.GetChild(0).GetChild(0).Find(weapon).gameObject.SetActive(true);

    }

    public void PlayHitSounds(bool kill)
    {
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

    public void Respawn()
    {
        //Starting weapon (gauntlet) stats
        ChangeWeapon("Gauntlet", 14, 0.55f, 2, false, false, null, 0, 0);

        // Disable other weapons
        plasma = false;
        lg = false;
        rl = false;
        rail = false;
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

    void HitDummy()
    {
        // Try and find an Health script on the gameobject hit.
        TargetDummy targetDummy = shootHit.collider.GetComponent<TargetDummy>();

        if (targetDummy != null)
        {
            // Knockback calculated using position of the weapon, use camera location instead to get correct vector?
            //knockback = transform.forward.normalized * knockbackForce;                                                    // Knockback direction (normalized) * force
            //knockback = (shootHit.transform.position - transform.position).normalized * knockbackForce;                   // Same as before

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
