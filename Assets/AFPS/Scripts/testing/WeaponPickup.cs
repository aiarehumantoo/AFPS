using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public GameObject weapon;        //weapon prefab
    public GameObject pickup;        //pickup prefab
    public int weaponSlot;          //slot the weapon belongs to. 0 = gauntlet, 1 = lg, 2 = rl, 3 = rail

    GameObject pickupAnim;

    bool once = false;  //for testing

    float timer;
    public float cooldown = 5.0f;          // cooldown

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
        if (cooldown <= timer)
        {
            // Show item
            pickupAnim.SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Weapon has respawned
        if (other.tag == "Player" && cooldown <= timer)
        {
            // Reset the timer
            timer = 0f;

            // Hide pickup item during cooldown
            pickupAnim.SetActive(false);

            // Search player/camera/weapons and give weapon if player doenst have one yet
            if (!other.transform.GetChild(0).GetChild(0).transform.Find(weapon.name + "(Clone)"))
            {
                //spawn weapon as a child object of player/camera/weapons (and in correct weapon slot)
                GameObject GO = Instantiate(weapon);
                GO.transform.parent = other.transform.GetChild(0).GetChild(0);
                GO.transform.position = GO.transform.parent.position;
                GO.transform.rotation = GO.transform.parent.rotation;
                GO.gameObject.transform.SetSiblingIndex(weaponSlot);
            }
            else
            {
                //give ammo
            }
        }
    }
}