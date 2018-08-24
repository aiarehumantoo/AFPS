﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileExplosion : MonoBehaviour
{
    public int splashDamage;

    Vector3 knockback;
    public float knockbackForce;

    public FireWeapon parentScript;
    public GameObject parentGameObject;

    void OnTriggerEnter(Collider other)
    {
        // Explosion hits player
        if (other.tag == "Player")
        {
            // Calculate knockback
            knockback = (other.transform.position - transform.position).normalized * knockbackForce;           // Reduce knockback if further away?
            
            // Calculate splash damage
            //splashDamage =

            // Deal damage
            TargetDummy targetDummy = other.gameObject.GetComponent<TargetDummy>();
            if (targetDummy != null)
            {
                // ... the enemy should take damage.
                targetDummy.TakeDamage(splashDamage, knockback);

                // Hp drops to 0 after taking damage
                if (targetDummy.currentHealth <= 0)
                {
                    parentScript.PlayHitSounds(true);
                }
                else
                {
                    parentScript.PlayHitSounds(false);
                }
            }
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // ... the enemy should take damage.
                //playerHealth.TakeDamage(splashDamage, knockback);
                playerHealth.TakeDamage(0, knockback);                                      // 0 damage to players, for testing rocket jumps

                if (parentGameObject != other.transform.gameObject) // No hitsounds if self damage
                {
                    // Hp drops to 0 after taking damage
                    if (playerHealth.currentHealth <= 0)
                    {
                        parentScript.PlayHitSounds(true);
                    }
                    else
                    {
                        parentScript.PlayHitSounds(false);
                    }
                }

            }
        }
    }
}