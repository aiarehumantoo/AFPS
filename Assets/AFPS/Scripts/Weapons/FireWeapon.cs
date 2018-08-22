using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FireWeapon : MonoBehaviour
{

    [Header("Weapon Stats")]
    public int damagePerShot = 7;                         // The damage inflicted by each bullet.              lg = 7, rail = 30
    public float timeBetweenBullets = 0.055f;              // The time between each shot. QC LG firerate       lg = 0.055f, rail = 1.5f
    public float range = 100f;                             // The distance the gun can fire.
    Vector3 knockback;
    float knockbackForce = 20f;
    public int weaponSlot = 0;

    [Header("Projectile")]
    public bool projectile;
    public GameObject projectilePrefab;             // Prefab of the projectile
    int spawnDistance = 2;                          // How far from the player projectile should spawn
    public int projectileSpeed = 25;
    public float splashRadius;
    public float maximumSplashDamage;
    float projectileLifeTime = 2.0f;                // For deleting projectiles that hit nothing

    [Header("SFX")]
    public bool beam;                                   // Does this weapon use beam sfx?
    public float effectDisplayTime;                     // For how long beam is displayed
    public ParticleSystem impactEffect;
    public AudioClip[] m_HitSounds;
    


    private AudioSource m_AudioSource;
    private LineRenderer beamLine;

    Transform camera;                                       //camera location for shooting

    public float timer;                                    // A timer to determine when to fire.
    Ray shootRay = new Ray();                       // A ray from the gun end forwards.
    RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
    int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.





    // TODO NOTES

    /*
     * Alternatively could just render beam straight from the weapon, just altering distance if it hits something
     *      This would get rid of "stutter" of the sfx
     *      However then beams end point wouldnt be accurate
     * 
     */


    void Awake()
    {
        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy", "Environment");

        // Get audiosource
        m_AudioSource = GetComponent<AudioSource>();

        // Get line renderer component
        if (beam && !projectile)
        {
            beamLine = GetComponent<LineRenderer>();
        }

        //get camera
        camera = transform.parent.parent;

        timer = timeBetweenBullets; // Start without cooldown
    }

    void Update()
    {
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        // Update beam sfx starting position
        if(beam && Input.GetButton("Fire1") && !projectile)
        {
            beamLine.SetPosition(0, transform.position);
        }

        // If the Fire1 button is being press and it's time to fire...
        if (Input.GetButton("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
        {
            // ... shoot the gun.
            if (projectile)
            {
                FireProjectile();
            }
            else
            {
                FireHitScan();
            }
        }
        else if(timer >= effectDisplayTime && !projectile) // Not firing and sfx has expired
        {
            // Disable beam sfx
            if (beam)
            {
                beamLine.enabled = false;
            }
        }

        // DEBUG. Draw line from barrel to target. ENABLE GIZMOS IN GAME VIEW
        if (Input.GetButton("Fire1"))
        {
            Debug.DrawRay(transform.position, shootHit.point - transform.position, Color.green);
        }
        else
        {
            //Debug.DrawRay(transform.position, shootHit.point - transform.position, Color.red);
        }
    }

    void FireProjectile()
    {
        // Reset the timer.
        timer = 0f;

        // Spawn point
        Vector3 projectileSpawn = camera.position + camera.transform.forward.normalized * spawnDistance;

        // Create the Bullet from the Bullet Prefab
        //var projectile = (GameObject)Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);
        var projectile = (GameObject)Instantiate(projectilePrefab, projectileSpawn, camera.rotation);

        // Add velocity to the bullet
        projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileSpeed;

        // Other projectile stats
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.damagePerShot = damagePerShot;
        projectileScript.knockbackForce = knockbackForce;
        // Link this script
        projectileScript.parentScript = GetComponent<FireWeapon>();

        // Spawn the bullet on the Clients
        //NetworkServer.Spawn(projectile);

        // Destroy the bullet after x seconds
        Destroy(projectile, projectileLifeTime);
    }

    void FireHitScan()
    {
        // Reset the timer.
        timer = 0f;

        // Set start position of line renderer & enable it
        if (beam)
        {
            beamLine.enabled = true;
        }

        // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
        //shootRay.origin = transform.position;
        shootRay.origin = camera.transform.position;        //fwd from camera
        shootRay.direction = transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
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

            // If the Health component exist...
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
            if (beam)
            {
                beamLine.SetPosition(1, shootHit.point);
            }

            // Impact effect
            Impact();

        }
        // If the raycast didn't hit anything on the shootable layer...
        else
        {
            // ... set the second position of the line renderer to the fullest extent of the gun's range.
            if (beam)
            {
                beamLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
            }
        }
    }

    public void PlayHitSounds(bool kill)
    {
        if(kill) //killshot
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

    private void Impact()
    {
        // effect location & rotation
        impactEffect.transform.position = shootHit.point;
        Vector3 dir = transform.position - shootHit.point;
        impactEffect.transform.rotation = Quaternion.LookRotation(dir);
        impactEffect.Play();
    }
}