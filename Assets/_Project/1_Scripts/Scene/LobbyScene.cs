using Cysharp.Threading.Tasks;

public class LobbyScene : BaseScene
{
    public override SceneType SceneType => SceneType.Lobby;
    
    public override async UniTask InitializeAsync()
    {
        await UniTask.CompletedTask;
    }

    public override UniTask CleanupAsync()
    {
        return default;
    }
}