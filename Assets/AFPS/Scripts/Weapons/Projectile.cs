using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damagePerShot;
    public int splashDamage;

    Vector3 knockback;
    public float knockbackForce;

    public FireWeapon parentScript;
    public GameObject parentGameObject;

    public GameObject explosionPrefab;



    //TODO
    /*
     * Same properties as for hitscan weapons
     *      "enemy" & "environment" layers
     *      
     * 
     * Splash damage & sfx
     * 
     * 
     * Hit detection is bit lacking. sometimes explosion spawns inside a wall
     *
     * 
     */



    void OnTriggerEnter(Collider other)
    {
        // Direct hit
        if (other.tag == "Player")
        {
            // Ignore player that shot the projectile
            if(parentGameObject == other.gameObject)
            {
                return;
            }

            // Calculate knockback (direct hit)                           // splash damage knockback = vector between origin and target (normalized)? (or without normalizing knockback will vary depending on distance. needs to be reversed tho for knockback to be stronger the closer it hits.
            knockback = transform.forward.normalized * knockbackForce;  // Direction projectile is moving * force                                            

            // Deal damage
            TargetDummy targetDummy = other.gameObject.GetComponent<TargetDummy>();
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
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
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
        }

        // Hits terrain                                                                                                             // NO SPLASH DAMAGE ON DIRECT HITS
        if(other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            // Spawn explosion
            var explosion = (GameObject)Instantiate(explosionPrefab, transform.position, transform.rotation);

            // Explosion stats
            ProjectileExplosion explosionScript = explosion.GetComponent<ProjectileExplosion>();
            //explosionScript.damagePerShot = damagePerShot;
            explosionScript.splashDamage = splashDamage;
            explosionScript.knockbackForce = knockbackForce;

            // Link parent script
            explosionScript.parentScript = parentScript;
            // Link player gameobject
            explosionScript.parentGameObject = parentGameObject;

            // Spawn the explosion on the Clients
            //NetworkServer.Spawn(projectile);

            // Destroy the explosion after x seconds
            Destroy(explosion, 0.25f);
        }

        // Destroy projectile
        Destroy(gameObject);
    }
}
