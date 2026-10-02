using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VanaMotion_XRI
{
    public class FadeCamera : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private float fadeDuration = 1.0f;

        private void Start()
        {
            Color tempColor = image.color;
            tempColor.a = 1;
            image.color = tempColor;
            
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeIn()
        {
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                SetAlpha(alpha);
                yield return null;
            }

            if (elapsedTime >= fadeDuration)
            {
                fadeDuration = 1f;
            }
        }

        private IEnumerator FadeOut()
        {
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(1.0f - (elapsedTime / fadeDuration));
                SetAlpha(alpha);
                yield return null;
            }
            
            if (elapsedTime >= fadeDuration)
            {
                fadeDuration = 1f;
            }
        }

        private void SetAlpha(float alpha)
        {
            Color tempColor = image.color;
            tempColor.a = alpha;
            image.color = tempColor;
        }
        
        public void StartFadeIn()
        {
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
        
        public void StartFadeOut()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        public void StartFadeInCustom(float duration)
        {
            StopAllCoroutines();
            
            fadeDuration = duration;
            
            StartCoroutine(FadeIn());
        }
        
        public void StartFadeOutCustom(float duration)
        {
            StopAllCoroutines();
            
            fadeDuration = duration;
            
            StartCoroutine(FadeOut());
        }
    }
}
