using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Jenkins의 Google Sheets 동기화 빌드를 트리거하는 에디터 윈도우.
/// Unity 에디터 메뉴 Tools > Google Sheets Sync에서 접근 가능.
/// </summary>
public class GoogleSheetsSync : EditorWindow
{
    #region EditorPrefs Keys
    // EditorPrefs에 저장할 때 사용하는 키 값들
    // EditorPrefs는 Unity 에디터의 설정을 로컬에 영구 저장함 (Windows: 레지스트리, Mac: plist)
    private const string JenkinsUrlKey = "GoogleSheetsSync_JenkinsUrl";
    private const string JenkinsUserKey = "GoogleSheetsSync_JenkinsUser";
    private const string JenkinsTokenKey = "GoogleSheetsSync_JenkinsToken";
    private const string JenkinsJobNameKey = "GoogleSheetsSync_JenkinsJobName";
    #endregion

    #region Settings Fields
    // Jenkins 연결 설정 값들
    private string jenkinsUrl;      // Jenkins 서버 URL (예: http://localhost:8080)
    private string jenkinsUser;     // Jenkins 사용자 ID
    private string jenkinsToken;    // Jenkins API Token (비밀번호 대신 사용)
    private string jenkinsJobName;  // 트리거할 Jenkins Job 이름
    #endregion

    #region State Fields
    // 현재 진행 중인 HTTP 요청 (null이면 요청 없음)
    private UnityWebRequestAsyncOperation currentRequest;

    // UI에 표시할 상태 메시지
    private string statusMessage = "";
    private MessageType statusType = MessageType.None;
    #endregion

    /// <summary>
    /// 에디터 메뉴에 "Tools/Google Sheets Sync" 항목 추가.
    /// MenuItem 속성은 Unity 에디터 메뉴에 항목을 등록함.
    /// </summary>
    [MenuItem("Tools/Data/Google Sheets Sync")]
    public static void ShowWindow()
    {
        // GetWindow: 이미 열려있으면 해당 윈도우 반환, 없으면 새로 생성
        var window = GetWindow<GoogleSheetsSync>("Google Sheets Sync");
        window.minSize = new Vector2(400, 250);
    }

    /// <summary>
    /// 윈도우가 활성화될 때 호출됨.
    /// EditorPrefs에서 저장된 설정값을 불러옴.
    /// </summary>
    private void OnEnable()
    {
        // GetString(key, defaultValue): 저장된 값이 없으면 기본값 사용
        jenkinsUrl = EditorPrefs.GetString(JenkinsUrlKey, "http://localhost:8080");
        jenkinsUser = EditorPrefs.GetString(JenkinsUserKey, "Jenkins User ID");
        jenkinsToken = EditorPrefs.GetString(JenkinsTokenKey, "Jenkins API Token");
        jenkinsJobName = EditorPrefs.GetString(JenkinsJobNameKey, "Jenkins Job Name");
    }

    /// <summary>
    /// 에디터 윈도우 GUI를 그리는 메서드.
    /// Unity IMGUI 방식으로 매 프레임 호출됨.
    /// </summary>
    private void OnGUI()
    {
        // === Jenkins 설정 섹션 ===
        GUILayout.Label("Jenkins Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // BeginChangeCheck/EndChangeCheck: 사이의 GUI 요소가 변경되었는지 감지
        EditorGUI.BeginChangeCheck();

        // 텍스트 필드로 설정 입력받기
        jenkinsUrl = EditorGUILayout.TextField("Jenkins URL", jenkinsUrl);
        jenkinsJobName = EditorGUILayout.TextField("Job Name", jenkinsJobName);

        // === 인증 섹션 ===
        EditorGUILayout.Space(10);
        GUILayout.Label("Authentication", EditorStyles.boldLabel);

        jenkinsUser = EditorGUILayout.TextField("User ID", jenkinsUser);
        // PasswordField: 입력값을 *로 마스킹하여 표시
        jenkinsToken = EditorGUILayout.PasswordField("API Token", jenkinsToken);

        // 설정값이 변경되었으면 저장
        if (EditorGUI.EndChangeCheck())
        {
            SaveSettings();
        }

        // === 빌드 트리거 버튼 ===
        EditorGUILayout.Space(15);

        // 요청 진행 중이면 버튼 비활성화 (중복 요청 방지)
        EditorGUI.BeginDisabledGroup(currentRequest != null && !currentRequest.isDone);

        if (GUILayout.Button("Trigger Sheets Build", GUILayout.Height(40)))
        {
            TriggerJenkinsBuild();
        }

        EditorGUI.EndDisabledGroup();

        // === 상태 표시 ===
        // 요청 진행 중이면 "Building..." 메시지 표시
        if (currentRequest != null && !currentRequest.isDone)
        {
            EditorGUILayout.HelpBox("Building...", MessageType.Info);
        }

        // 결과 메시지가 있으면 표시 (성공/실패)
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(statusMessage, statusType);
        }
    }

