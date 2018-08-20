using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    int damagePerShot = 100;     // Damage
    Vector3 knockback = Vector3.zero;



    //TODO
    /*
     * Same properties as for hitscan weapons
     *      "enemy" & "environment" layers
     *      
     * Audio component (hitsound) in parent (weapon)?
     *      trigger when projectile hits enemy
     * 
     * Splash damage & sfx
     * 
     */



    void OnTriggerEnter(Collider other)
    {
        var hit = other.gameObject;

        // Deal damage                                                                          // Clean up + knockback based on where projectile hits
        TargetDummy targetDummy = hit.GetComponent<TargetDummy>();
        if (targetDummy != null)
        {
            //knockback = transform.forward * knockbackForce;

            // ... the enemy should take damage.
            targetDummy.TakeDamage(damagePerShot, knockback);
            //PlayHitSounds();
        }
        PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            //knockback = transform.forward * knockbackForce;

            // ... the enemy should take damage.
            playerHealth.TakeDamage(damagePerShot, knockback);
            //PlayHitSounds();
        }

        Destroy(gameObject);
    }
}
