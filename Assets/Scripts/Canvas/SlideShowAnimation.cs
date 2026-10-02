using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SlideShowAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Image displayImage;
    [SerializeField] private float slideDuration = 5f;
    [SerializeField] private float transitionDuration = 0.5f;
    private int _currentIndex = 0;

    private void OnEnable()
    {
        displayImage.color = new Color(displayImage.color.r, displayImage.color.g, displayImage.color.b, 1f);
        displayImage.sprite = sprites[_currentIndex];

        StartCoroutine(SlideShowCoroutine());
    }

    IEnumerator SlideShowCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(slideDuration);

            displayImage.DOFade(0f, transitionDuration).OnComplete(() =>
            {
                int rand = Random.Range(0, sprites.Length);
                _currentIndex = rand == _currentIndex ? Random.Range(0, sprites.Length) : rand;
                displayImage.sprite = sprites[_currentIndex];
                displayImage.DOFade(1f, transitionDuration);
            });
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
