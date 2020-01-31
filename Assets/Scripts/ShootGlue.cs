using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShootGlue : MonoBehaviour
{
    [SerializeField] private ParticleSystem glueDropplets;
    [SerializeField] private Transform glueFiller;

    public GameObject glueFillerPrefab;


    void Start()
    {
        Assert.IsNotNull(glueDropplets);
        Assert.IsNotNull(glueFiller);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
