using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// State machine headset: handshake -> (pairing | disabled) -> loop sesi.
/// Kontrak: inovia-main/docs/api-device.md
/// Semua jeda memakai waktu realtime karena Time.timeScale = 0 saat konten dibekukan.
/// </summary>
public class GameManagerApi : MonoBehaviour
{
    [SerializeField] private LoadingView loadingView;
    [SerializeField] private ContentPatientManager contentPatientManager;
    [SerializeField] private ApiDevice apiDevice;
    [SerializeField] private DevicePairingView pairingView;

    private DataHandler _dataHandler;
    // Timer header modul (waktu sesi berjalan). Dulu dimulai tombol Start di panel pilih pasien yang sudah dihapus.
    private GlobalTimer _globalTimer;
    private string _timerSessionId;
    private Coroutine _sessionLoop;
    private bool _inSession;
    private bool _isReloading;
    // Perangkat/klinik dinonaktifkan saat sesi berjalan: konten dibekukan, handshake diulang sampai aktif lagi
    private bool _suspended;
    private bool _sessionPaused;

    [ShowNonSerializedField] private string _deviceName;
    [ShowNonSerializedField] private string _clinicTitle;

    private void Awake()
    {
        loadingView.ActiveLoadingPanel("Menghubungkan perangkat ...");
    }

    private void Start()
    {
        _dataHandler = DataHandler.Instance;
        _globalTimer = FindFirstObjectByType<GlobalTimer>();
        SetFrozen(false);

        ApiController.OnDeviceUnpaired += HandleDeviceUnpaired;
        ApiController.OnDeviceDisabled += HandleDeviceDisabled;

        contentPatientManager.ContentChanged += () => EnableContentPanel(contentPatientManager.HasContent);
        contentPatientManager.ContentChanged += StartSessionTimer;
        contentPatientManager.WaitingContent += ShowWaiting;
        contentPatientManager.WhenEndContent += ContentEnd;
        contentPatientManager.SessionEnd += () => loadingView.DisableLoadingPanel();
        contentPatientManager.SessionPausedChanged += HandleSessionPaused;

        pairingView.HideAll();
        StartCoroutine(DeviceFlow());
    }

    private void OnDestroy()
    {
        ApiController.OnDeviceUnpaired -= HandleDeviceUnpaired;
        ApiController.OnDeviceDisabled -= HandleDeviceDisabled;
        SetFrozen(false);
    }

    private IEnumerator DeviceFlow()
    {
        while (true)
        {
            if (!_suspended) loadingView.ActiveLoadingPanel("Menghubungkan perangkat ...");

            ApiResponse response = null;
            HandshakeData data = null;
            apiDevice.Handshake((r, d) =>
            {
                response = r;
                data = d;
            });
            yield return new WaitUntil(() => response != null);

            if (data == null)
            {
                // Jaringan putus / server error / rate limit: token TIDAK dihapus, cukup coba lagi
                if (!_suspended)
                {
                    loadingView.ActiveLoadingPanel(response.IsNetworkError
                        ? "Tidak dapat terhubung ke server.\nMencoba lagi ..."
                        : "Server belum merespons.\nMencoba lagi ...");
                }
                yield return new WaitForSecondsRealtime(response.IsRateLimited
                    ? GameEnv.RATE_LIMIT_RETRY_INTERVAL
                    : GameEnv.NETWORK_RETRY_INTERVAL);
                continue;
            }

            switch (data.state)
            {
                case ApiDevice.StateReady:
                    _deviceName = data.deviceName;
                    _clinicTitle = data.clinicTitle;
                    pairingView.HideAll();
                    StartSession();
                    yield break;

                case ApiDevice.StateDisabled:
                    loadingView.DisableLoadingPanel();
                    pairingView.ShowDisabled(data.deviceName ?? _deviceName, data.reason);
                    yield return new WaitForSecondsRealtime(GameEnv.DISABLED_RETRY_INTERVAL);
                    break;

                case ApiDevice.StateUnregistered:
                case ApiDevice.StateReauthRequired:
                    DeviceAuth.ClearToken();
                    if (contentPatientManager.HasActiveContent)
                    {
                        // Perangkat dihapus saat konten masih tampil: mulai bersih dari scene baru
                        ReloadScene();
                        yield break;
                    }

                    loadingView.DisableLoadingPanel();
                    if (data.pairing != null)
                    {
                        yield return StartCoroutine(PairingFlow(data.pairing, data.knownDeviceName));
                    }
                    else
                    {
                        yield return new WaitForSecondsRealtime(GameEnv.NETWORK_RETRY_INTERVAL);
                    }
                    break;

                default:
                    Debug.LogError("State handshake tidak dikenal: " + data.state);
                    yield return new WaitForSecondsRealtime(GameEnv.NETWORK_RETRY_INTERVAL);
                    break;
            }
        }
    }

