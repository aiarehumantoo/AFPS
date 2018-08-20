using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBox : MonoBehaviour
{
    float timer;
    float damageTick = 2.0f;
    public int damagePerTick = 2;

    void OnTriggerStay(Collider other)
    {
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        // player touching + dmg tick
        if (other.tag == "Player" && damageTick <= timer)
        {
            // Reset the timer
            timer = 0f;

            // Damage player inside the trigger
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damagePerTick, Vector3.zero);   // damage without knockback
            }

        }
    }
}
