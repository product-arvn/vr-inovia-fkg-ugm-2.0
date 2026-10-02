using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingView : MonoBehaviour
{
    [SerializeField] private TMP_Text loadingPurposeText;
    
    public void ActiveLoadingPanel(string purpose)
    {
        gameObject.SetActive(true);
        loadingPurposeText.text = purpose;
    }

    public void DisableLoadingPanel()
    {
        gameObject.SetActive(false);
        loadingPurposeText.text = "Loading...";
    }
}
