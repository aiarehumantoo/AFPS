using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public GameObject player;           // Player prefab
    public Transform[] locations;       // Spawn locations

    private void Start()
    {
        RespawnPlayer();
    }

    public void RespawnPlayer()
    {
        // Spawn player at random spawn location
        int i = Random.Range(0, locations.Length);
        Instantiate(player, locations[i].transform.position, transform.rotation);
    }
}
