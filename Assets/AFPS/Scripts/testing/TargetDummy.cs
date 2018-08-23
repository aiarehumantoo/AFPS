using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TargetDummy : MonoBehaviour
{
    public int startingHealth = 100;            // The amount of health the enemy starts the game with.
    public int currentHealth;                   // The current health the enemy has.
    bool isDead = false;

    CapsuleCollider capsuleCollider;            // Reference to the capsule collider.
    Rigidbody rb;

    void Awake()
    {
        // Setting up the references.
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

        // Setting the current health when the enemy first spawns.
        currentHealth = startingHealth;
        isDead = false;
    }

    //public void TakeDamage(int amount, Vector3 hitPoint)
    public void TakeDamage(int amount, Vector3 knockback)
    {
        // If the enemy is dead...
        if (isDead)
        {
            // ... no need to take damage so exit the function.
            return;
        }

        // Reduce the current health by the amount of damage sustained.
        currentHealth -= amount;

        // Apply knockback
        rb.AddForce(knockback * 10);     // works for test dummy, but players are kinematic. Needs other way to apply knockback force

        // If the current health is less than or equal to zero...
        if (currentHealth <= 0)
        {
            // ... the enemy is dead.
            isDead = true;
            Death();
        }
    }

    void Death()
    {
        Destroy(gameObject);        // just destroy for now. TODO death anim etc later.
    }
}