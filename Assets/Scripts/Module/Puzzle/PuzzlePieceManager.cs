using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePieceManager : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    
    public void ChangeImageMaterial(Sprite smat)
    {
        meshRenderer.sharedMaterials[0].SetTexture("_BaseMap", smat.texture);
    }
}
