using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShootGlueManager : MonoBehaviour
{
    [SerializeField] private float speedHorizontal = 2.0f;
    [SerializeField] private float speedVertical = 2.0f;
    [SerializeField] private float yaw;
    [SerializeField] private float pitch;

    // Update is called once per frame
    void Update()
    {

        yaw += speedHorizontal * Input.GetAxis("Mouse X");
        pitch -= speedVertical * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}
