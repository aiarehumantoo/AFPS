using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTest : MonoBehaviour
{
    public Transform endPoint;
    LineRenderer gunLine;                           // Reference to the line renderer.

    // Use this for initialization
    void Start ()
    {
        gunLine = GetComponent<LineRenderer>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        //gunLine.SetPositions(transform.position, endPoint.transform.position);
    }
}
