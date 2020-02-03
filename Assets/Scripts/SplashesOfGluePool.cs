using UnityEngine;

public class SplashesOfGluePool : MonoBehaviour
{
    public int maxDecal = 100;
    public float splashSizeMin = .5f;
    public float splashSizeMax = 1.5f;

    public Transform splashPrefab;


    private ParticleSystem splashyParticleSystem;
    private int splashesOfGlueDataIndex;
    private Transform[] splashes;
    private GameObject[] splashesGO;


    private void Start()
    {
        splashyParticleSystem = GetComponent<ParticleSystem>();
        splashes = new Transform[maxDecal];
        splashesGO = new GameObject[maxDecal];
        for (int i = 0; i < maxDecal; i++)
        {
            splashes[i] = Instantiate(splashPrefab, transform);
            splashesGO[i] = splashes[i].gameObject;
            splashesGO[i].SetActive(false);
        }
    }

    public void ParticleHit(ParticleCollisionEvent e, Gradient colorGradient)
    {
        SetParticalData(e, colorGradient);
    }

    private void SetParticalData(ParticleCollisionEvent e, Gradient colorGradient)
    {
        if (splashesOfGlueDataIndex >= maxDecal)
        {
            splashesOfGlueDataIndex = 0;
        }

        if (!splashesGO[splashesOfGlueDataIndex].activeSelf)
        {
            splashesGO[splashesOfGlueDataIndex].SetActive(true);
        }
        splashes[splashesOfGlueDataIndex].position = e.intersection;

        Vector3 particleRotationEuler = Quaternion.LookRotation(e.normal).eulerAngles;
        splashes[splashesOfGlueDataIndex].eulerAngles = particleRotationEuler;
        splashes[splashesOfGlueDataIndex].Rotate(Vector3.forward, Random.Range(0, 360));
        splashes[splashesOfGlueDataIndex].localScale = Vector3.one * Random.Range(splashSizeMin, splashSizeMax);

        splashes[splashesOfGlueDataIndex].Translate(0f,0f,-0.23f);

        splashesOfGlueDataIndex++;
    }
}
