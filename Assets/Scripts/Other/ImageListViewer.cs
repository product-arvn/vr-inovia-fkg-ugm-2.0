using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ImageListViewer : MonoBehaviour
{
    public Transform listParent;
    public Image imageObject;
    public List<Sprite> spriteList;

    private VerticalLayoutGroup layoutGroup;

    public UnityEvent onScrollEnd;

    static ScrollRect scrollRect;

    // Start is called before the first frame update
    void Awake()
    {        
        scrollRect = GetComponentInChildren<ScrollRect>();
        scrollRect.onValueChanged.AddListener(CheckScrollPos);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateImageList()
    {
        layoutGroup = listParent.gameObject.GetComponent<VerticalLayoutGroup>();
        foreach (Transform t in listParent)
        {
            Destroy(t.gameObject);
        }
        foreach (Sprite s in spriteList)
        {
            Image img = Instantiate(imageObject, listParent);
            img.sprite = s;
            img.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
            layoutGroup.enabled = false;
            layoutGroup.enabled = true;
            listParent.gameObject.SetActive(false);
            listParent.gameObject.SetActive(true);
        }
    }

    public void CheckScrollPos(Vector2 value)
    {
        Debug.Log("Scroll View");
        Debug.Log(value.ToString());
        Debug.Log(scrollRect.verticalNormalizedPosition.ToString());
        if (scrollRect.verticalNormalizedPosition < 0.05f)
        {
            onScrollEnd.Invoke();
        }
    }
}
