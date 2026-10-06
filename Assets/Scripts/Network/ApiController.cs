using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class ApiResponse
{
    public long StatusCode;
    public JObject Body;
    public string Code;          // "DEVICE_UNPAIRED" / "DEVICE_DISABLED" / "CLINIC_DISABLED", hanya pada 401/403
    public string Error;
    public bool IsNetworkError;  // timeout / koneksi putus: JANGAN hapus token, cukup retry

    public bool Success => Body != null && (Body["success"]?.Type == JTokenType.Boolean) && Body["success"].Value<bool>();
    public JToken Data => Body?["data"];
    public bool IsRateLimited => StatusCode == 429;
}

public class ApiController : MonoBehaviour
{
    public const string CodeDeviceUnpaired = "DEVICE_UNPAIRED";
    public const string CodeDeviceDisabled = "DEVICE_DISABLED";
    public const string CodeClinicDisabled = "CLINIC_DISABLED";

    // Dipicu dari endpoint ber-token mana pun. Token sudah dihapus sebelum OnDeviceUnpaired dipanggil.
    public static event Action OnDeviceUnpaired;
    // DEVICE_DISABLED atau CLINIC_DISABLED. Token TIDAK dihapus: begitu aktif lagi, handshake langsung "ready"
    // Argumen: kode error (CodeDeviceDisabled / CodeClinicDisabled)
    public static event Action<string> OnDeviceDisabled;

    public void Get(string url, Action<ApiResponse> onDone, Dictionary<string, string> headers = null)
    {
        StartCoroutine(Send(UnityWebRequest.Get(url), headers, onDone));
    }

    public void Post(string url, JObject postData, Action<ApiResponse> onDone, Dictionary<string, string> headers = null)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes((postData ?? new JObject()).ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", GameEnv.ACCEPT);

        StartCoroutine(Send(request, headers, onDone));
    }

    private IEnumerator Send(UnityWebRequest request, Dictionary<string, string> headers, Action<ApiResponse> onDone)
    {
        using (request)
        {
            request.timeout = GameEnv.REQUEST_TIMEOUT;
            request.SetRequestHeader("Accept", GameEnv.ACCEPT);

            if (DeviceAuth.HasToken)
            {
                request.SetRequestHeader("Authorization", "Bearer " + DeviceAuth.Token);
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            yield return request.SendWebRequest();

            ApiResponse response = new ApiResponse
            {
                StatusCode = request.responseCode,
                Error = request.error,
                IsNetworkError = request.result == UnityWebRequest.Result.ConnectionError
            };

            // Body 4xx tetap dibaca: status device ada di field "code", bukan di request.error
            string text = request.downloadHandler?.text;
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    response.Body = JObject.Parse(text);
                    response.Code = response.Body["code"]?.ToString();
                }
                catch (Exception e)
                {
                    response.Error ??= "Error Parsing Json: " + e.Message;
                }
            }

#if UNITY_EDITOR
            if (!response.Success)
            {
                Debug.LogWarning($"API {request.method} {request.url} -> {response.StatusCode} {response.Code} {response.Error}");
            }
#endif

            if (response.Code == CodeDeviceUnpaired)
            {
                DeviceAuth.ClearToken();
                OnDeviceUnpaired?.Invoke();
            }
            else if (response.Code == CodeDeviceDisabled || response.Code == CodeClinicDisabled)
            {
                OnDeviceDisabled?.Invoke(response.Code);
            }

            onDone?.Invoke(response);
        }
    }

    public void DownloadImage(string imageUrl, Action<Sprite> callback)
    {
        StartCoroutine(SafeDownloadImage(imageUrl, callback));
    }

    // Helper coroutine to handle image download with error handling
    public IEnumerator SafeDownloadImage(string imageUrl, Action<Sprite> callback)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            callback(null);
            yield break;
        }

        string encodedUrl = Uri.EscapeUriString(GameEnv.ToAbsoluteUrl(imageUrl));
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(encodedUrl);
        request.timeout = GameEnv.REQUEST_TIMEOUT;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading image: " + request.error);
            callback(null);
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        callback(texture != null ? ConvertTextureToSprite(texture) : null);
    }

    private Sprite ConvertTextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
