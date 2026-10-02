using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuizScore : MonoBehaviour
{
    public int score;
    public TMP_Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        scoreText.text = "SCORE = " + score;
    }

    public void AddScore(int s)
    {
        score += s;
        scoreText.text = "SCORE = " + score;
    }

    public void ResetScore()
    {
        score = 0;
        scoreText.text = "SCORE = " + score;
    }
}
