using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLouncher : MonoBehaviour
{
    public SplashesOfGluePool splashDecalPool;
    public Gradient particleColorGradient;

    private ParticleSystem glueDropplets;
    private List<ParticleCollisionEvent> glueCollisionEvents;
    void Start()
    {
        glueDropplets = GetComponent<ParticleSystem>();
        glueCollisionEvents = new List<ParticleCollisionEvent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            ParticleSystem.MainModule mainModule = glueDropplets.main;
            mainModule.startColor = particleColorGradient.Evaluate(Random.Range(0, 1));

            glueDropplets.Emit(1);
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
