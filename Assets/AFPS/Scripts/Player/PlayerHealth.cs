﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // Health
    public int startingHealth = 125;    // Health at spawn
    public int maxHealth = 100;         // Maximum health
    public int maxOverHeal = 200;          // Maximum overheal
    public int currentHealth;           // Current health

    // Armor
    public int startingArmor = 50;
    public int maxArmor = 100;
    public int maxOverArmor = 200;      // better name?
    public int currentArmor;
    public int armorDamageReduction = 2;       // Starting with 50% reduction. Will need some tweaking

    // Stack decay
    float timer;
    int decayRate = 2;               // How much stacks decay each tick
    float decayTick = 2.0f;         // Delay between each tick

    bool isDead = false;

    CapsuleCollider capsuleCollider;            // Reference to the capsule collider.
    Rigidbody rb;

    private CharacterController _controller;

    private Vector3 playerVelocity = Vector3.zero;

    public GUIStyle style;





    // TODO NOTES

    /*
     * Update TakeDamage() 
     *      Use float instead of int to get rid of rounding
     *      ie. 7 / 2 = 3.5
     *      but int displays result as 3
     *      
     * 
     */


    void Awake()
    {
        // Setting up the references.
        capsuleCollider = GetComponent<CapsuleCollider>();

        rb = GetComponent<Rigidbody>();

        _controller = GetComponent<CharacterController>();

        // Health at spawn
        currentHealth = startingHealth;
        currentArmor = startingArmor;
        isDead = false;
    }

    private void Update()
    {
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        StackDecay();
    }

    public void TakeDamage(int amount, Vector3 knockback)
    {
        // If the enemy is dead...
        if (isDead)
        {
            // ... no need to take damage so exit the function.
            return;
        }

        // if player has enough armor
        if(currentArmor >= amount / armorDamageReduction)
        {
            // Take armor damage
            currentArmor -= amount / armorDamageReduction;
            // Remaining damage
            amount = amount / armorDamageReduction;  
        }
        else if(currentArmor > 0)   // Player has some armor left but not enough for full damage reduction
        {
            // Reduce damage by the amount of armor left
            amount = amount - currentArmor;
            // Armor left drops to zero
            currentArmor = 0;
        }

        // Reduce the current health by the amount of damage sustained.
        currentHealth -= amount;

        // Apply knockback
        playerVelocity += knockback * Time.deltaTime;
        _controller.Move(playerVelocity * Time.deltaTime);

        // If the current health is less than or equal to zero...
        if (currentHealth <= 0)
        {
            // ... the enemy is dead.
            isDead = true;
            Death();
        }
    }

    public void ItemPickup(int type, int amount, bool overheal)                     //TODO: cleanup. same code, change variables? ie. currentStat --> currentHealth or CurrentArmor
    {
        if(type == 0)   // Health
        {
            if (overheal)    // Mega
            {
                if(currentHealth <= maxOverHeal - amount)
                {
                    currentHealth += amount;
                }
                else
                {
                    currentHealth = maxOverHeal;
                }
            }
            else // bubbles
            {
                if (currentHealth <= maxHealth - amount)
                {
                    currentHealth += amount;
                }
                else if (currentHealth < maxHealth)
                {
                    currentHealth = maxHealth;
                }
            }
        }

        if(type == 1)   // Armor
        {
            if (overheal)    // Large armor
            {
                if (currentArmor <= maxOverArmor - amount)
                {
                    currentArmor += amount;
                }
                else
                {
                    currentArmor = maxOverArmor;
                }
            }
            else // shards
            {
                if (currentArmor <= maxArmor - amount)
                {
                    currentArmor += amount;
                }
                else if (currentArmor < maxArmor)
                {
                    currentArmor = maxArmor;
                }
            }
        }
    }

    void StackDecay()   // Decay Health / Armor stack if over maximum
    {
        if (timer >= decayTick)
        {
            // Reset the timer
            timer = 0;

            // Decay only until max health
            if (currentHealth > maxHealth + decayRate)
            {
                currentHealth -= decayRate;
            }
            else if(currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            // Decay only until max armor
            if (currentArmor > maxArmor + decayRate)
            {
                currentArmor -= decayRate;
            }
            else if(currentArmor > maxArmor)
            {
                currentArmor = maxArmor;
            }
        }
    }


    void Death()
    {
        Destroy(this.gameObject);                       // just destroy for now. TODO death anim etc later.
        //Debug.Log("Player health < 0");

        GameObject respawn = GameObject.Find("RespawnSystem");
        respawn.GetComponent<Respawn>().RespawnPlayer();
    }

    // Respawn enemy when killed
    void OnDestroy()
    {
        //GameObject respawn = GameObject.Find("RespawnSystem");
        //respawn.GetComponent<Respawn>().RespawnPlayer();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 25, Screen.height - Screen.height / 10, 400, 100), "Health: " + currentHealth, style);
        GUI.Label(new Rect(Screen.width / 25, Screen.height - Screen.height / 12, 400, 100), "Armor: " + currentArmor, style);
    }
}
