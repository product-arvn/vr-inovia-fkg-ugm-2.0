using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quiz
{
    public class QuizPanelView : MonoBehaviour
    {
        [Header("Question Panel")]
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text countQuizText;

        [Header("Button")] 
        [SerializeField] private ButtonQuizView optionA;
        [SerializeField] private ButtonQuizView optionB;
        
        public void SetQuestionText(string text)
        {
            questionText.text = text;
        }
        
        public void SetCountQuizText(string text)
        {
            countQuizText.text = text;
        }
        
        public ButtonQuizView GetOptionA()
        {
            return optionA;
        }
        
        public ButtonQuizView GetOptionB()
        {
            return optionB;
        }
    }
}
