using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Newtonsoft.Json.Linq;

/// <summary>
/// Sesi aktif milik headset ini: GET /device/session dan POST /device/session/content/stop.
/// Format data sama dengan /session/detail/{patientId} lama, jadi parsing konten per type tetap dipakai.
/// </summary>
public class ApiPatientDetail : MonoBehaviour
{
    [ShowNonSerializedField] private readonly string _sessionUrl = GameEnv.BASE_URL + "/device/session";
    [ShowNonSerializedField] private readonly string _doneContentUrl = GameEnv.BASE_URL + "/device/session/content/stop";
    private const int DoneContentMaxAttempt = 3;

    public SessionDetailData sessionDetailData;
    public ContentList contentList;

    [SerializeField] private ApiController apiController;

    private DataHandler _dataHandler;
    public bool IsContentDone { get; private set; }

    // Penanda konten yang terakhir dimuat. Bukan seluruh JSON, karena field seperti actual_duration berubah tiap polling
    private string _savedSignature;

    // Bool
    public bool HasSession { get; private set; }
    public bool HasContent { get; private set; }
    public bool IsDataIdentical { get; private set; }

    public bool IsDoneLoadedContent { get; private set; }

    // Jeda di level SESI (tombol Jeda dashboard / klinik dinonaktifkan). Konten tetap dikirim dengan status running.
    public bool IsPaused => sessionDetailData.status == "paused";

    private void Start()
    {
        _dataHandler = DataHandler.Instance;
    }

    // true sampai respons diterima dan seluruh gambar konten selesai diunduh (atau request gagal)
    public bool IsFetchingDetail { get; private set; }

    public void GetSessionDetail()
    {
        IsFetchingDetail = true;
        apiController.Get(_sessionUrl, response =>
        {
            if (response.Success)
            {
                StartCoroutine(FetchDetailCoroutine(response.Body));
            }
            else
            {
                IsFetchingDetail = false;
            }
        });
    }

    private IEnumerator FetchDetailCoroutine(JObject jsonResponse)
    {
        yield return StartCoroutine(HandleSessionDetailCoroutine(jsonResponse));
        IsFetchingDetail = false;
    }

    // Polling ringan: hanya cek ada sesi / ada konten berjalan. Juga berfungsi sebagai heartbeat device.
    public void GetNecessaryDetail()
    {
        apiController.Get(_sessionUrl, response =>
        {
            if (response.Success) HandleSessionNecessaryDetail(response.Body);
        });
    }

    // session_contents berupa objek: {} jika sesi ada tapi belum ada konten berjalan
    private static bool IsContentRunning(JObject sessionContents)
    {
        return sessionContents != null && sessionContents["id"] != null && sessionContents["id"].Type != JTokenType.Null;
    }

    private void HandleSessionNecessaryDetail(JObject jsonResponse)
    {
        try
        {
            // data: null = belum ada sesi yang di-assign ke headset ini
            JObject sessionData = jsonResponse["data"] as JObject;
            HasSession = sessionData != null;

            if (sessionData == null)
            {
                HasContent = false;
                return;
            }

            sessionDetailData.id = sessionData["id"]?.ToString();
            ReadSessionMeta(sessionData);

            if (sessionData["patient"] is JObject patientData)
            {
                sessionDetailData.patientData.id = patientData["id"]?.ToString();
                sessionDetailData.patientData.name = patientData["name"]?.ToString();
                _dataHandler?.SetPatientSelected(sessionDetailData.patientData.id, sessionDetailData.patientData.name);
            }

            if (sessionData["doctor"] is JObject doctorData)
            {
                sessionDetailData.doctorData.id = doctorData["id"]?.ToString();
                sessionDetailData.doctorData.name = doctorData["name"]?.ToString();
            }

            HasContent = IsContentRunning(sessionData["session_contents"] as JObject);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in HandleSessionNecessaryDetail: " + ex.Message);
        }
    }

