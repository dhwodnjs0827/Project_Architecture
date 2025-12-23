using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetsSync : EditorWindow
{
    private const string JenkinsUrlKey = "GoogleSheetsSync_JenkinsUrl";
    private const string JenkinsUserKey = "GoogleSheetsSync_JenkinsUser";
    private const string JenkinsTokenKey = "GoogleSheetsSync_JenkinsToken";
    private const string JenkinsJobNameKey = "GoogleSheetsSync_JenkinsJobName";

    private string jenkinsUrl;
    private string jenkinsUser;
    private string jenkinsToken;
    private string jenkinsJobName;

    private UnityWebRequestAsyncOperation currentRequest;
    private string statusMessage = "";
    private MessageType statusType = MessageType.None;

    [MenuItem("Tools/Google Sheets Sync")]
    public static void ShowWindow()
    {
        var window = GetWindow<GoogleSheetsSync>("Google Sheets Sync");
        window.minSize = new Vector2(400, 250);
    }

    private void OnEnable()
    {
        jenkinsUrl = EditorPrefs.GetString(JenkinsUrlKey, "http://localhost:8080");
        jenkinsUser = EditorPrefs.GetString(JenkinsUserKey, "dhwodnjs0827");
        jenkinsToken = EditorPrefs.GetString(JenkinsTokenKey, "1124e901afb2c126bc595908d101e6d891");
        jenkinsJobName = EditorPrefs.GetString(JenkinsJobNameKey, "google-sheets-sync");
    }

    private void OnGUI()
    {
        GUILayout.Label("Jenkins Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUI.BeginChangeCheck();

        jenkinsUrl = EditorGUILayout.TextField("Jenkins URL", jenkinsUrl);
        jenkinsJobName = EditorGUILayout.TextField("Job Name", jenkinsJobName);

        EditorGUILayout.Space(10);
        GUILayout.Label("Authentication", EditorStyles.boldLabel);

        jenkinsUser = EditorGUILayout.TextField("User ID", jenkinsUser);
        jenkinsToken = EditorGUILayout.PasswordField("API Token", jenkinsToken);

        if (EditorGUI.EndChangeCheck())
        {
            SaveSettings();
        }

        EditorGUILayout.Space(15);

        EditorGUI.BeginDisabledGroup(currentRequest != null && !currentRequest.isDone);

        if (GUILayout.Button("Trigger Sheets Build", GUILayout.Height(40)))
        {
            TriggerJenkinsBuild();
        }

        EditorGUI.EndDisabledGroup();

        if (currentRequest != null && !currentRequest.isDone)
        {
            EditorGUILayout.HelpBox("Building...", MessageType.Info);
        }

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(statusMessage, statusType);
        }
    }

    private void SaveSettings()
    {
        EditorPrefs.SetString(JenkinsUrlKey, jenkinsUrl);
        EditorPrefs.SetString(JenkinsUserKey, jenkinsUser);
        EditorPrefs.SetString(JenkinsTokenKey, jenkinsToken);
        EditorPrefs.SetString(JenkinsJobNameKey, jenkinsJobName);
    }

    private void TriggerJenkinsBuild()
    {
        if (string.IsNullOrEmpty(jenkinsUrl) || string.IsNullOrEmpty(jenkinsJobName))
        {
            statusMessage = "Jenkins URL and Job Name are required.";
            statusType = MessageType.Error;
            return;
        }

        string buildUrl = $"{jenkinsUrl.TrimEnd('/')}/job/{jenkinsJobName}/build";

        var request = new UnityWebRequest(buildUrl, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();

        if (!string.IsNullOrEmpty(jenkinsUser) && !string.IsNullOrEmpty(jenkinsToken))
        {
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{jenkinsUser}:{jenkinsToken}"));
            request.SetRequestHeader("Authorization", $"Basic {auth}");
        }

        statusMessage = "";
        currentRequest = request.SendWebRequest();
        currentRequest.completed += OnBuildRequestCompleted;

        EditorApplication.update += RepaintWhileLoading;
    }

    private void RepaintWhileLoading()
    {
        if (currentRequest == null || currentRequest.isDone)
        {
            EditorApplication.update -= RepaintWhileLoading;
            return;
        }
        Repaint();
    }

    private void OnBuildRequestCompleted(AsyncOperation op)
    {
        var asyncOp = (UnityWebRequestAsyncOperation)op;
        var request = asyncOp.webRequest;

        if (request.result == UnityWebRequest.Result.Success ||
            request.responseCode == 201)
        {
            statusMessage = $"Build triggered successfully! (HTTP {request.responseCode})";
            statusType = MessageType.Info;
            Debug.Log($"[GoogleSheetsSync] Jenkins build triggered: {jenkinsJobName}");
        }
        else
        {
            statusMessage = $"Failed to trigger build.\nHTTP {request.responseCode}: {request.error}";
            statusType = MessageType.Error;
            Debug.LogError($"[GoogleSheetsSync] Failed to trigger Jenkins build: {request.error}");
        }

        request.Dispose();
        currentRequest = null;
        Repaint();
    }
}