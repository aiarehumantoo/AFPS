using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject ob;

	// Use this for initialization
	void Start ()
    {
        GameObject derp = Instantiate(ob);
        derp.transform.position = transform.position;
    }
}
