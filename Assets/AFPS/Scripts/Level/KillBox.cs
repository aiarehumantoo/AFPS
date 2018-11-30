using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    public bool kill;
    float timer;
    float damageTick = 2.0f;
    public int damagePerTick = 2;


    void OnTriggerStay(Collider other)
    {
        if(kill)    // Kill player. Should just directly set player to death state but whatever
        {
            if (other.tag == "Player")
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(999, Vector3.zero);
                }

            }
        }
        else // Damage player
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
}
