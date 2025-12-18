using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene 전환 및 초기화 관리 클래스
/// </summary>
public class SceneLoadManager : MonoSingleton<SceneLoadManager>
{
    private bool isInitialized = false;
    
    private Dictionary<SceneType, BaseScene> scenes;

    private BaseScene currentScene;
    private bool isLoading = false;

    /// <summary>
    /// SceneLoadManager 초기화
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
        {
            // SceneLoadManager의 초기화는 한 번만 필요
            return;
        }
        
        // 초기화 플래그 설정
        isInitialized = true;
        
        InitializeSceneList();
        InitializeCurrentActiveScene().Forget();
    }

    /// <summary>
    /// 비동기 Scene 로딩
    /// </summary>
    /// <param name="sceneType">로딩할 Scene</param>
    public async UniTask LoadSceneAsync(SceneType sceneType)
    {
        if (isLoading)
        {
            CDebug.LogWarning("[SceneLoadManager] 이미 씬 로딩 중입니다.");
            return;
        }

        if (!scenes.TryGetValue(sceneType, out var scene))
        {
            CDebug.LogError($"[SceneLoadManager] {sceneType} 씬이 등록되지 않았습니다.");
            return;
        }

        // 중복 로딩 방지 플래그
        isLoading = true;

        try
        {
            // 1. 현재 씬 정리
            if (currentScene != null)
            {
                await currentScene.CleanupAsync();
            }

            // 2. 로딩 UI 표시
            //TODO: LoadingUI 활성화, 필요 시, Fade 연출 추가
            // var loadingUI = await UIManager.Instance.OpenAsync<LoadingUI>();
            // await FadeOut();

            // 3. 씬 로드
            var operation = SceneManager.LoadSceneAsync(sceneType.ToString());
            if (operation == null)
            {
                CDebug.LogError($"[SceneLoadManager] {sceneType} 씬 로드 실패");
                return;
            }

            operation.allowSceneActivation = false;
            while (operation.progress < 0.9f)
            {
                CDebug.Log($"[SceneLoadManager] 로딩 진행률: {operation.progress * 100:N0}%");
                await UniTask.Yield();
            }

            // 4. 씬 활성화
            operation.allowSceneActivation = true;
            await operation.ToUniTask();
            
            // 5. 새 씬의 초기화
            currentScene = scene;
            await currentScene.InitializeAsync();

            // 6. LoadingUI 닫기
            // await FadeIn();
            // UIManager.Instance.Close<LoadingUI>(loadingUI);
        }
        catch (Exception e)
        {
            CDebug.LogError($"[SceneLoadManager] 씬 로드 중 에러 발생: {e}");
        }
        finally
        {
            // 7. 씬 준비 완료
            isLoading = false;
        }
    }

    /// <summary>
    /// 각 Scene 클래스 초기화
    /// </summary>
    private void InitializeSceneList()
    {
        scenes = new Dictionary<SceneType, BaseScene>()
        {
            { SceneType.TitleScene, new TitleScene() },
            { SceneType.LobbyScene, new LobbyScene() },
            { SceneType.GameScene, new GameScene() },
            { SceneType.SampleScene, new SampleScene() }
        };
    }

    /// <summary>
    /// 게임 시작 시, 첫 Scene 초기화
    /// </summary>
    private async UniTaskVoid InitializeCurrentActiveScene()
    {
        var sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (scenes.TryGetValue((SceneType)sceneIndex, out var scene))
        {
            currentScene = scene;
            await currentScene.InitializeAsync();
            CDebug.Log($"[SceneLoadManager] 시작 씬: {currentScene.SceneType}");
        }
    }
}