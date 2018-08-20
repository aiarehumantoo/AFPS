using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAnim : MonoBehaviour
{
    float rotationSpeed = 0.4f;
    float moveDuration = 2f;
    float moveSpeed = 0.1f;
    float timer;

    // Update is called once per frame
    void Update()
    {
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        if (timer >= moveDuration && Time.timeScale != 0)
        {
            // change direction
            timer = 0;
            moveSpeed = moveSpeed * -1;     //theres command for this.

        }

        // move & rotate
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationSpeed, transform.eulerAngles.z);
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
}