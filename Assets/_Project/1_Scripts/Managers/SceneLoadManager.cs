using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoSingleton<SceneLoadManager>
{
    private Dictionary<SceneType, BaseScene> scenes;

    private BaseScene currentScene;
    private bool isLoading = false;

    protected override void Awake()
    {
        base.Awake();

        InitializeSceneList();
        InitializeCurrentActiveScene();
    }

    public async UniTask LoadSceneAsync(BaseScene scene)
    {
        if (isLoading)
        {
            CDebug.LogWarning("[SceneLoadManager] 이미 씬 로딩 중입니다.]");
            return;
        }

        isLoading = true;

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
        var operation = SceneManager.LoadSceneAsync((int)scene.SceneType);
        if (operation != null)
        {
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                CDebug.Log($"[SceneLoadManager] 로딩 진행률: {operation.progress * 100 :N0}%");
                await UniTask.Yield();
            }
            
            // 4. 씬 활성화
            operation.allowSceneActivation = true;
            await operation.ToUniTask();

            
            // 5. 새 씬의 초기화
            await scene.InitializeAsync();
            
            // 6. LoadingUI 닫기
            // await FadeIn();
            // UIManager.Instance.Close<LoadingUI>(loadingUI);
            
            // 7. 씬 준비 완료
            isLoading = false;
        }
    }

    private void InitializeSceneList()
    {
        scenes = new Dictionary<SceneType, BaseScene>()
        {
            { SceneType.Title, new TitleScene() },
            { SceneType.Lobby, new LobbyScene() },
            { SceneType.Game, new GameScene() },
            { SceneType.Sample , new SampleScene() }
        };
    }

    private void InitializeCurrentActiveScene()
    {
        var sceneIndex = SceneManager.GetActiveScene().buildIndex;
        currentScene = scenes[(SceneType)sceneIndex];
        currentScene.InitializeAsync();
    }
}