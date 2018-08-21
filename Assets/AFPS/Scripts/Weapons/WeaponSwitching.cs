using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;      //public just to see value in editor

	// Use this for initialization
	void Start ()
    {
        SelectWeapon();
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Active weapon
        FireWeapon fireWeapon = this.transform.GetChild(selectedWeapon).GetComponent<FireWeapon>();

        if (fireWeapon.timer < fireWeapon.timeBetweenBullets || Input.GetButton("Fire1"))            // To change a weapon, current weapon must be ready to fire and player cannot be shooting
        {
            return;
        }


        int previousSelectedWeapon = selectedWeapon;

		if(Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeapon >= transform.childCount - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon++;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedWeapon <= 0)
            {
                selectedWeapon = transform.childCount - 1;
            }
            else
            {
                selectedWeapon--;
            }
        }

        if(previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    void SelectWeapon()
    {
        int i = 0;
        foreach(Transform weapon in transform)
        {
            if(i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
    }
}
