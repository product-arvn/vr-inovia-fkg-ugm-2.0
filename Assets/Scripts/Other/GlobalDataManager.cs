using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDataManager : MonoBehaviour
{
    public static GlobalDataManager Instance;

    public string appVersion;

    [Header("From Setting")]
    public Language language;
    public float volume;

    [Header("From API")]
    public Credential playerCred;
    public PlayerData playerData;
    public DashboardData dashboardData;

    // Start is called before the first frame update
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDashboard(DashboardData data)
    {
        dashboardData = data;
    }

    public void SetPlayerData(string name, string npk, string token = "")
    {
        playerData.userToken = token;
        playerData.userName = name;
        playerData.userNPK = npk;
    }

    public void SaveCredential(string name, string password)
    {
        playerCred.username = name;
        playerCred.password = password;
    }
}

[Serializable]
public class PlayerData
{
    public string userToken;
    public string userName;
    public string userNPK;
}

[Serializable]
public class Credential
{
    public string username;
    public string password;
}

[Serializable]
public class DashboardData
{
    public string userName;
    public string userNPK;
    public string lastPlayDate;
    public string lastScore;
    public string avgScore;
    public string totalFinished;
}

[Serializable]
public enum Language { ID, EN }
