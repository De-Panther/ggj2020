using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobScript : MonoBehaviour
{
    public float CrackState = 0;
    public event System.Action<GameObject> ShouldInfect;

    void Start()
    {
        InvokeRepeating("UpdateState", 0f, 0.1f);
    }

    private void UpdateState()
    {
        if (CrackState > 0)
        {
            CrackState += 1.5f;
            CrackState = Mathf.Min(CrackState, 100);

            if (Random.Range(0, 100) * 10 < CrackState)
            {
                ShouldInfect?.Invoke(gameObject);
            }
        }
    }
}
