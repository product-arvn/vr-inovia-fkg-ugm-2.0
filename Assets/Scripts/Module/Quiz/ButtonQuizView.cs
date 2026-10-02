using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Quiz
{
    public class ButtonQuizView : MonoBehaviour
    {
        [SerializeField] private TMP_Text optionText;
        [SerializeField] private Image thumbnail;
        [SerializeField] private Button button;

        public void SetOption(string nameText, Sprite thumbnailSprite)
        {
            optionText.text = nameText;
            thumbnail.sprite = thumbnailSprite;
        }
    
        public void SetOnClick(UnityAction action)
        {
            button.onClick.AddListener(action);
        }
    }
}
