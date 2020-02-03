using UnityEngine;

public class ShootGlueManager : MonoBehaviour
{
    public ParticleLouncher particleLouncher;
    public Transform particleLouncherPivot;
    public Camera mainCamera;
    public WebXRController webXRController;
    public WebXRManager webXRManager;

    private RaycastHit hit;
    private Ray ray;

    private bool allowShooting = true;

    void OnEnable()
    {
        webXRManager.OnXRChange += OnXRChange;
    }

    void OnDisable()
    {
        webXRManager.OnXRChange -= OnXRChange;
    }

    void OnXRChange(WebXRState state)
    {
        allowShooting = (state == WebXRState.NORMAL);
        particleLouncherPivot.gameObject.SetActive(!allowShooting);
    }

    void Update()
    {
        if (!allowShooting)
        {
            return;
        }
        ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            particleLouncherPivot.LookAt(hit.point);
            if (Input.GetMouseButton(0) || webXRController.GetButton("Trigger"))
            {
                particleLouncher.isShooting = true;
            }
            else
            {
                particleLouncher.isShooting = false;
            }
        }
    }
}
