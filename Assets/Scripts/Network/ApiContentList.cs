using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

public class ApiContentList : MonoBehaviour
{
    [ShowNonSerializedField] private string _url = " ";
    
    // Content List
    [SerializeField] private List<ContentList> contentLists = new List<ContentList>();
    
    // Get Api Controller
    [SerializeField] private ApiController apiController;
    
    // Data Handler
    private DataHandler _dataHandler;

    private void Awake()
    {
        _dataHandler = DataHandler.Instance;
    }

    private void Start()
    {
        _url = $"{GameEnv.BASE_URL}/session/content/{_dataHandler.GetPatientSelectedId()}/list?page=1&limit=10";
    }

    [Button]
    public void TrySetUrl()
    {
        _url = $"{GameEnv.BASE_URL}/session/content/{_dataHandler.GetPatientSelectedId()}/list?page=1&limit=10";
    }

    [Button]
    public void CallContentList()
    {
        if (_dataHandler.GetPatientSelectedId() == null) return;
        
        apiController.CallApi(_url, HandleContentList, apiController.HandleError);
    }

    private void HandleContentList(JObject jsonResponse)
    {
        bool success = jsonResponse["success"].Value<bool>();

        if (success)
        {
            JArray list = (JArray)jsonResponse["data"]["list"];

            if (list== null) return;

            foreach (var item in list)
            {
                ContentList contentListData = new ContentList
                {
                    idContent = item["id"]?.ToString(),
                    status = item["status"]?.ToString(),
                    // duration = item["duration"]?.ToString(),
                };

                JObject content = (JObject)item["content"];
                if (content == null) return;
                Content contentData = new Content
                {
                    id = content["id"]?.ToString(),
                    type = content["type"]?.ToString(),
                    title = content["title"]?.ToString(),
                    videoUrl = YoutubeParse.GetYouTubeVideoID(content["video_url"]?.ToString()),
                    description = content["description"]?.ToString(),
                    images = null,
                    questions = new List<Question>()
                };
                
                // Single Image
                apiController.DownloadImage(content["puzzle_image_url"]?.ToString(), 
                    sprite => contentData.puzzleImage = sprite);
                apiController.DownloadImage(content["cover"]?.ToString(), 
                    sprite => contentData.coverSprite = sprite);

                JArray questions = (JArray)content["questions"];
                if (questions == null) return;
                foreach (var question in questions)
                {
                    Question questionData = new Question
                    {
                        id = question["id"]?.ToString(),
                        questionText = question["question_text"]?.ToString(),
                        correctAnswer = question["correct_answer"]?.ToString(),
                        score = (question["score"] ?? 0).Value<int>(),
                        explanation = question["explanation"]?.ToString(),
                        choices = new List<Choice>()
                    };
                    apiController.DownloadImage(question["question_image"]?.ToString(),
                        sprite => questionData.image = sprite);

                    JArray choices = (JArray)question["choices"];
                    if (choices == null) return;

                    foreach (var choice in choices)
                    {
                        Choice choiceData = new Choice
                        {
                            id = choice["id"]?.ToString(),
                            key = choice["key"]?.ToString(),
                            text = choice["text"]?.ToString()
                        };
                        
                        apiController.DownloadImage(choice["image_url"]?.ToString(),
                            sprite => choiceData.image = sprite);
                        
                        questionData.choices.Add(choiceData);
                    }
                    contentData.questions.Add(questionData);
                }
                contentListData.content = contentData;
                
                // Simpan hasil convert
                contentLists.Add(contentListData);
            }
            Debug.Log($"Data saved: {contentLists.Count} records");
        }
        else
        {
            Debug.LogError("API response unsuccessful");
        }
    }

    public List<ContentList> GetContentList()
    {
        return contentLists;
    }
}


