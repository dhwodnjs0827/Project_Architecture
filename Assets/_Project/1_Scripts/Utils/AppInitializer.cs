using UnityEngine;

/// <summary>
/// Application 초기화 클래스
/// </summary>
public static class AppInitializer
{
    /// <summary>
    /// 씬 로드 전 호출되는 메서드
    /// </summary>
    /// <remarks>
    /// Awake보다 먼저 호출됨
    /// </remarks>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeSceneLoad()
    {
        Application.targetFrameRate = 60;
    }
    
    /// <summary>
    /// 씬 로드 후 호출되는 메서드
    /// </summary>
    /// <remarks>
    /// 모든 Awake 호출 후 실행됨
    /// </remarks>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeAfterSceneLoad()
    {
        SceneLoadManager.Instance.Initialize();
    }
}
