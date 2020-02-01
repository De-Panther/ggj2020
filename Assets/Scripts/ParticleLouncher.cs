using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLouncher : MonoBehaviour
{
    [SerializeField] private ParticleSystem glueDropplets;
    [SerializeField] private ParticleSystem splatterParticles;

    List<ParticleCollisionEvent> glueCollisionEvents;
    void Start()
    {
        glueCollisionEvents = new List<ParticleCollisionEvent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            Debug.Log("Fire Glue");
            glueDropplets.Emit(1);
        }
    }


    private void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(glueDropplets, other, glueCollisionEvents);

        for (int i = 0; i < glueCollisionEvents.Count;  i++)
        {
            EmitAtLocation(glueCollisionEvents[i]);
        }

    }

    private void EmitAtLocation(ParticleCollisionEvent i)
    {
        splatterParticles.transform.position = i.intersection;
        splatterParticles.transform.rotation = Quaternion.LookRotation(i.normal);
        splatterParticles.Emit(1);
    }
}
