using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectWeapon : MonoBehaviour
{
    //int selectedWeapon = 0;
    string currentWeapon = "Gauntlet";
    string clone = "(Clone)";

	// Use this for initialization
	void Start ()
    {
		
	}

    // Update is called once per frame
    void Update()
    {
        // Active weapon
        FireWeapon fireWeapon = this.transform.Find(currentWeapon +clone).GetComponent<FireWeapon>();

        if (fireWeapon.timer < fireWeapon.timeBetweenBullets || Input.GetButton("Fire1"))            // To change a weapon, current weapon must be ready to fire and player cannot be shooting
        {
            return;
        }

        if (Input.GetButton("Gauntlet"))
        {
            ChangeWeapon("Gauntlet");  
        }

        if (Input.GetButton("Plasma"))
        {
            ChangeWeapon("Plasma");
        }

        if (Input.GetButton("RL"))
        {
            ChangeWeapon("RocketLauncher");
        }

        if (Input.GetButton("LG"))
        {
            ChangeWeapon("LG");
        }

        if (Input.GetButton("Rail"))
        {
            ChangeWeapon("Rail");
        }
    }

    void ChangeWeapon(string weapon)
    {
        // If player doesnt have this weapon yet
        if(transform.Find(weapon +clone) == null)
        {
            return;
        }

        // Disable every weapon
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        currentWeapon = weapon;

        // Enable selected weapon
        this.transform.Find(weapon + clone).gameObject.SetActive(true);
    }
}
