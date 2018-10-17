using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damagePerShot;
    public int splashDamage;
    public float splashRadius;

    Vector3 knockback;
    public float knockbackForce;

    public PlayerFire parentScript;
    public GameObject parentGameObject;

    public GameObject explosionPrefab;

    void OnTriggerEnter(Collider other)
    {
        // Ignore player that shot the projectile
        if (parentGameObject == other.gameObject)
        {
            return;
        }

        // Direct hit
        if (other.tag == "Player")
        {
            // Calculate knockback (direct hit)                           // splash damage knockback = vector between origin and target (normalized)? (or without normalizing knockback will vary depending on distance. needs to be reversed tho for knockback to be stronger the closer it hits.
            knockback = transform.forward.normalized * knockbackForce;  // Direction projectile is moving * force                                            

            // Deal damage
            TargetDummy targetDummy = other.gameObject.GetComponent<TargetDummy>();         // Target dummy for testing
            if (targetDummy != null)
            {
                // ... the enemy should take damage.
                //targetDummy.TakeDamage(damagePerShot, knockback);
                targetDummy.TakeDamage(damagePerShot, Vector3.zero);        // See below

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
                // ... the enemy should take damage.    (projectile impact damage)
                //playerHealth.TakeDamage(damagePerShot, knockback);
                playerHealth.TakeDamage(damagePerShot, Vector3.zero);                   // Testing impact + splash damage. Not ignoring targets hit directly by the projectile --> dealing knockback just once (splash)

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

            // Spawn explosion
            //Explosion(other.gameObject);        // other.transform.root.gameobject if there are other colliders besides root (player)

            // Deal explosion damage
            ExplosionDamage(transform.position, splashRadius);
        }

        // Hits terrain 
        if(other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            // Spawn explosion
            //Explosion(null);

            // Deal explosion damage
            ExplosionDamage(transform.position, splashRadius);
        }

        // Destroy projectile
        Destroy(gameObject);
    }

    void Explosion(GameObject direct)
    {
        // Spawn explosion
        var explosion = (GameObject)Instantiate(explosionPrefab, transform.position, transform.rotation);

        // Explosion stats
        ProjectileExplosion explosionScript = explosion.GetComponent<ProjectileExplosion>();
        explosionScript.splashDamage = splashDamage;
        explosionScript.knockbackForce = knockbackForce;

        // Link parent script
        explosionScript.parentScript = parentScript;
        // Link player gameobject
        explosionScript.parentGameObject = parentGameObject;

        // Player that got hit directly
        explosionScript.directHit = direct;

        // Spawn the explosion on the Clients                                       // No need to do this since projectiles are client side after spawning?
        //NetworkServer.Spawn(projectile);

        // Destroy the explosion after x seconds
        Destroy(explosion, 0.25f);
    }

    void ExplosionDamage(Vector3 center, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        int i = 0;
        while (i < hitColliders.Length)
        {
            // Calculate knockback
            knockback = (hitColliders[i].transform.position - transform.position).normalized * knockbackForce;           // Reduce knockback if further away?

            // Calculate splash damage
            //splashDamage =

            // Deal damage
            PlayerHealth playerHealth = hitColliders[i].gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // ... the enemy should take damage.
                playerHealth.TakeDamage(splashDamage, knockback);
                //playerHealth.TakeDamage(0, knockback);                      // 0 damage to players, for testin rocket jumps

                if (parentGameObject != hitColliders[i].transform.gameObject) // No hitsounds if self damage
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
            i++;
        }
    }

    /*
    //Display overlapsphere
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
    */
}
