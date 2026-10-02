using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollControll : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollSpeed = 0.1f;

    public void ScrollUp()
    {
        scrollRect.verticalNormalizedPosition += scrollSpeed;
    }
    
    public void ScrollDown()
    {
        scrollRect.verticalNormalizedPosition -= scrollSpeed;
    }
    
    public void ScrollToTop()
    {
        scrollRect.verticalNormalizedPosition = 1f;
    }
    
    public void ScrollToBottom()
    {
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
