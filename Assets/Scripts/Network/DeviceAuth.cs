using UnityEngine;

/// <summary>
/// Identitas dan token headset untuk Device API.
/// hardware_id JANGAN diganti sumbernya di versi APK berikutnya: jika berubah, server mencabut token.
/// Di Android nilainya turunan ANDROID_ID yang terikat signing key, jadi semua build wajib memakai keystore yang sama.
/// </summary>
public static class DeviceAuth
{
    private const string TokenKey = "deviceToken";

    public static string HardwareId => SystemInfo.deviceUniqueIdentifier;

    public static string Model => SystemInfo.deviceModel;

    public static string AppVersion => Application.version;

    public static string Token => PlayerPrefs.GetString(TokenKey, string.Empty);

    public static bool HasToken => !string.IsNullOrEmpty(Token);

    public static void SaveToken(string token)
    {
        PlayerPrefs.SetString(TokenKey, token);
        PlayerPrefs.Save();
    }

    public static void ClearToken()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.Save();
    }
}
