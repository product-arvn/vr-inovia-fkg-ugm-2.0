using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EducationCoverView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image coverImage;
    
    public void SetTitle(string title)
    {
        titleText.text = title;
    }
    
    public void SetCover(Sprite sprite)
    {
        if (sprite == null)
        {
            coverImage.gameObject.SetActive(false);
            return;
        }
        
        coverImage.gameObject.SetActive(true);
        coverImage.sprite = sprite;
    }

    public void ActiveThis(bool activate)
    {
        this.gameObject.SetActive(activate);
    }
}
