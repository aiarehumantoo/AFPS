using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class lg : MonoBehaviour
{
    //basic lg functions for testing purposes

    public Transform camera;                        //

    int damagePerShot = 0; //7                         // The damage inflicted by each bullet.
    float timeBetweenBullets = 0.055f;              // The time between each shot. QC LG firerate
    float range = 100f;                             // The distance the gun can fire.

    Vector3 knockback;
    float knockbackForce = 100f;

    float timer;                                    // A timer to determine when to fire.
    Ray shootRay = new Ray();                       // A ray from the gun end forwards.
    RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
    int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.

    public AudioClip[] m_HitSounds;
    private AudioSource m_AudioSource;

    public ParticleSystem impactEffect;


    void Awake()
    {
        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy","Environment");

        m_AudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        // If the Fire1 button is being press and it's time to fire...
        if (Input.GetButton("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
        {
            // ... shoot the gun.
            Fire();
        }

        // Debug. Draw line from barrel to target. ENABLE GIZMOS IN GAME VIEW
        if (Input.GetButton("Fire1"))
        {
            Debug.DrawRay(transform.position, shootHit.point - transform.position, Color.green);
        }
        else
        {
            //Debug.DrawRay(transform.position, shootHit.point - transform.position, Color.red);
        }
    }

    void Fire()
    {
        // Reset the timer.
        timer = 0f;

        // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
        //shootRay.origin = transform.position;
        shootRay.origin = camera.transform.position;        //fwd from camera
        shootRay.direction = transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
        {
            // Try and find an EnemyHealth script on the gameobject hit.
            //EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();
            TargetDummy targetDummy = shootHit.collider.GetComponent<TargetDummy>();

            // If the EnemyHealth component exist...
            if (targetDummy != null)
            {
                knockback = transform.forward * knockbackForce;

                // ... the enemy should take damage.
                targetDummy.TakeDamage(damagePerShot, knockback);
                PlayHitSounds();
            }

            //
            PlayerHealth playerHealth = shootHit.collider.GetComponent<PlayerHealth>();             //CLEAN UP LATER
            if (playerHealth != null)
            {
                knockback = transform.forward * knockbackForce;

                // ... the enemy should take damage.
                playerHealth.TakeDamage(damagePerShot, knockback);
                PlayHitSounds();
            }
            //

            // Impact effect
            Impact();

        }
        // If the raycast didn't hit anything on the shootable layer...
        else
        {
            // Endpoint = camera.fwd + max distance
        }
    }

    private void PlayHitSounds()
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