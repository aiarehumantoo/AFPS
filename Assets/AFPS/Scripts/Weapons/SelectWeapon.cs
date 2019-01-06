using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;   // Networking namespace



// old --> PlayerFire.cs


public class SelectWeapon : NetworkBehaviour
{
    //int selectedWeapon = 0;
    string currentWeapon = "Gauntlet";
    string clone = "(Clone)";

    public GameObject startingWeapon;

    string weaponString;

    void Start()
    {
        StartingWeapon();
    }

    void StartingWeapon()
    {
        // Give starting weapon (gauntlet)
        GameObject GO = Instantiate(startingWeapon);
        GO.transform.parent = this.transform;
        GO.transform.position = GO.transform.parent.position;
        GO.transform.rotation = GO.transform.parent.rotation;

        CmdStartingWeapon();
    }

    [Command]
    void CmdStartingWeapon()
    {
        // Spawn on the Clients
        NetworkServer.Spawn(startingWeapon);
        //NetworkServer.Spawn(GO);
    }

    /*
    void OnPlayerConnected(NetworkPlayer player)        //obsolete. use unity multiplayer and NetworkIdentity instead
    {
        //NetworkServer.Spawn(startingWeapon);
    }
    */

    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer)
        {
            //return;
        }

        // Active weapon
        FireWeapon fireWeapon = this.transform.Find(currentWeapon +clone).GetComponent<FireWeapon>();

        if (fireWeapon.timer < fireWeapon.timeBetweenBullets || Input.GetButton("Fire1"))            // To change a weapon, current weapon must be ready to fire and player cannot be shooting
        {
            return;
        }

        if (Input.GetButton("Gauntlet"))
        {
            //ChangeWeapon("Gauntlet");
            weaponString = "Gauntlet";
        }

        if (Input.GetButton("Plasma"))
        {
            //ChangeWeapon("Plasma");
            weaponString = "Plasma";
        }

        if (Input.GetButton("RL"))
        {
            //ChangeWeapon("RocketLauncher");
            weaponString = "RocketLauncher";
        }

        if (Input.GetButton("LG"))
        {
            //ChangeWeapon("LG");
            weaponString = "LG";
        }

        if (Input.GetButton("Rail"))
        {
            //ChangeWeapon("Rail");
            weaponString = "Rail";
        }

        ChangeWeapon(weaponString);
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
        transform.Find(weapon + clone).gameObject.SetActive(true);

    }

    public void Respawn()
    {
        // Respawn with only gauntlet
        for (int i = 0; i < transform.childCount; i++)
        {
            // Destroy if not starting weapon
            if (transform.GetChild(i).gameObject.name != startingWeapon.name + clone)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        // Select starting weapon
        ChangeWeapon("Gauntlet");
    }
}
