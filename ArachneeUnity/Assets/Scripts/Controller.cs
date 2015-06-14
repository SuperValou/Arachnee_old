﻿using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour 
{
    public bool canMove = true;

    // keyboard
    public float keyboardSensitivity = 1;

    // mouse
    public float sensitivityX = 1F;
    public float sensitivityY = 1F;

    public float minimumX = -80F;
    public float maximumX = 80F;
    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationY = 0F;


    void Update()
    {
        // keyboard
        if (this.canMove)
        {
            this.transform.position += this.transform.forward * Input.GetAxis("Vertical") * keyboardSensitivity;
            this.transform.position += this.transform.right * Input.GetAxis("Horizontal") * keyboardSensitivity;
        }
        
        // mouse
        if (Input.GetMouseButton(0))
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
            transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision avec l'ID " + other.gameObject.GetComponent<Entry>().Id);
    }
}
