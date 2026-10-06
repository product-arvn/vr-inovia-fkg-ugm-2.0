using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pindah otomatis ke scene lain saat Start. Dipakai Main Menu: alur klinik/PIN/pasien sudah diganti
/// pairing perangkat di Api MainScene.
/// </summary>
public class AutoLoadScene : MonoBehaviour
{
    [SerializeField] private string sceneName = "Api MainScene";
    [SerializeField] private float delay = 0.5f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
