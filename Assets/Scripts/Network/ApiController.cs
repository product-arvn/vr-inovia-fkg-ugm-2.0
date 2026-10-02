using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class ApiController : MonoBehaviour
{
    public void CallApi(string url, Action<JObject> onSuccess, Action<string> onError)
    {
        StartCoroutine(SendRequest(url, onSuccess, onError));
    }

    IEnumerator SendRequest(string url, Action<JObject> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        
        request.SetRequestHeader("Accept", GameEnv.ACCEPT);
        request.SetRequestHeader("Access-Key", PlayerPrefs.GetString("accessKey", "431f053c-cf0a-44cc-a018-03c15e1ee142"));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                JObject jsonResponse = JObject.Parse(request.downloadHandler.text);

                onSuccess(jsonResponse);
            }
            catch (Exception e)
            {
                onError("Error Parsing Json:  " + e.Message);
            }
        }
        else
        {
            onError("Request failed: " + request.error);
        }
    }

    public void HandleError(string errorMessage)
    {
#if UNITY_EDITOR
        Debug.LogError("API Error: " + errorMessage);
#endif
    }

    public void DownloadImage(string imageUrl, Action<Sprite> callback)
    {
        StartCoroutine(AsyncDownloadImage(imageUrl, callback));
    }

    private IEnumerator AsyncDownloadImage(string imageUrl, Action<Sprite> callback)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            Debug.Log("URL is null or empty.");
            callback(null);
            yield break;
        }

        string encodedUrl = Uri.EscapeUriString(imageUrl);
        using UnityWebRequest request = new UnityWebRequest(encodedUrl);
    
        request.downloadHandler = new DownloadHandlerTexture();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
#if UNITY_EDITOR
            Debug.LogError("Error downloading image: " + request.error);
#endif
            callback(null);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            if (texture != null)
            {
                Sprite sprite = ConvertTextureToSprite(texture);
                callback(sprite);
            }
            else
            {
                Debug.LogError("Failed to load texture from the response.");
                callback(null);
            }
        }
    }
    
    // Helper coroutine to handle image download with error handling
    public IEnumerator SafeDownloadImage(string imageUrl, Action<Sprite> callback)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            Debug.Log("URL is null or empty.");
            callback(null);
            yield break;
        }

        string encodedUrl = Uri.EscapeUriString(imageUrl);
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(encodedUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error downloading image: " + request.error);
            callback(null);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            if (texture != null)
            {
                Sprite sprite = ConvertTextureToSprite(texture);
                callback(sprite);
            }
            else
            {
                Debug.LogError("Failed to load texture from the response.");
                callback(null);
            }
        }
    }

    private Sprite ConvertTextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    
    public void PostApi(string url, JObject postData, Action<JObject> onSuccess, Action<string> onError)
    {
        StartCoroutine(SendPostRequest(url, postData, onSuccess, onError));
    }

    IEnumerator SendPostRequest(string url, JObject postData, Action<JObject> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(postData.ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        // Set Header
        request.SetRequestHeader("Content-Type", GameEnv.ACCEPT);
        request.SetRequestHeader("Accept", GameEnv.ACCEPT);
        request.SetRequestHeader("Access-Key", PlayerPrefs.GetString("accessKey", "431f053c-cf0a-44cc-a018-03c15e1ee142"));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                JObject jsonResponse = JObject.Parse(request.downloadHandler.text);
                onSuccess(jsonResponse);
            }
            catch (Exception e)
            {
                onError("Error Parsing Json: " + e.Message);
            }
        }
        else
        {
            onError("Request failed: " + request.error);
        }
    }
}
