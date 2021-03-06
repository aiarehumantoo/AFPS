﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Not used anymore. using overlapsphere in projectile.cs instead

public class ProjectileExplosion : MonoBehaviour
{
    public int splashDamage;

    Vector3 knockback;
    public float knockbackForce;

    //public FireWeapon parentScript;
    public PlayerFire parentScript;
    public GameObject parentGameObject;
    public GameObject directHit;

    void OnTriggerEnter(Collider other)             // Applied on first hit. calculate knockback based on distance to center of explosion.       !!Can bug out in certain situations if player is already inside the trigger and thus never enters it. maybe timestep issue, object disappears before next cycle?!!
    //void OnTriggerStay(Collider other)          // knockback applied on every timestep as long as player is within radius, no need to calculate distance based force. Remember to apply damage just once.
    {
        // Explosion hits player
        if (other.tag == "Player")
        {
            /*  TRYING SPLITTING PROJECTILE DAMAGE TO IMPACT AND SPLASH DAMAGE INSTEAD OF DEALING FULL DAMAGE ON DIRECT HIT AND THEN IGNORING THAT TARGET WHEN CALCULATING SPLASH DAMAGE
            // Ignore player that got hit directly by the projectile
            if (directHit == other.gameObject)
            {
                return;
            }
            */

            // Calculate knockback
            knockback = (other.transform.position - transform.position).normalized * knockbackForce;           // Reduce knockback if further away?
            
            // Calculate splash damage
            //splashDamage =

            // Deal damage
            TargetDummy targetDummy = other.gameObject.GetComponent<TargetDummy>();     // Dummy for testing
            if (targetDummy != null)
            {
                // ... the enemy should take damage.    (projectile splash damage)
                targetDummy.TakeDamage(splashDamage, knockback);

                // Hp drops to 0 after taking damage
                if (targetDummy.currentHealth <= 0)
                {
                    //parentScript.PlayHitSounds(true);
                }
                else
                {
                    //parentScript.PlayHitSounds(false);
                }
            }

            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // ... the enemy should take damage.
                playerHealth.TakeDamage(splashDamage, knockback);
                //playerHealth.TakeDamage(0, knockback);                      // 0 damage to players, for testin rocket jumps

                if (parentGameObject != other.transform.gameObject) // No hitsounds if self damage
                {
                    // Hp drops to 0 after taking damage
                    if (playerHealth.currentHealth <= 0)
                    {
                        //parentScript.PlayHitSounds(true);
                    }
                    else
                    {
                        //parentScript.PlayHitSounds(false);
                    }
                }

            }
        }
    }
}