    /// <summary>
    /// 현재 설정값을 EditorPrefs에 저장.
    /// Unity 에디터를 닫아도 설정이 유지됨.
    /// </summary>
    private void SaveSettings()
    {
        EditorPrefs.SetString(JenkinsUrlKey, jenkinsUrl);
        EditorPrefs.SetString(JenkinsUserKey, jenkinsUser);
        EditorPrefs.SetString(JenkinsTokenKey, jenkinsToken);
        EditorPrefs.SetString(JenkinsJobNameKey, jenkinsJobName);
    }

    /// <summary>
    /// Jenkins 빌드를 트리거하는 HTTP POST 요청 전송.
    /// Jenkins Remote Access API 사용: POST /job/{jobName}/build
    /// </summary>
    private void TriggerJenkinsBuild()
    {
        // 필수 설정값 검증
        if (string.IsNullOrEmpty(jenkinsUrl) || string.IsNullOrEmpty(jenkinsJobName))
        {
            statusMessage = "Jenkins URL and Job Name are required.";
            statusType = MessageType.Error;
            return;
        }

        // Jenkins 빌드 트리거 URL 구성
        // 예: http://localhost:8080/job/google-sheets-sync/build
        string buildUrl = $"{jenkinsUrl.TrimEnd('/')}/job/{jenkinsJobName}/build";

        // UnityWebRequest로 POST 요청 생성
        var request = new UnityWebRequest(buildUrl, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();

        // 인증 정보가 있으면 Basic Auth 헤더 추가
        // Basic Auth 형식: "Basic Base64(username:password)"
        if (!string.IsNullOrEmpty(jenkinsUser) && !string.IsNullOrEmpty(jenkinsToken))
        {
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{jenkinsUser}:{jenkinsToken}"));
            request.SetRequestHeader("Authorization", $"Basic {auth}");
        }

        // 상태 초기화 및 비동기 요청 시작
        statusMessage = "";
        currentRequest = request.SendWebRequest();

        // 요청 완료 시 콜백 등록
        currentRequest.completed += OnBuildRequestCompleted;

        // 로딩 중 UI 갱신을 위해 Update 이벤트에 등록
        EditorApplication.update += RepaintWhileLoading;
    }

    /// <summary>
    /// 요청 진행 중 UI를 주기적으로 갱신.
    /// EditorApplication.update에 등록되어 매 프레임 호출됨.
    /// </summary>
    private void RepaintWhileLoading()
    {
        // 요청이 완료되면 Update 이벤트에서 제거
        if (currentRequest == null || currentRequest.isDone)
        {
            EditorApplication.update -= RepaintWhileLoading;
            return;
        }

        // 윈도우 다시 그리기 (로딩 상태 표시 갱신)
        Repaint();
    }

    /// <summary>
    /// HTTP 요청 완료 시 호출되는 콜백.
    /// 성공/실패에 따라 상태 메시지 업데이트.
    /// </summary>
    /// <param name="op">완료된 비동기 작업</param>
    private void OnBuildRequestCompleted(AsyncOperation op)
    {
        var asyncOp = (UnityWebRequestAsyncOperation)op;
        var request = asyncOp.webRequest;

        // Jenkins는 빌드 트리거 성공 시 201 Created 반환
        if (request.result == UnityWebRequest.Result.Success ||
            request.responseCode == 201)
        {
            statusMessage = $"Build triggered successfully! (HTTP {request.responseCode})";
            statusType = MessageType.Info;
            Debug.Log($"[GoogleSheetsSync] Jenkins build triggered: {jenkinsJobName}");
        }
        else
        {
            // 실패 시 에러 메시지 표시
            statusMessage = $"Failed to trigger build.\nHTTP {request.responseCode}: {request.error}";
            statusType = MessageType.Error;
            Debug.LogError($"[GoogleSheetsSync] Failed to trigger Jenkins build: {request.error}");
        }

        // 리소스 정리
        request.Dispose();
        currentRequest = null;

        // UI 갱신하여 결과 표시
        Repaint();
    }
}