    // Status dan jenis sesi diperbarui di setiap polling (juga saat konten sama)
    private void ReadSessionMeta(JObject sessionData)
    {
        sessionDetailData.status = ReadString(sessionData["status"]);
        sessionDetailData.sessionType = ReadString(sessionData["session_type"]);          // pre_clinics | on_clinics | null
        sessionDetailData.sessionTypeLabel = ReadString(sessionData["session_type_label"]); // Pre-Clinics | On-Clinics | null
    }

    // JSON null -> null (bukan string kosong / "null")
    private static string ReadString(JToken token)
    {
        return token == null || token.Type == JTokenType.Null ? null : token.ToString();
    }

    private static string BuildSignature(JObject sessionData)
    {
        if (sessionData == null) return "none";
        JObject sessionContents = sessionData["session_contents"] as JObject;
        return $"{sessionData["id"]}|{sessionContents?["id"]}|{sessionContents?["content"]?["id"]}";
    }

    private IEnumerator HandleSessionDetailCoroutine(JObject jsonResponse)
    {
        // Status sesi selalu diperbarui, termasuk saat konten sama (paused <-> running tanpa ganti konten)
        if (jsonResponse["data"] is JObject currentSession)
        {
            ReadSessionMeta(currentSession);
        }

        string signature = BuildSignature(jsonResponse["data"] as JObject);
        if (signature == _savedSignature)
        {
            IsDataIdentical = true;
            yield break;
        }

        IsDataIdentical = false;
        IsContentDone = false;
        IsDoneLoadedContent = false;

        _savedSignature = signature;

        JObject sessionData = jsonResponse["data"] as JObject;
        HasSession = sessionData != null;
        if (sessionData == null)
        {
            HasContent = false;
            yield break;
        }

        sessionDetailData.id = sessionData["id"]?.ToString();
        ReadSessionMeta(sessionData);

        JObject sessionContentData = sessionData["session_contents"] as JObject;
        if (!IsContentRunning(sessionContentData))
        {
            HasContent = false;
            yield break;
        }

        HasContent = true;
        contentList.idContent = sessionContentData["id"]?.ToString();
        contentList.status = sessionContentData["status"]?.ToString();
        contentList.duration = (sessionContentData["duration"] ?? 1).Value<int>() * 60;

        JObject contentData = sessionContentData["content"] as JObject;
        if (contentData == null) yield break;

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

        // Unduh daftar gambar, urutan dipertahankan (gagal unduh = null di posisinya).
        // Selalu buat list baru agar gambar konten sebelumnya tidak terbawa.
        List<Sprite> imagesSprite = new List<Sprite>();
        if (contentData["images"] is JArray images)
        {
            foreach (var image in images)
            {
                yield return StartCoroutine(apiController.SafeDownloadImage(image["url"]?.ToString(),
                    sprite => imagesSprite.Add(sprite)));
            }
        }
        contentList.content.images = imagesSprite;

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
        IsDoneLoadedContent = false;
        StartCoroutine(DoneContentCoroutine(contentList.idContent));
    }

    // Endpoint stop idempoten, jadi aman di-retry saat jaringan putus
    private IEnumerator DoneContentCoroutine(string sessionContentId)
    {
        JObject postData = new JObject
        {
            ["session_content_id"] = sessionContentId
        };

        for (int attempt = 1; attempt <= DoneContentMaxAttempt; attempt++)
        {
            ApiResponse result = null;
            apiController.Post(_doneContentUrl, postData, response => result = response);
            yield return new WaitUntil(() => result != null);

            if (result.Success)
            {
                IsContentDone = true;
                HasContent = false;
                yield break;
            }

            if (!result.IsNetworkError && !result.IsRateLimited) break;
            yield return new WaitForSecondsRealtime(GameEnv.NETWORK_RETRY_INTERVAL);
        }

        // Konten masih running di server: lupakan respons tersimpan agar konten yang sama ditampilkan ulang
        Debug.LogError($"Gagal menghentikan konten {sessionContentId}");
        _savedSignature = null;
        IsContentDone = false;
        HasContent = false;
    }
}

[Serializable]
public class SessionDetailData
{
    public string id;
    public string status;
    public string sessionType;
    public string sessionTypeLabel;
    public PatientData patientData;
    public DoctorData doctorData;
}

[Serializable]
public class PatientData
{
    public string id;
    public string name;
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
