using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHP : MonoBehaviour
{
    // Set player hp to 100/0. For easier damage testing.
    public int hp = 100;
    public int armor = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.currentHealth = hp;
                playerHealth.currentArmor = armor;
            }
        }
    }
}
