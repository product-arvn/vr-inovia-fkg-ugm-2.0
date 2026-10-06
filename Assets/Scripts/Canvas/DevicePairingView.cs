using UnityEngine;
using TMPro;

/// <summary>
/// Layar kode pairing dan layar "perangkat dinonaktifkan".
/// </summary>
public class DevicePairingView : MonoBehaviour
{
    [Header("Pairing")]
    [SerializeField] private GameObject pairingPanel;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text expiryText;

    [Header("Disabled")]
    [SerializeField] private GameObject disabledPanel;
    [SerializeField] private TMP_Text disabledText;

    public void ShowPairing(PairingData pairing, string knownDeviceName)
    {
        disabledPanel.SetActive(false);
        pairingPanel.SetActive(true);
        codeText.text = pairing.DisplayCode;

        instructionText.text = string.IsNullOrEmpty(knownDeviceName)
            ? "Buka dashboard Inovia > Perangkat VR, lalu masukkan kode ini."
            : $"Perangkat dikenali sebagai <b>{knownDeviceName}</b>.\nMinta admin klik \"Setujui\" di dashboard Inovia > Perangkat VR.";
    }

    public void SetExpiry(int secondsLeft)
    {
        if (expiryText == null) return;
        int minutes = Mathf.Max(0, secondsLeft) / 60;
        int seconds = Mathf.Max(0, secondsLeft) % 60;
        expiryText.text = $"Kode berlaku {minutes:00}:{seconds:00}";
    }

    public void ShowDisabled(string deviceName, string reason)
    {
        pairingPanel.SetActive(false);
        disabledPanel.SetActive(true);

        if (reason == ApiDevice.ReasonClinicDisabled)
        {
            disabledText.text = "Akses klinik sedang dinonaktifkan.\nHubungi admin klinik.";
            return;
        }

        disabledText.text = string.IsNullOrEmpty(deviceName)
            ? "Perangkat dinonaktifkan admin klinik."
            : $"Perangkat <b>{deviceName}</b> dinonaktifkan admin klinik.";
    }

    // Overlay di atas konten yang sedang dibekukan
    public void ShowPaused()
    {
        pairingPanel.SetActive(false);
        disabledPanel.SetActive(true);
        disabledText.text = "Sesi dijeda.\nMenunggu dokter melanjutkan ...";
    }

    public void HideAll()
    {
        pairingPanel.SetActive(false);
        disabledPanel.SetActive(false);
    }
}
