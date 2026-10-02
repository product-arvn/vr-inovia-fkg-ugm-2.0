using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingAnimation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 180f;
    
    private void Update()
    {
        if (gameObject.activeSelf)
        {
            transform.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));
        }
    }
}
