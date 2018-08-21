using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damagePerShot;

    Vector3 knockback;
    public float knockbackForce;

    public FireWeapon parentScript;



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
     * inherit stats from weapon
     * 
     */



    void OnTriggerEnter(Collider other)
    {
        var hit = other.gameObject;

        // Calculate knockback
        knockback = (transform.position - other.transform.position) * knockbackForce;

        // Deal damage                                                                          // Clean up + knockback based on where projectile hits
        TargetDummy targetDummy = hit.GetComponent<TargetDummy>();
        if (targetDummy != null)
        {
            //knockback = transform.forward * knockbackForce;

            // ... the enemy should take damage.
            targetDummy.TakeDamage(damagePerShot, knockback);
            //PlayHitSounds();
            parentScript.PlayHitSounds();
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
