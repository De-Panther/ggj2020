using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShootGlueManager : MonoBehaviour
{
    private ParticleLouncher particleLouncher;

    private void Start()
    {
        particleLouncher = GetComponentInChildren<ParticleLouncher>();
    }
    void Update()
    { 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray))
        {
            Debug.DrawRay(transform.position, ray.direction);
            {
                if(Input.GetMouseButton(0))
                {
                    var targetPoint = ray.GetPoint(10 - transform.position.y);  // 10 = ceiling height
                    particleLouncher.isShooting = true;
                    particleLouncher.glueDropplets.transform.rotation = Quaternion.LookRotation(targetPoint - transform.position);
                }
                else
                {
                    particleLouncher.isShooting = false;
                }
            }
        }
    }
}
