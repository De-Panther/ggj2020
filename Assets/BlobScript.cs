using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobScript : MonoBehaviour
{
    public float CrackState = 0;
    public event System.Action<GameObject> ShouldInfect;

    public int test;

    void Start()
    {
        
    }

    void Update()
    {
        if (CrackState > 0)
        {
            CrackState += 1.5f;
            CrackState = Mathf.Min(CrackState, 100);

            if (Random.Range(0, 100) < CrackState)
            {
                ShouldInfect?.Invoke(gameObject);
            }
        }
    }
}
