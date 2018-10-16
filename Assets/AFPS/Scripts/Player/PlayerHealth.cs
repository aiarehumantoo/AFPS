using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;   // Networking namespace

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health")]
    public int startingHealth = 125;    // Health at spawn
    public int maxHealth = 100;         // Maximum health
    public int maxOverHeal = 200;          // Maximum overheal

    [SyncVar]
    public int currentHealth;           // Current health

    [Header("Armor")]
    public int startingArmor = 50;
    public int maxArmor = 100;
    public int maxOverArmor = 200;      // better name?
    public int armorDamageReduction = 2;       // Starting with 50% reduction. Will need some tweaking

    [SyncVar]
    public int currentArmor;        // current armor

    [Header("Stack decay")]
    int decayRate = 2;               // How much stacks decay each tick
    float decayTick = 2.0f;         // Delay between each tick

    float timer;
    bool isDead = false;

    CapsuleCollider capsuleCollider;            // Reference to the capsule collider.
    public GUIStyle style;

    private NetworkStartPosition[] spawnPoints;

    //private SelectWeapon selectWeapon;
    PlayerFire playerFire;

    // pointless?
    Rigidbody rb;
    private CharacterController _controller;
    private Vector3 playerVelocity = Vector3.zero;






    // TODO NOTES

    /*
     * Update TakeDamage() 
     *      Use float instead of int to get rid of cutoff
     *      ie. 7 / 2 = 3.5
     *      but int displays result as 3
     *      
     * 
     */

    void Start()
    {
        if (isLocalPlayer)
        {
            spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        }
    }


    void Awake()
    {
        // Setting up the references.
        capsuleCollider = GetComponent<CapsuleCollider>();

        rb = GetComponent<Rigidbody>();
        _controller = GetComponent<CharacterController>();

        //selectWeapon = transform.GetChild(0).GetChild(0).GetComponent<SelectWeapon>();
        playerFire = GetComponent<PlayerFire>();

        // Health at spawn
        //currentHealth = startingHealth;
        //currentArmor = startingArmor;
        currentArmor = 0;                           //TESTING
        currentHealth = 100;
        isDead = false;
    }

    private void Update()
    {
        if (!isServer)
        {
            return;
        }

        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        StackDecay();
    }

    public void TakeDamage(int amount, Vector3 knockback)
    {
        // Damage only applied on the server
        if (!isServer)
        {
            return;
        }

        // If the player is dead...
        if (isDead)
        {
            // ... no need to take damage so exit the function.
            return;
        }

        /*                                                              disabled armor for testing purposes. health taking dmg directly
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
        */

        // Reduce the current health by the amount of damage sustained.
        currentHealth -= amount;

        // Apply knockback
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.KnockBack(knockback);    // Doing knockback in movement code
        }

        // If the current health is less than or equal to zero...
        if (currentHealth <= 0)
        {
            // ... the player is dead.
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
        /*
        Destroy(this.gameObject);                       // just destroy for now. TODO death anim etc later.
        //Debug.Log("Player health < 0");

        GameObject respawn = GameObject.Find("RespawnSystem");
        respawn.GetComponent<Respawn>().RespawnPlayer();
        */

        // Respawn with starting weapon(s)
        //selectWeapon.Respawn();
        playerFire.CmdRespawn();

        // Health at spawn
        currentHealth = startingHealth;
        currentArmor = startingArmor;
        isDead = false;

        // called on the Server, but invoked on the Clients
        RpcRespawn();
    }

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            // Set the spawn point to origin as a default value
            Vector3 spawnPoint = Vector3.zero;

            // If there is a spawn point array and the array is not empty, pick one at random
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
            }

            // Set the player’s position to the chosen spawn point
            transform.position = spawnPoint;
        }
    }

    private void OnGUI()
    {
        if (isLocalPlayer)
        {
            GUI.Label(new Rect(Screen.width / 25, Screen.height - Screen.height / 10, 400, 100), "Health: " + currentHealth, style);
            GUI.Label(new Rect(Screen.width / 25, Screen.height - Screen.height / 12, 400, 100), "Armor: " + currentArmor, style);
        }
    }
}
