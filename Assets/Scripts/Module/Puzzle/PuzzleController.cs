using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

public class PuzzleController : MonoBehaviour, IModuleController
{
    private PuzzleData _puzzleData = new PuzzleData();
    [SerializeField] private PuzzleModuleView puzzleModuleView;
    
    // Type Puzzle
    [SerializeField] private SlidePuzzleControllerData[] slidePuzzleControllers;
    
    public int score = 0;

    public UnityAction OnPuzzleEnd;

    private GlobalTimer _globalTimer;

    public void StartContent()
    {
        
    }

    public void EndContent()
    {
        OnPuzzleEnd?.Invoke();
    }

    private void Start()
    {
        StartContent();
        _globalTimer = FindObjectOfType<GlobalTimer>();
    }

    private void Update()
    {
        puzzleModuleView.SetTimerText(_globalTimer.Timer);
    }

    public void ActivateGameObject(bool activate)
    {
        this.gameObject.SetActive(activate);
    }

    public void SetPuzzleData(string theme1, Sprite image1, float duration, string type)
    {
        if (theme1 == null)
        {
            Debug.Log("Theme Null");
        }

        if (image1 == null)
        {
            Debug.Log("Image Null");
        }
        
        _puzzleData.theme = theme1;
        _puzzleData.image = image1;

        switch (type)
        {
            case "horizontal":
                SetPuzzle(slidePuzzleControllers[0].slidePuzzleController, duration, image1);
                slidePuzzleControllers[0].panelObject.SetActive(true);
                break;
            case "square":
                SetPuzzle(slidePuzzleControllers[1].slidePuzzleController, duration, image1);
                slidePuzzleControllers[1].panelObject.SetActive(true);
                break;
            case "vertical":
                SetPuzzle(slidePuzzleControllers[2].slidePuzzleController, duration, image1);
                slidePuzzleControllers[2].panelObject.SetActive(true);
                break;
            default:
                SetPuzzle(slidePuzzleControllers[0].slidePuzzleController, duration, image1);
                slidePuzzleControllers[0].panelObject.SetActive(true);
                break;
        }
        
        puzzleModuleView.SetThemeText(theme1);
    }

    private void SetPuzzle(SlidePuzzleController slidePuzzle, float duration, Sprite image)
    {
        slidePuzzle.ActivateThis(true);
        slidePuzzle.SetAutoCompleteDuration(duration);
        StartCoroutine(slidePuzzle.StartGame(image));
        
        slidePuzzle.AddOnPuzzleCompleted((() =>
        {
            Debug.Log( gameObject.name +", Puzzle Complete");
            puzzleModuleView.ShowDonePanel(score);
            foreach (var slide in slidePuzzleControllers)
            {
                slide.panelObject.SetActive(false);
            }
        }));
    }

    public void SetHeader(string creator, string title)
    {
        puzzleModuleView.SetCreatorText(creator);
        puzzleModuleView.SetThemeText(title);
    }
    
}

[Serializable]
public class PuzzleData
{
    public string theme;
    public Sprite image;
}

[Serializable]
public class SlidePuzzleControllerData
{
    public GameObject panelObject;
    public SlidePuzzleController slidePuzzleController;
}