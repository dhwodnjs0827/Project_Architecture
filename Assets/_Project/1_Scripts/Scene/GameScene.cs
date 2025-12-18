using Cysharp.Threading.Tasks;

public class GameScene : BaseScene
{
    public override SceneType SceneType => SceneType.Game;
    
    public override async UniTask InitializeAsync()
    {
        await UniTask.CompletedTask;
    }

    public override UniTask CleanupAsync()
    {
        return default;
    }
}