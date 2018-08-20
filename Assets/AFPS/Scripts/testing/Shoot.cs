using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Shoot : MonoBehaviour
{
    // Rail is default weapon.
    int fireMode = 0;                               // 0 = rail, 1 = LG
    public string activeWeapon = "rail";
    int damagePerShot = 100;                        // The damage inflicted by each bullet.
    float timeBetweenBullets = 1f;                  // The time between each shot.
    float range = 100f;                             // The distance the gun can fire.
    bool singleShot = true;                         // Delete weapon effects with delay if rail and on Mouse1 release if LG.

    float timer;                                    // A timer to determine when to fire.
    Ray shootRay = new Ray();                       // A ray from the gun end forwards.
    RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
    int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.
                                                    //ParticleSystem gunParticles;                    // Reference to the particle system.
    LineRenderer gunLine;                           // Reference to the line renderer.
                                                    //AudioSource gunAudio;                           // Reference to the audio source.
    Light gunLight;                                 // Reference to the light component.
    public Light faceLight;                             // Duh
    float effectsDisplayTime = 0.2f;                // The proportion of the timeBetweenBullets that the effects will display for.

    void Awake()
    {
        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy");

        // Set up the references.
        //gunParticles = GetComponent<ParticleSystem>();
        gunLine = GetComponent<LineRenderer>();
        //gunAudio = GetComponent<AudioSource>();
        gunLight = GetComponent<Light>();
        faceLight = GetComponentInChildren<Light>();
    }

    void Update()
    {
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;
        
        // If the Fire1 button is being press and it's time to fire...
        if (Input.GetButton("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
        {
            // ... shoot the gun.
            //Fire();
        }

        //cycle weapons.
        if (Input.GetButtonDown("CycleWeapons"))
        {
            //next weapon.
            CycleWeapons();
        }

        // Disable weapon effects with delay after firing.
        if (timer >= timeBetweenBullets * effectsDisplayTime)
        {
            // ... disable the effects.
            DisableEffects();
        }
    }

    // Rail --> disable all. LG --> Wait for button release before disabling lights to prevent flicker.
    public void DisableEffects()
    {
        if (singleShot == true)
        {
            gunLine.enabled = false;
            faceLight.enabled = false;
            gunLight.enabled = false;
        }
        else
        {
            gunLine.enabled = false;
            if (Input.GetButtonUp("Fire1"))
            {
                faceLight.enabled = false;
                gunLight.enabled = false;
            }
        }
    }

    /*
    void Fire()
    {
        // Reset the timer.
        timer = 0f;

        // Play the gun shot audioclip.
        //gunAudio.Play();

        // Enable the lights.
        gunLight.enabled = true;
        faceLight.enabled = true;

        // Stop the particles from playing if they were, then start the particles.
        //gunParticles.Stop();
        //gunParticles.Play();

        // Enable the line renderer and set it's first position to be the end of the gun.
        gunLine.enabled = true;
        gunLine.SetPosition(0, transform.position);

        // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
        shootRay.origin = transform.position;
        shootRay.direction = transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
        {
            // Try and find an EnemyHealth script on the gameobject hit.
            EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

            // If the EnemyHealth component exist...
            if (enemyHealth != null)
            {
                // ... the enemy should take damage.
                enemyHealth.TakeDamage(damagePerShot, shootHit.point);
            }

            // Set the second position of the line renderer to the point the raycast hit.
            gunLine.SetPosition(1, shootHit.point);
        }
        // If the raycast didn't hit anything on the shootable layer...
        else
        {
            // ... set the second position of the line renderer to the fullest extent of the gun's range.
            gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
        }
    }
    */

    void CycleWeapons()
    {
        //TODO; make into cycle through list of weapons. Config file for weapons ?
        if (fireMode == 0)
        {
            //LG
            fireMode = 1;
            activeWeapon = "LG";
            damagePerShot = 1;                  // The damage inflicted by each bullet.
            timeBetweenBullets = 0.01f;         // The time between each shot.
            range = 100f;                       // The distance the gun can fire.
            singleShot = false;                 // For disabling weapon effects.
            return;
        }

        if (fireMode == 1)
        {
            //rail
            fireMode = 0;
            activeWeapon = "rail";
            damagePerShot = 100;                // The damage inflicted by each bullet.
            timeBetweenBullets = 1f;            // The time between each shot.
            range = 100f;                       // The distance the gun can fire.
            singleShot = true;                  // For disabling weapon effects.
            return;
        }
    }
}