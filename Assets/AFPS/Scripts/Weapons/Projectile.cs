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
     * 
     * Splash damage & sfx
     * 
     *
     * 
     */



    void OnTriggerEnter(Collider other)
    {
        var hit = other.gameObject;

        // Calculate knockback
        knockback = transform.forward.normalized * knockbackForce;  // Direction projectile is moving * force

        // Deal damage
        TargetDummy targetDummy = hit.GetComponent<TargetDummy>();
        if (targetDummy != null)
        {
            // ... the enemy should take damage.
            targetDummy.TakeDamage(damagePerShot, knockback);

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
        PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // ... the enemy should take damage.
            playerHealth.TakeDamage(damagePerShot, knockback);

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

        Destroy(gameObject);
    }
}
