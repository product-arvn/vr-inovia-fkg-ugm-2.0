using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EducationContentView : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private Image thumbnailImage;
    
    public void SetCount(int count, int total)
    {
        countText.text = $"{count}/{total}";
    }
    
    public void SetType(string type)
    {
        typeText.text = type;
    }
    
    public void SetContent(string content)
    {
        contentText.text = content;
    }
    
    public void SetThumbnail(Sprite sprite)
    {
        if (sprite == null)
        {
            thumbnailImage.sprite = null;
            thumbnailImage.color = new Color(0, 0, 0, 0);
            return;
        }
        
        thumbnailImage.color = new Color(1, 1, 1, 1);
        thumbnailImage.sprite = sprite;
    }

    public void ActivateThis(bool activate)
    {
        this.gameObject.SetActive(activate);
    }
}
