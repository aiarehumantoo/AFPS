using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float force;     // Jump pad force
    Vector3 direction;

    void OnTriggerEnter(Collider other)
    {
        /*
        // Using Knockback for jumppad force
        if (other.tag == "Player")
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                direction = transform.up.normalized;
                playerHealth.TakeDamage(0, direction * force);
            }
        }
        */

        // Set player velocity
        if (other.tag == "Player")
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                direction = transform.up.normalized;
                playerMovement.JumpPad(direction * force);
            }
        }
    }
}
