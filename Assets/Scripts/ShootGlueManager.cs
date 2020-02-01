﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShootGlueManager : MonoBehaviour
{
    public SplashesOfGluePool splashDecalPool;
    public Gradient particleColorGradient;
    public LayerMask wallsMask;

    private ParticleSystem glueDropplets;
    private List<ParticleCollisionEvent> glueCollisionEvents;


    void Start()
    {
        glueDropplets = GetComponentInChildren<ParticleSystem>();
        glueCollisionEvents = new List<ParticleCollisionEvent>();
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
                    glueDropplets.transform.rotation = Quaternion.LookRotation(targetPoint - transform.position);
                    ParticleSystem.MainModule mainModule = glueDropplets.main;
                    mainModule.startColor = particleColorGradient.Evaluate(Random.Range(0, 1));

                    glueDropplets.Emit(1);
                }
            }
        }
    }


    private void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(glueDropplets, other, glueCollisionEvents);

        for (int i = 0; i < glueCollisionEvents.Count; i++)
        {
            Debug.Log("is collided");
            splashDecalPool.ParticleHit(glueCollisionEvents[i], particleColorGradient);
        }
    }
}
