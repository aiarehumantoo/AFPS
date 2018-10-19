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

    bool debugSplash;   // For displaying explosion/splash radius in debug mode (enable gizmos)

    void OnTriggerEnter(Collider other)
    {
        // Ignore player that shot the projectile
        if (parentGameObject == other.gameObject)
        {
            return;
        }

        // Direct hit on player collider
        if (other.tag == "Player" && other is CapsuleCollider)
        {
            // Calculate knockback (direct hit)
            knockback = transform.forward.normalized * knockbackForce;  // Direction projectile is moving * force                                      

            // Deal damage, zero knockback on direct hit. Adding knockback only in splash.
            HitDummy(other, damagePerShot, knockback); // Dummy
            HitPlayer(other, damagePerShot, knockback); // Player, dealing projectile impact damage without any knockback

            // Deal explosion damage
            ExplosionDamage(transform.position, splashRadius, other);
        }

        // Hits terrain 
        if(other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            // Deal explosion damage
            ExplosionDamage(transform.position, splashRadius, null);
        }

        // Destroy projectile
        //Destroy(gameObject);
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero; // stopping projectile instead of destroying it. for debugging. projectile is destroyed anyway when its lifetime expires
    }

    void ExplosionDamage(Vector3 center, float radius, Collider other)
    {
        debugSplash = true;     // Debug, display explosion radius

        Collider[] hitColliders = Physics.OverlapSphere(center, radius);    // Get colliders within splash radius
        for(int i = 0; i < hitColliders.Length; i++)    // Repeat for every hit collider
        {
            // Hits CapsuleCollider of player. (Otherwise CharacterController would count as hit too, resulting in double damage)
            if (hitColliders[i].tag == "Player" && hitColliders[i] is CapsuleCollider)
            {
                // Add line of sight check? (Raycast to all targets)                splash ignoring map geometry could work too. Remember to ignore players in line of sight check.

                if (other == hitColliders[i])   // direct hit, no additional knockback from splash
                {
                    Debug.Log("direct splash");
                    knockback = Vector3.zero;
                }
                else
                {
                    Debug.Log("normal splash");
                    knockback = (hitColliders[i].transform.position - transform.position).normalized * knockbackForce;           // Reduce knockback if further away?
                }

                // Calculate splash damage
                //splashDamage =

                // Deal damage
                HitDummy(hitColliders[i], splashDamage, knockback);
                HitPlayer(hitColliders[i], splashDamage, knockback);
            }
        }
    }   

    void HitPlayer(Collider other, int dmg, Vector3 force)
    {
        // If the Health component exists...
        PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Deal damage
            playerHealth.TakeDamage(dmg, force);

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

    //Display overlapsphere, remember to enable gizmos in play mode
    private void OnDrawGizmos()
    {
        if (debugSplash)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, splashRadius);
        }
    }

    void HitDummy(Collider other, int dmg, Vector3 force)
    {
        TargetDummy targetDummy = other.gameObject.GetComponent<TargetDummy>();         // Target dummy for testing
        if (targetDummy != null)
        {
            // Deal damage
            targetDummy.TakeDamage(dmg, force);

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
    }

    // old way of creating explosion. spawns sphere and collider of that sphere is used for hit detection
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
}