public class GameEnv
{
    // Main Key
    public const string PROD_URL = "https://inovia.saviar.id/api";
    // Server lokal backend (php artisan serve --host=0.0.0.0). Ganti dengan IP LAN laptop backend.
    // Build ke headset butuh Player Settings > "Allow downloads over HTTP" (khusus build dev).
    public const string DEV_URL = "http://192.168.1.10:8000/api";

#if INOVIA_DEV_SERVER
    public const string BASE_URL = DEV_URL;
#else
    public const string BASE_URL = PROD_URL;
#endif

    public const string ACCEPT = "application/json";
    public const float TIME_CONSTANT = 6f;

    // Network
    public const int REQUEST_TIMEOUT = 15;          // detik
    public const float SESSION_POLL_INTERVAL = 4f;  // detik, kontrak: 3-5 detik
    public const float DISABLED_RETRY_INTERVAL = 30f;
    public const float NETWORK_RETRY_INTERVAL = 8f; // kontrak: 5-10 detik
    public const float RATE_LIMIT_RETRY_INTERVAL = 15f;

    // Origin server (BASE_URL tanpa "/api"), untuk file storage seperti video upload
    public static string ServerOrigin => BASE_URL.EndsWith("/api") ? BASE_URL.Substring(0, BASE_URL.Length - 4) : BASE_URL;

    // "/storage/..." -> "https://server/storage/...". URL absolut dikembalikan apa adanya.
    public static string ToAbsoluteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        url = url.Trim();
        if (url.StartsWith("http://") || url.StartsWith("https://")) return url;
        return ServerOrigin + (url.StartsWith("/") ? url : "/" + url);
    }
}
