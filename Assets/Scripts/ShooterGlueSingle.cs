using UnityEngine;

public class ShooterGlueSingle : MonoBehaviour
{
    public ParticleLouncher particleLouncher;
    public WebXRController webXRController;
    public string buttonName;

    void Update()
    {
        particleLouncher.isShooting = webXRController.GetButton(buttonName);
    }
}
