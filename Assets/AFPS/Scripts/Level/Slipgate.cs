using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slipgate : MonoBehaviour
{
    public Transform location;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.transform.position = location.transform.position;
        }
    }
}
