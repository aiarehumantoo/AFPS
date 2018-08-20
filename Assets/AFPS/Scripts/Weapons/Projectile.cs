using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Transform camera;                                       //camera location for shooting

    public GameObject projectilePrefab;             // Prefab of the projectile
    int spawnDistance = 2;                          // How far from the player projectile should spawn
    int rocketSpeed = 25;

    float timer;                                    // A timer to determine when to fire.
    public float timeBetweenBullets = 0.055f;              // The time between each shot. QC LG firerate       lg = 0.055f, rail = 1.5f

    private void Awake()
    {
        timer = timeBetweenBullets; // Start without cooldown

        //get camera
        camera = transform.parent.parent;
    }



    //TODO
    // merge with hitscan --> single shooting code?





    void Update()
    {
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        // If the Fire1 button is being press and it's time to fire...
        if (Input.GetButton("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
        {
            // ... shoot the gun.
            Fire();
        }
    }

    void Fire()
    {
        // Reset the timer.
        timer = 0f;

        // Spawn point
        Vector3 projectileSpawn = camera.position + camera.transform.forward * spawnDistance;

        // Create the Bullet from the Bullet Prefab
        //var projectile = (GameObject)Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);
        var projectile = (GameObject)Instantiate(projectilePrefab, projectileSpawn, camera.rotation);

        // Add velocity to the bullet
        projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * rocketSpeed;

        // Spawn the bullet on the Clients
        //NetworkServer.Spawn(bullet);

        // Destroy the bullet after 10 seconds
        Destroy(projectile, 10.0f);
    }
}
