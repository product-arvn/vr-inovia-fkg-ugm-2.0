using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    private float _yPos;

    // Start is called before the first frame update
    void Start()
    {
        _yPos = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
            this.transform.LookAt(targetPos);
        }
    }

    public void SetTargetTransform(Transform t)
    {
        target = t;
    }
}
