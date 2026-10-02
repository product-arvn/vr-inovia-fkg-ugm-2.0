using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] InputActionProperty menuButton;
    float pauseDelay;
    public bool isPaused;
    public bool isFinished;
    public float playTime;

    public GameObject taskPanel;
    public GameObject confirmHomePanel;
    public GameObject confirmRestartPanel;
    public GameObject confirmFinishPanel;
    public GameObject gameOverPanel;
    public GameObject successPanel;
    public GameObject homeButton;
    public GameObject restartButton;
    public GameObject finishButton;
    public AudioSource successAudio;
    public AudioSource failAudio;
    public ParticleSystem confetti;

    [Header("Player Data")]
    public TMP_Text userNameText;
    public TMP_Text userNPKText;

    [Header("Modul Data")]
    public TMP_Text activityNameText;
    public TMP_Text scenarioNameText;
    public TMP_Text playTimeText;
    public TMP_Text scoreText;
    public TMP_Text finishScoreText;
    public TMP_Text statusText;

    // Start is called before the first frame update
    void Start()
    {
        //if(Instance == null)
        //{
        //    Instance = this;
        //    DontDestroyOnLoad(this.gameObject);
        //}
        //else
        //{
        //    Destroy(this.gameObject);
        //}
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name != "MenuScene")
        {
            GoToScene("MenuScene");
        }
        if (pauseDelay <= 0)
        {
            pauseDelay = 0;
            float getPause = menuButton.action.ReadValue<float>();
            if ((getPause == 1 || Input.GetKey(KeyCode.P)) && SceneManager.GetActiveScene().name != "MenuScene")
            {
                isPaused = !isPaused;
                pauseDelay = 0.5f;
                //GoToScene("MenuScene");
                if (isPaused)
                {
                    //Pause();
                    Debug.Log("Paused");
                }
                else
                {
                    HideTaskPanel();
                    //UnPause();
                    Debug.Log("UnPaused");
                }
            }
        }
        else
        {
            pauseDelay -= Time.deltaTime;
        }

        if (!isFinished)
        {
            playTime += Time.deltaTime;
        }
        if (isPaused)
        {
            playTimeText.text = FormatTime(playTime);
        }
    }

    public void HideTaskPanel()
    {
        isPaused = false;
        taskPanel.transform.parent.gameObject.SetActive(true);
        taskPanel.SetActive(false);
        confirmHomePanel.SetActive(false);
        confirmRestartPanel.SetActive(false);
        confirmFinishPanel.SetActive(false);
    }

    public void GoToHome()
    {
        GoToScene("MenuScene");
    }

    public void Restart()
    {
        GoToScene(SceneManager.GetActiveScene().name);
    }

    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("IsLogin");
    }

    public string FormatTime(float t)
    {
        int minutes = (int)t / 60;
        int seconds = (int)t - 60 * minutes;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
