using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RocketTest : NetworkBehaviour
{
    [SyncVar]
    public int damagePerShot = 100;                 
    public float timeBetweenShots = 0.8f;                                    
    //Vector3 knockback;
    float knockbackForce = 5f;

    //public bool projectile;
    public GameObject projectilePrefab;           
    float spawnDistance = 0.0f;                         
    public int projectileSpeed = 25;
    public int maximumSplashDamage = 100;
    float projectileLifeTime = 2.0f;               

    public float timer;

    Transform camera;

    void Start()
    {
        //get camera
        camera = transform.GetChild(0);

        if (!isLocalPlayer)
        {
            return;
        }

        gameObject.name = "Local Player";

        //get camera
        //camera = transform.GetChild(0);

        timer = timeBetweenShots; // Start without cooldown
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;


        if (Input.GetButton("Fire1") && timer >= timeBetweenShots)
        {
            timer = 0f;
            CmdFireProjectile();
        }
    }

    [Command]
    void CmdFireProjectile()
    {
        // Spawn point
        Vector3 projectileSpawn = camera.position + camera.transform.forward.normalized * spawnDistance;

        // Create the projectile from the Prefab
        var projectile = (GameObject)Instantiate(projectilePrefab, projectileSpawn, camera.rotation);

        // Add velocity to the projectile
        projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileSpeed;

        // Other projectile stats
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.damagePerShot = damagePerShot;
        projectileScript.knockbackForce = knockbackForce;
        projectileScript.splashDamage = maximumSplashDamage;

        // Link this script
        projectileScript.parentScript = GetComponent<PlayerFire>();
        // Link player gameobject
        projectileScript.parentGameObject = transform.gameObject;

        // Spawn the projectile on the Clients
        NetworkServer.Spawn(projectile);

        // Destroy the projectile after x seconds
        Destroy(projectile, projectileLifeTime);
    }
}
