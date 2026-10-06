using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// POST /device/handshake dan GET /device/pairing/status. Kontrak: inovia-main/docs/api-device.md
/// </summary>
public class ApiDevice : MonoBehaviour
{
    public const string StateReady = "ready";
    public const string StateDisabled = "disabled";
    public const string StateUnregistered = "unregistered";
    public const string StateReauthRequired = "reauth_required";

    // data.reason saat state = disabled
    public const string ReasonDeviceDisabled = "device_disabled";
    public const string ReasonClinicDisabled = "clinic_disabled";

    public const string PairingPending = "pending";
    public const string PairingApproved = "approved";
    public const string PairingExpired = "expired";

    private readonly string _handshakeUrl = GameEnv.BASE_URL + "/device/handshake";
    private readonly string _pairingStatusUrl = GameEnv.BASE_URL + "/device/pairing/status";

    [SerializeField] private ApiController apiController;

    public void Handshake(Action<ApiResponse, HandshakeData> onDone)
    {
        JObject postData = new JObject
        {
            ["hardware_id"] = DeviceAuth.HardwareId,
            ["model"] = DeviceAuth.Model,
            ["app_version"] = DeviceAuth.AppVersion
        };

        apiController.Post(_handshakeUrl, postData, response =>
        {
            HandshakeData data = response.Success ? ParseHandshake(response.Data as JObject) : null;
            onDone(response, data);
        });
    }

    public void PairingStatus(string pollToken, Action<ApiResponse, PairingStatusData> onDone)
    {
        var headers = new Dictionary<string, string> { ["X-Pairing-Token"] = pollToken };

        apiController.Get(_pairingStatusUrl, response =>
        {
            PairingStatusData data = null;
            if (response.Success && response.Data is JObject json)
            {
                data = new PairingStatusData
                {
                    status = json["status"]?.ToString(),
                    token = json["token"]?.ToString(),
                    expiresIn = json["expires_in"]?.Value<int?>() ?? 0
                };
            }
            onDone(response, data);
        }, headers);
    }

    private static HandshakeData ParseHandshake(JObject json)
    {
        if (json == null) return null;

        HandshakeData data = new HandshakeData
        {
            state = json["state"]?.ToString(),
            reason = json["reason"]?.ToString(),
            session = json["session"] as JObject
        };

        if (json["device"] is JObject device)
        {
            data.deviceName = device["name"]?.ToString();
            data.clinicTitle = device["clinic"]?["title"]?.ToString();
        }

        if (json["pairing"] is JObject pairing)
        {
            data.pairing = new PairingData
            {
                code = pairing["code"]?.ToString(),
                pollToken = pairing["poll_token"]?.ToString(),
                expiresIn = pairing["expires_in"]?.Value<int?>() ?? 0,
                interval = Mathf.Max(1, pairing["interval"]?.Value<int?>() ?? 3)
            };
        }

        data.knownDeviceName = json["known_device"]?["name"]?.ToString();
        return data;
    }
}

[Serializable]
public class HandshakeData
{
    public string state;
    public string reason;
    public string deviceName;
    public string clinicTitle;
    public string knownDeviceName;
    public PairingData pairing;
    [NonSerialized] public JObject session;
}

[Serializable]
public class PairingData
{
    public string code;
    public string pollToken;
    public int expiresIn;
    public int interval;

    // K7MQ2P -> K7M-Q2P
    public string DisplayCode => code != null && code.Length == 6 ? code.Substring(0, 3) + "-" + code.Substring(3) : code;
}

[Serializable]
public class PairingStatusData
{
    public string status;
    public string token;
    public int expiresIn;
}
