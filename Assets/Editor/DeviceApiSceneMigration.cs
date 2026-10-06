using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Migrasi scene dari alur Access-Key + PIN + pilih pasien ke Device API (handshake/pairing/token).
/// Aman dijalankan ulang: langkah yang sudah diterapkan dilewati.
/// Menu: Tools > Inovia > Migrasi Scene ke Device API
/// Batch: Unity.exe -batchmode -projectPath . -executeMethod DeviceApiSceneMigration.RunBatch -quit
/// </summary>
public static class DeviceApiSceneMigration
{
    private const string Tag = "[DeviceMigration] ";
    private const string MainMenuPath = "Assets/Scenes/Main Menu.unity";
    private const string ApiMainScenePath = "Assets/Scenes/Api MainScene.unity";
    private const string PairingCanvasName = "Device Pairing";

    [MenuItem("Tools/Inovia/Migrasi Scene ke Device API")]
    public static void RunFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string originalScene = SceneManager.GetActiveScene().path;
        bool ok = Migrate();

        if (!string.IsNullOrEmpty(originalScene)) EditorSceneManager.OpenScene(originalScene);
        EditorUtility.DisplayDialog("Migrasi Device API",
            ok ? "Selesai. Detail ada di Console (prefix [DeviceMigration])." : "Gagal. Lihat Console.", "OK");
    }

    public static void RunBatch()
    {
        if (!Migrate()) EditorApplication.Exit(1);
    }

    private static bool Migrate()
    {
        try
        {
            MigrateMainMenu();
            MigrateApiMainScene();
            AssetDatabase.SaveAssets();
            Debug.Log(Tag + "SELESAI");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(Tag + "GAGAL: " + e);
            return false;
        }
    }

    // Main Menu: hapus komponen login lama, matikan panel klinik/PIN/pasien, langsung pindah ke Api MainScene
    private static void MigrateMainMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);

        GameObject loginManager = Require(scene, "Login Manager");
        RemoveMissingScripts(loginManager);
        RemoveMissingScripts(Require(scene, "API Manager"));

        if (loginManager.GetComponent<AutoLoadScene>() == null)
        {
            var autoLoad = loginManager.AddComponent<AutoLoadScene>();
            var so = new SerializedObject(autoLoad);
            so.FindProperty("sceneName").stringValue = "Api MainScene";
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log(Tag + "Main Menu: AutoLoadScene -> Api MainScene ditambahkan ke 'Login Manager'");
        }

        foreach (string panel in new[] { "Welcome Panel", "Choose Patient Panel", "Choose Pin Panel", "Wrong", "Choose Clinic" })
        {
            GameObject go = Find(scene, panel);
            if (go != null && go.activeSelf)
            {
                go.SetActive(false);
                Debug.Log(Tag + $"Main Menu: '{panel}' dinonaktifkan");
            }
        }

        // Tampilkan loading selama pindah scene
        GameObject loading = Find(scene, "Loading", parentName: "login Menu");
        if (loading != null) loading.SetActive(true);

        Save(scene);
    }

    // Api MainScene: ApiDevice + layar pairing, hapus pemilihan pasien
    private static void MigrateApiMainScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ApiMainScenePath, OpenSceneMode.Single);

        GameObject apiManager = Require(scene, "API Manager");
        GameObject gameManager = Require(scene, "Game Manager");
        RemoveMissingScripts(apiManager);
        RemoveMissingScripts(gameManager);

        // Cek null eksplisit: operator ?? melewati null-check UnityEngine.Object
        ApiController apiController = apiManager.GetComponent<ApiController>();
        if (apiController == null) throw new Exception("ApiController tidak ada di 'API Manager'");

        ApiDevice apiDevice = apiManager.GetComponent<ApiDevice>();
        if (apiDevice == null)
        {
            apiDevice = apiManager.AddComponent<ApiDevice>();
            Debug.Log(Tag + "Api MainScene: ApiDevice ditambahkan ke 'API Manager'");
        }
        SetReference(apiDevice, "apiController", apiController);

        // Panel pilih pasien (prefab Choose Patient) tidak dipakai lagi
        GameObject choosePatient = Find(scene, "Choose Patient", parentName: "UI");
        if (choosePatient != null)
        {
            Object.DestroyImmediate(choosePatient);
            Debug.Log(Tag + "Api MainScene: panel 'Choose Patient' dihapus");
        }

        DevicePairingView pairingView = CreatePairingView(scene);

        GameManagerApi gameManagerApi = gameManager.GetComponent<GameManagerApi>();
        if (gameManagerApi == null) throw new Exception("GameManagerApi tidak ada di 'Game Manager'");
        SetReference(gameManagerApi, "apiDevice", apiDevice);
        SetReference(gameManagerApi, "pairingView", pairingView);

        Save(scene);
    }

    // Duplikat canvas "Not Found Session" (world space, sudah diposisikan di depan pemain) jadi layar pairing + nonaktif
    private static DevicePairingView CreatePairingView(Scene scene)
    {
        GameObject existing = Find(scene, PairingCanvasName);
        if (existing != null)
        {
            Debug.Log(Tag + "Api MainScene: '" + PairingCanvasName + "' sudah ada, dilewati");
            DevicePairingView existingView = existing.GetComponent<DevicePairingView>();
            if (existingView == null) throw new Exception($"'{PairingCanvasName}' ada tanpa DevicePairingView");
            return existingView;
        }

        GameObject source = Require(scene, "Not Found Session");
        GameObject canvas = Object.Instantiate(source, source.transform.parent);
        canvas.name = PairingCanvasName;
        canvas.SetActive(true);

        Transform pairingPanel = canvas.transform.Find("Not Found");
        if (pairingPanel == null) throw new Exception("'Not Found' tidak ditemukan");
        pairingPanel.name = "Pairing Panel";
        Transform disabledPanel = Object.Instantiate(pairingPanel.gameObject, canvas.transform).transform;
        disabledPanel.name = "Disabled Panel";

        foreach (Transform panel in new[] { pairingPanel, disabledPanel })
        {
            Transform spinner = panel.Find("Loading");
            if (spinner != null) Object.DestroyImmediate(spinner.gameObject);
        }

        TMP_Text template = pairingPanel.Find("Tittle Text (TMP)").GetComponent<TMP_Text>();

        TMP_Text codeText = CloneText(template, pairingPanel, "Code Text", "XXX-XXX", -62, 70, 44, FontStyles.Bold, false);
        TMP_Text expiryText = CloneText(template, pairingPanel, "Expiry Text", "Kode berlaku 10:00", -200, 24, 11, FontStyles.Normal, false);
        TMP_Text instructionText = template;
        instructionText.name = "Instruction Text";
        SetText(instructionText, "Buka dashboard Inovia > Perangkat VR, lalu masukkan kode ini.", -140, 64, 14, FontStyles.Normal, true);

        TMP_Text disabledText = disabledPanel.Find("Tittle Text (TMP)").GetComponent<TMP_Text>();
        disabledText.name = "Disabled Text";
        SetText(disabledText, "Perangkat dinonaktifkan admin klinik.", -115, 120, 18, FontStyles.Bold, true);

        var view = canvas.AddComponent<DevicePairingView>();
        SetReference(view, "pairingPanel", pairingPanel.gameObject);
        SetReference(view, "codeText", codeText);
        SetReference(view, "instructionText", instructionText);
        SetReference(view, "expiryText", expiryText);
        SetReference(view, "disabledPanel", disabledPanel.gameObject);
        SetReference(view, "disabledText", disabledText);

        pairingPanel.gameObject.SetActive(false);
        disabledPanel.gameObject.SetActive(false);

        Debug.Log(Tag + "Api MainScene: canvas '" + PairingCanvasName + "' dibuat dari 'Not Found Session'");
        return view;
    }

    private static TMP_Text CloneText(TMP_Text template, Transform parent, string name, string text,
        float y, float height, float fontSize, FontStyles style, bool autoSize)
    {
        TMP_Text clone = Object.Instantiate(template.gameObject, parent).GetComponent<TMP_Text>();
        clone.name = name;
        SetText(clone, text, y, height, fontSize, style, autoSize);
        return clone;
    }

    private static void SetText(TMP_Text label, string text, float y, float height, float fontSize, FontStyles style, bool autoSize)
    {
        var rect = (RectTransform)label.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(320, height);

        label.text = text;
        label.enableAutoSizing = autoSize;
        label.fontSizeMin = 8;
        label.fontSizeMax = fontSize;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.horizontalAlignment = HorizontalAlignmentOptions.Center;
        label.verticalAlignment = VerticalAlignmentOptions.Middle;
    }

    private static void SetReference(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field) ?? throw new Exception($"Field '{field}' tidak ada di {target.GetType().Name}");
        property.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void RemoveMissingScripts(GameObject go)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (removed > 0) Debug.Log(Tag + $"{go.scene.name}: {removed} komponen script lama dihapus dari '{go.name}'");
    }

    private static GameObject Find(Scene scene, string name, string parentName = null)
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Where(t => t.name == name && (parentName == null || (t.parent != null && t.parent.name == parentName)))
            .Select(t => t.gameObject)
            .FirstOrDefault();
    }

    private static GameObject Require(Scene scene, string name)
    {
        return Find(scene, name) ?? throw new Exception($"GameObject '{name}' tidak ditemukan di {scene.name}");
    }

    private static void Save(Scene scene)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Gagal menyimpan " + scene.path);
        Debug.Log(Tag + "Disimpan: " + scene.path);
    }
}
