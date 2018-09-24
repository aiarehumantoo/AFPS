using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


// Shooting sync test


public class NetworkShoot : NetworkBehaviour
{
    [Header("Weapon Stats")]
    public int damagePerShot = 7;                         // The damage inflicted by each bullet.              lg = 7, rail = 30
    public float timeBetweenBullets = 1f;              // The time between each shot. QC LG firerate       lg = 0.055f, rail = 1.5f
    public float range = 100f;                             // The distance the gun can fire.
    Vector3 knockback;
    float knockbackForce = 5f;
    public int weaponSlot = 0;

    [Header("Projectile")]
    public bool projectile;
    public GameObject projectilePrefab;             // Prefab of the projectile
    float spawnDistance = 0.0f;                          // How far from the player projectile should spawn                         // Spawn distance is pointless with projectiles igonring the shooter. Might be needed for better visuals?
    public int projectileSpeed = 25;
    public float splashRadius;                                                                                                      // Currently using prefab size for explosion / splash size
    public int maximumSplashDamage;
    float projectileLifeTime = 2.0f;                // For deleting projectiles that hit nothing

    public float timer;

    Transform camera;

    void Start()
    {
        //get camera
        camera = transform.GetChild(0);

        timer = timeBetweenBullets; // Start without cooldown
    }

    // Update is called once per frame
    void Update ()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;


        if (Input.GetButton("Fire1") && timer >= timeBetweenBullets)        // && Time.timeScale != 0
        {
            CmdFire();
        }
	}

    [Command]
    void CmdFire()
    {
        // Reset the timer.
        timer = 0f;

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
        //projectileScript.parentScript = GetComponent<FireWeapon>();
        // Link player gameobject
        projectileScript.parentGameObject = transform.root.gameObject;                      //old stuff

        // Spawn the projectile on the Clients
        NetworkServer.Spawn(projectile);

        // Destroy the bullet after x seconds
        Destroy(projectile, projectileLifeTime);
    }
}
