﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceTest : MonoBehaviour
{
    public float amount;
    private CharacterController _controller;

    // Use this for initialization
    void Start ()
    {
        _controller = GetComponent<CharacterController>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        //_controller.Move(transform.forward * Time.deltaTime);
        _controller.Move(transform.forward * 0.01f);
    }
}
