using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Video;
using VanaMotion_XRI;
using NaughtyAttributes;

public class Manager360 : MonoBehaviour
{
    [SerializeField] private FadeCamera fadeCamera;
    [SerializeField] private GameObject[] objectInScene;
    [SerializeField] private GameObject[] objectToShow;
    [SerializeField] private Material resultMaterial;
    [SerializeField, CanBeNull] private VideoPlayer videoPlayer;
    [SerializeField] private float fadeDuration = 1.0f;
    private Material _skyMaterial;

    private void Start()
    {
        _skyMaterial = RenderSettings.skybox;
    }

    [Button]
    public void Start360()
    {
        StartCoroutine(FadeAndStart360(resultMaterial, null));
    }

    [Button]
    public void Stop360()
    {
        StartCoroutine(FadeAndStart360(_skyMaterial, null));
    }

    IEnumerator FadeAndStart360(Material target, [CanBeNull] Action onComplete)
    {
        fadeCamera.StartFadeIn();
        yield return new WaitForSeconds(fadeDuration);
        
        HideOrShowObjects(target);
        fadeCamera.StartFadeOut();

        RenderSettings.skybox = target;
        onComplete?.Invoke();
    }

    private void HideOrShowObjects(Material target)
    {
        SetObjectActive(target.Equals(_skyMaterial));
    }

    private void SetObjectActive(bool isActive)
    {
        foreach (var obj in objectInScene)
        {
            obj.SetActive(isActive);
        }

        foreach (var obj in objectToShow)
        {
            obj.SetActive(!isActive);
        }
    }
}
