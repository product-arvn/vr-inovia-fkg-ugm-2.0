using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralScript : MonoBehaviour
{
    [SerializeField] [CanBeNull] private AudioSource bgmSound;
    
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void PlayBGM(bool activated)
    {
        if (activated)
        {
            bgmSound.Play();
        }
        else
        {
            bgmSound.Pause();
        }
    }
}
