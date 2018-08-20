using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public GameObject pickup;        //pickup prefab
    public int itemType;                // 0 = health, 1 = armor
    public int amount;
    public bool overheal;

    GameObject pickupAnim;

    float timer;       
    public float cooldown;          // 10s for health bubbles / armor shards, 30s for  large armor shard, 60s for mega / large armor

    private void Awake()
    {
        pickupAnim = Instantiate(pickup);
        pickupAnim.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

        timer = cooldown;   // Start without cooldown
    }

    private void Update()
    {
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        // Cooldown is over
        if(cooldown <= timer)
        {
            // Show item
            pickupAnim.SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // item has respawned
        if (other.tag == "Player" && cooldown <= timer)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Return if player cant pick up this item
                if (itemType == 0 && !overheal && playerHealth.currentHealth >= playerHealth.maxHealth)
                {
                    return;
                }
                if (itemType == 1 && !overheal && playerHealth.currentArmor >= playerHealth.maxArmor)
                {
                    return;
                }

                // Reset the timer
                timer = 0f;

                // Hide pickup item during cooldown
                pickupAnim.SetActive(false);

                // Give health/armor
                playerHealth.ItemPickup(itemType, amount, overheal);
            }
        }
    }
}
