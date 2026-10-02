using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ApiPatientDetail : MonoBehaviour
{
    [ShowNonSerializedField] private string _url = "";
    [ShowNonSerializedField] private readonly string _doneContentUrl = GameEnv.BASE_URL + "/session/content/stop";

    public SessionDetailData sessionDetailData;
    public ContentList contentList;

    [SerializeField] private ApiController apiController;

    private DataHandler _dataHandler;
    public bool IsContentDone { get; private set; }
    
    private JObject _savedPostData;
    
    public UnityAction OnSessionNotFound;
    
    // Bool
    public bool HasContent { get; private set; }
    public bool IsDataIdentical { get; private set; }
    
    public bool IsDoneLoadedContent { get; private set; }

    private void Start()
    {
        _dataHandler = DataHandler.Instance;
    }
    
    public void GetSessionDetail()
    {
        _url = $"{GameEnv.BASE_URL}/session/detail/{_dataHandler.GetPatientSelectedId()}";
        apiController.CallApi(_url, HandleSessionDetail, HandleError);
    }
    
    private void HandleError(string errorMessage)
    {
#if UNITY_EDITOR
        Debug.LogError("API Patient Error: " + errorMessage);
#endif
        if (errorMessage == "Request failed: HTTP/1.1 404 Not Found")
        {
            OnSessionNotFound?.Invoke();
        }
    }

    public void GetNecessaryDetail()
    {
        _url = $"{GameEnv.BASE_URL}/session/detail/{_dataHandler.GetPatientSelectedId()}";
        apiController.CallApi(_url, HandleSessionNecessaryDetail, HandleError);
    }

    private void HandleSessionNecessaryDetail(JObject jsonResponse)
    {
        try
        {
            if (jsonResponse == null)
            {
                Debug.LogError("Null JSON response");
                return;
            }

            bool success = jsonResponse["success"]?.Value<bool>() ?? false;
            Debug.Log("RUN HANDLE NECESSARY ONLY");

            if (success)
            {
                JObject sessionData = (JObject)jsonResponse["data"];
                if (sessionData != null)
                {
                    
                    sessionDetailData.id = sessionData["id"]?.ToString();
                    sessionDetailData.status = sessionData["status"]?.ToString();

                    JObject patientData = (JObject)sessionData["patient"];
                    if (patientData != null)
                    {
                        sessionDetailData.patientData.id = patientData["id"]?.ToString();
                        sessionDetailData.patientData.name = patientData["name"]?.ToString();
                    }

                    JObject doctorData = (JObject)sessionData["doctor"];
                    if (doctorData != null)
                    {
                        sessionDetailData.doctorData.id = doctorData["id"]?.ToString();
                        sessionDetailData.doctorData.name = doctorData["name"]?.ToString();
                    }
                    
                    JObject sessionContents = (JObject)sessionData["session_contents"];
                    if (sessionContents == null || sessionContents.Count == 0)
                    {
                        HasContent = false;
                    }
                    else
                    {
                        HasContent = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in HandleSessionNecessaryDetail: " + ex.Message);
        }
    }

    private void HandleSessionDetail(JObject jsonResponse)
    {
        StartCoroutine(HandleSessionDetailCoroutine(jsonResponse));
    }

    private IEnumerator HandleSessionDetailCoroutine(JObject jsonResponse)
    {
        if (jsonResponse == null)
        {
            Debug.LogError("Null JSON response");
            yield break;
        }

        bool success = jsonResponse["success"]?.Value<bool>() ?? false;
        if (JToken.DeepEquals(jsonResponse, _savedPostData))
        {
            IsDataIdentical = true;
            yield break;
        }

        IsDataIdentical = false;
        IsContentDone = false;
        Debug.Log("RUN HANDLE ALL DATA");

        _savedPostData = jsonResponse;

        if (success)
        {
            JObject sessionData = (JObject)jsonResponse["data"];
            if (sessionData != null)
            {
                sessionDetailData.id = sessionData["id"]?.ToString();
                sessionDetailData.status = sessionData["status"]?.ToString();

                JObject sessionContentData = (JObject)sessionData["session_contents"];
                if (sessionContentData != null)
                {
                    HasContent = true;
                    contentList.idContent = sessionContentData["id"]?.ToString();
                    contentList.status = sessionContentData["status"]?.ToString();
                    contentList.duration = (sessionContentData["duration"] ?? 1).Value<int>() * 60;

                    JObject contentData = (JObject)sessionContentData["content"];
                    if (contentData != null)
                    {
                        contentList.content.id = contentData["id"]?.ToString();
                        contentList.content.type = contentData["type"]?.ToString();
                        contentList.content.title = contentData["title"]?.ToString();
                        contentList.content.description = contentData["description"]?.ToString();

                        switch (contentList.content.type)
                        {
                            case "puzzle":
                                yield return StartCoroutine(HandlePuzzleTypeCoroutine(contentData));
                                break;
                            case "quiz":
                                yield return StartCoroutine(HandleQuizTypeCoroutine(contentData));
                                break;
                            case "education":
                                yield return StartCoroutine(HandleEducationTypeCoroutine(contentData));
                                break;
                            case "video":
                                HandleVideoType(contentData);
                                break;
                        }
                    }
                }
                else
                {
                    HasContent = false;
                }
            }
        }
    }

    IEnumerator HandleQuizTypeCoroutine(JObject contentData)
    {
        List<Question> questions = new List<Question>();

        JArray questionsData = (JArray)contentData["questions"];

        if (questionsData != null)
        {
            foreach (var question in questionsData)
            {
                Question quest = new Question
                {
                    id = question["id"]?.ToString(),
                    questionText = question["question_text"]?.ToString(),
                    correctAnswer = question["correct_answer"]?.ToString(),
                    score = (question["score"] ?? 0).Value<int>(),
                    explanation = question["explanation"]?.ToString(),
                    choices = new List<Choice>()
                };

                JArray choices = question["choices"] as JArray;
                if (choices != null)
                {
                    foreach (var choice in choices)
                    {
                        Choice choiceData = new Choice
                        {
                            id = choice["id"]?.ToString(),
                            key = choice["key"]?.ToString(),
                            text = choice["text"]?.ToString()
                        };
                        
                        // Yield Image Safe
                        yield return StartCoroutine(apiController.SafeDownloadImage(choice["image_url"]?.ToString(),
                            sprite => choiceData.image = sprite));

                        quest.choices.Add(choiceData);
                    }
                }

                questions.Add(quest);
            }

            contentList.content.questions = questions;
            IsDoneLoadedContent = true;
        }
    }
    
    private IEnumerator HandlePuzzleTypeCoroutine(JObject contentData)
    {
        if (contentData == null)
        {
            Debug.LogError("Content data is null in HandlePuzzleType");
            yield break;
        }
        
        yield return StartCoroutine(apiController.SafeDownloadImage(contentData["puzzle_image_url"]?.ToString(),
            sprite => contentList.content.puzzleImage = sprite));
        
        contentList.content.puzzleType = contentData["puzzle_dimention"]?.ToString();
        
        IsDoneLoadedContent = true;
    }

    private IEnumerator HandleEducationTypeCoroutine(JObject contentData)
    {
        if (contentData == null)
        {
            Debug.LogError("Content data is null in HandleEducationType");
            yield break;
        }

        // Unduh gambar cover secara aman
        yield return StartCoroutine(apiController.SafeDownloadImage(contentData["cover"]?.ToString(),
            sprite => contentList.content.coverSprite = sprite));

        contentList.content.description = contentData["description"]?.ToString();

        // Unduh daftar gambar
        JArray images = contentData["images"] as JArray;
        if (images != null)
        {
            List<Sprite> imagesSprite = new List<Sprite>();
            foreach (var image in images)
            {
                yield return StartCoroutine(apiController.SafeDownloadImage(image["url"]?.ToString(),
                    sprite => imagesSprite.Add(sprite)));
            }

            contentList.content.images = imagesSprite;
        }
        
        IsDoneLoadedContent = true;
    }

    private void HandleVideoType(JObject contentData)
    {
        try
        {
            contentList.content.videoUrl = YoutubeParse.GetYouTubeVideoID(contentData["video_url"]?.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in HandleVideoType: " + ex.Message);
        }
        
        IsDoneLoadedContent = true;
    }

    public void DoneContent()
    {
        JObject postData = new JObject
        {
            ["patient_id"] = _dataHandler.GetPatientSelectedId(),
            ["session_content_id"] = contentList.idContent
        };
        apiController.PostApi(_doneContentUrl, postData, OnContentDone, apiController.HandleError);
        IsDoneLoadedContent = false;
    }

    private void OnContentDone(JObject response)
    {
        bool success = response["success"].Value<bool>();
        IsContentDone = success;
        HasContent = false;
    }
}

[Serializable]
public class SessionDetailData
{
    public string id;
    public string status;
    public PatientData patientData;
    public DoctorData doctorData;
}

[Serializable]
public class DoctorData
{
    public string id;
    public string name;
}

[Serializable]
public class ContentList
{
    public string idContent;
    public string status;
    public int duration;
    public Content content;
}

[Serializable]
public class Content
{
    public string id;
    public string type;
    public string title;
    public string videoUrl;
    public Sprite puzzleImage;
    public string puzzleType;
    public string description;
    public Sprite coverSprite;
    public List<Sprite> images = new List<Sprite>();
    public List<Question> questions;
}

[Serializable]
public class Question
{
    public string id;
    public string questionText;
    public Sprite image;
    public string correctAnswer;
    public int score;
    public string explanation;
    public List<Choice> choices;
}

[Serializable]
public class Choice
{
    public string id;
    public string key;
    public string text;
    public Sprite image;
}