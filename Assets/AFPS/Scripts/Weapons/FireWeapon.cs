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
    float knockbackForce = 100f;

    [Header("Projectile")]
    public bool projectile;
    public GameObject projectilePrefab;             // Prefab of the projectile
    int spawnDistance = 2;                          // How far from the player projectile should spawn
    public int projectileSpeed = 25;

    [Header("SFX")]
    public bool beam;                                   // Does this weapon use beam sfx?
    public float effectDisplayTime;                     // For how long beam is displayed
    public ParticleSystem impactEffect;
    public AudioClip[] m_HitSounds;
    


    private AudioSource m_AudioSource;
    LineRenderer beamLine;

    Transform camera;                                       //camera location for shooting

    float timer;                                    // A timer to determine when to fire.
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
        Vector3 projectileSpawn = camera.position + camera.transform.forward * spawnDistance;

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

        // Destroy the bullet after 5 seconds
        Destroy(projectile, 5.0f);
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
            // Try and find an EnemyHealth script on the gameobject hit.
            TargetDummy targetDummy = shootHit.collider.GetComponent<TargetDummy>();

            if (targetDummy != null)
            {
                knockback = transform.forward * knockbackForce;

                // ... the enemy should take damage.
                targetDummy.TakeDamage(damagePerShot, knockback);
                PlayHitSounds();
            }

            // If the Health component exist...
            PlayerHealth playerHealth = shootHit.collider.GetComponent<PlayerHealth>();             //CLEAN UP LATER
            if (playerHealth != null)
            {
                knockback = transform.forward * knockbackForce;

                // ... the enemy should take damage.
                playerHealth.TakeDamage(damagePerShot, knockback);
                PlayHitSounds();
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

    public void PlayHitSounds()
    {
        // pick & play a random jump sound from the array,
        // excluding sound at index 0
        //int n = Random.Range(1, m_HitSounds.Length);
        m_AudioSource.clip = m_HitSounds[0];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        //m_HitSounds[n] = m_HitSounds[0];
        //m_HitSounds[0] = m_AudioSource.clip;
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