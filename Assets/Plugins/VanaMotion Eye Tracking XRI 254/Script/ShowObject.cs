using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowObject : MonoBehaviour
{
    [SerializeField] private Material material;

    public void Selected()
    {
        material.color = Color.red;
    }

    public void Deselect()
    {
        material.color = Color.gray;
    }
}
