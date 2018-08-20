using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTest : MonoBehaviour
{
    public Transform target;
	
	// Update is called once per frame
	void Update ()
    {
        Debug.DrawRay(transform.position, target.transform.position - transform.position, Color.red);
    }
}
