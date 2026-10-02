using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class LoginManager : MonoBehaviour
{
    [Header("General")] 
    [SerializeField] private LoadingView loadingView;

    [Header("PIN")] 
    [SerializeField] private GameObject pinPanel;
    [SerializeField] private ButtonSelect[] optionPinButton;
    [SerializeField] private ApiLogin apiLogin;
    [SerializeField] private GameObject wrongPinPanel;
    [SerializeField] private UnityEvent onLoginSuccess;

    [Header("Clinic")] 
    [SerializeField] private ClinicListManager clinicListManager;

    private void Start()
    {
        loadingView.ActiveLoadingPanel("Tunggu Mendapatkan list klinik");
        clinicListManager.Init();
        clinicListManager.OnGetClinicComplete += () => loadingView.DisableLoadingPanel();
    }

    [Button]
    public void StartLogin()
    {
        loadingView.ActiveLoadingPanel("Tunggu Mendapatkan Pin");
        StartCoroutine(CheckOptionPin());
    }

    IEnumerator CheckOptionPin()
    {
        while (!apiLogin.HasPinList)
        {
            apiLogin.GetOptionPin();
            loadingView.ActiveLoadingPanel("Tolong Buat Pin Terlebih Dahulu");
            yield return new WaitForSeconds(GameEnv.TIME_CONSTANT);
        }
        
        loadingView.DisableLoadingPanel();

        yield return new WaitForSeconds(1f);
        pinPanel.SetActive(true);
        SetupOptionPin();
    }

    IEnumerator LoginFailed()
    {
        wrongPinPanel.SetActive(true);
        pinPanel.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        wrongPinPanel.SetActive(false);
        StartCoroutine(CheckOptionPin());
    }

    private void SetupOptionPin()
    {
        for (int i = 0; i < optionPinButton.Length; i++)
        {
            optionPinButton[i].ResetOnclick();
            string pin = apiLogin.pinList[i];
            optionPinButton[i].SetText(pin);
            optionPinButton[i].AddOnclickButton(() => StartCoroutine(CheckPin(pin)));
        }
    }

    private IEnumerator CheckPin(string pin)
    {
        loadingView.ActiveLoadingPanel("Pengecekan Pin");
        // Memulai proses pengecekan pin dengan memanggil API
        yield return StartCoroutine(apiLogin.CheckPinCoroutine(pin));

        // Setelah API selesai, cek apakah login berhasil atau gagal
        if (apiLogin.IsLogin)
        {
            onLoginSuccess?.Invoke();
        }
        else
        {
            loadingView.DisableLoadingPanel();
            StartCoroutine(LoginFailed());
        }
    }
}
