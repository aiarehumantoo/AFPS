using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    private CharacterController _controller;

    public GUIStyle style;
    string touching = "Null";

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 60, 400, 100), "Touching: " + touching, style);
        GUI.Label(new Rect(0, 80, 400, 100), "OnGround: " + _controller.isGrounded, style);

        var ups = _controller.velocity;
        ups.y = 0;
        GUI.Label(new Rect(0, 100, 400, 100), "Speed: " + Mathf.Round(ups.magnitude * 100) / 100 + "ups", style);
        
        var ups2 = _controller.velocity;
        //ups2.x = 0;
        //ups2.z = 0;
        //GUI.Label(new Rect(0, 120, 400, 100), "Vertical Speed: " + Mathf.Round(ups2.magnitude * 100) / 100 + "ups", style);
        GUI.Label(new Rect(0, 120, 400, 100), "Vertical Speed: " + ups2.y + "ups", style);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag != "Untagged" && collider.tag != "Player")
        {
            touching = collider.tag;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag != "Untagged")
        {
            touching = "Null";
        }
    }
}