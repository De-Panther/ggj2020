using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashesOfGluePool : MonoBehaviour
{
    public int maxDecal = 100;
    public float splashSizeMin = .5f;
    public float splashSizeMax = 1.5f;
    public Gradient splashesColorGradient;


    private ParticleSystem splashyParticleSystem;
    private int splashesOfGlueDataIndex;
    private ParticleDecalData[] particleData;
    private ParticleSystem.Particle [] particles;


    private void Start()
    {
        splashyParticleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[maxDecal];
        particleData = new ParticleDecalData[maxDecal];
        for (int i = 0; i < maxDecal; i++)
        {
            particleData[i] = new ParticleDecalData(); 
        }
    }

    public void ParticleHit(ParticleCollisionEvent e, Gradient colorGradient)
    {

        SetParticalData(e, colorGradient);
        DisplayParticles();
    }
    private void SetParticalData(ParticleCollisionEvent e, Gradient colorGradient)
    {
        if (splashesOfGlueDataIndex >= maxDecal)
        {
            splashesOfGlueDataIndex = 0;
        }

        Debug.Log("this is happening");
        particleData[splashesOfGlueDataIndex].position = e.intersection;

        Vector3 particleRotationEuler = Quaternion.LookRotation(e.normal).eulerAngles;
        particleRotationEuler.z = Random.Range(0, 360);

        particleData[splashesOfGlueDataIndex].rotation = particleRotationEuler;
        particleData[splashesOfGlueDataIndex].size = Random.Range(splashSizeMin, splashSizeMax);
        particleData[splashesOfGlueDataIndex].color = splashesColorGradient.Evaluate(Random.Range(0f, 1f));

        splashesOfGlueDataIndex++;
    }

    public void DisplayParticles()
    {
        for (int i = 0; i < particleData.Length; i++)
        {
            particles[i].position = particleData[i].position;
            particles[i].rotation3D = particleData[i].rotation;
            particles[i].startSize = particleData[i].size;
            particles[i].startColor = particleData[i].color;
        }
        Debug.Log("this is happening too");
        splashyParticleSystem.SetParticles(particles, particles.Length);
    }
}