    // Tampilkan kode dan polling status sampai approved / expired. Selesai -> kembali ke handshake.
    private IEnumerator PairingFlow(PairingData pairing, string knownDeviceName)
    {
        pairingView.ShowPairing(pairing, knownDeviceName);
        float expiresAt = Time.realtimeSinceStartup + pairing.expiresIn;

        while (true)
        {
            for (int i = 0; i < pairing.interval; i++)
            {
                pairingView.SetExpiry(Mathf.CeilToInt(expiresAt - Time.realtimeSinceStartup));
                yield return new WaitForSecondsRealtime(1f);
            }

            if (Time.realtimeSinceStartup >= expiresAt) yield break;

            ApiResponse response = null;
            PairingStatusData status = null;
            apiDevice.PairingStatus(pairing.pollToken, (r, s) =>
            {
                response = r;
                status = s;
            });
            yield return new WaitUntil(() => response != null);

            if (status == null)
            {
                if (response.IsRateLimited)
                {
                    yield return new WaitForSecondsRealtime(GameEnv.RATE_LIMIT_RETRY_INTERVAL);
                    continue;
                }

                // poll_token ditolak (4xx): minta kode/poll_token baru lewat handshake
                if (!response.IsNetworkError && response.StatusCode >= 400 && response.StatusCode < 500) yield break;
                continue;
            }

            switch (status.status)
            {
                case ApiDevice.PairingApproved:
                    if (string.IsNullOrEmpty(status.token))
                    {
                        Debug.LogError("Pairing approved tanpa token");
                        yield break;
                    }
                    // Token hanya dikirim sekali: simpan sebelum apa pun
                    DeviceAuth.SaveToken(status.token);
                    pairingView.HideAll();
                    yield break;

                case ApiDevice.PairingExpired:
                    yield break;

                default: // pending
                    if (status.expiresIn > 0) expiresAt = Time.realtimeSinceStartup + status.expiresIn;
                    break;
            }
        }
    }

    private void StartSession()
    {
        _inSession = true;

        if (_suspended)
        {
            // Kembali dari nonaktif: konten tetap dibekukan sampai polling pertama menentukan status sesi
            _suspended = false;
            contentPatientManager.ResetPausedState();
        }
        else
        {
            ShowWaiting(false);
        }

        _sessionLoop = StartCoroutine(contentPatientManager.CheckContent());
    }

    // 401 DEVICE_UNPAIRED (token sudah dihapus ApiController): muat ulang scene, handshake akan meminta pairing
    private void HandleDeviceUnpaired()
    {
        if (!_inSession) return;
        ReloadScene();
    }

    // 403 DEVICE_DISABLED / CLINIC_DISABLED saat sesi berjalan: token tetap, konten dibekukan (tidak ditutup),
    // lalu handshake tiap 30 detik. Setelah aktif lagi sesi biasanya "paused" sampai dokter melanjutkan.
    private void HandleDeviceDisabled(string code)
    {
        if (!_inSession || _suspended || _isReloading) return;

        _inSession = false;
        _suspended = true;
        if (_sessionLoop != null) StopCoroutine(_sessionLoop);

        SetFrozen(true);
        loadingView.DisableLoadingPanel();
        pairingView.ShowDisabled(_deviceName, code == ApiController.CodeClinicDisabled
            ? ApiDevice.ReasonClinicDisabled
            : ApiDevice.ReasonDeviceDisabled);

        StartCoroutine(DeviceFlow());
    }

    private void HandleSessionPaused(bool paused)
    {
        _sessionPaused = paused;
        SetFrozen(paused);

        if (paused && contentPatientManager.HasActiveContent)
        {
            pairingView.ShowPaused();
        }
        else
        {
            pairingView.HideAll();
            if (!contentPatientManager.HasActiveContent) ShowWaiting(true);
        }
    }

    private void SetFrozen(bool frozen)
    {
        contentPatientManager.SetFrozen(frozen);
    }

    private void ReloadScene()
    {
        if (_isReloading) return;
        _isReloading = true;
        StopAllCoroutines();
        SetFrozen(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowWaiting(bool hasSession)
    {
        // Baris jenis sesi (Pre-Clinics / On-Clinics); disembunyikan untuk sesi lama tanpa kategori
        string sessionType = hasSession ? contentPatientManager.CurrentSessionTypeLabel : null;
        string typeLine = string.IsNullOrEmpty(sessionType) ? "" : $"<b>Sesi {sessionType}</b>\n";

        if (hasSession && _sessionPaused)
        {
            loadingView.ActiveLoadingPanel($"{typeLine}Sesi dijeda.\nMenunggu dokter melanjutkan ...");
            return;
        }

        string patientName = _dataHandler != null ? _dataHandler.GetPatientSelectedName() : null;

        if (hasSession && !string.IsNullOrEmpty(patientName))
        {
            loadingView.ActiveLoadingPanel($"{typeLine}Menunggu konten untuk <color=blue>{patientName}</color> ...");
        }
        else if (hasSession)
        {
            loadingView.ActiveLoadingPanel($"{typeLine}Menunggu konten dari dokter ...");
        }
        else
        {
            string device = string.IsNullOrEmpty(_deviceName) ? "" : $"<b>{_deviceName}</b>\n";
            loadingView.ActiveLoadingPanel($"{device}Menunggu sesi dari dokter ...");
        }
    }

    // Mulai saat konten pertama sesi tampil; sesi baru di headset yang sama mulai lagi dari 00:00
    private void StartSessionTimer()
    {
        if (_globalTimer == null) return;

        string sessionId = contentPatientManager.CurrentSessionId;
        if (sessionId == _timerSessionId && !_globalTimer.IsPaused) return;

        if (sessionId != _timerSessionId) _globalTimer.StopTimer();
        _globalTimer.StartTimer();
        _timerSessionId = sessionId;
    }

    private void ContentEnd()
    {
        loadingView.ActiveLoadingPanel("Menyimpan hasil ...");
        contentPatientManager.isContentDone = true;
    }

    private void EnableContentPanel(bool activate)
    {
        if (activate)
        {
            loadingView.DisableLoadingPanel();
        }
        else
        {
            ShowWaiting(true);
        }
    }
}
