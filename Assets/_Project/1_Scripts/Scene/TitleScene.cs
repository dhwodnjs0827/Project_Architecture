using Cysharp.Threading.Tasks;

public class TitleScene : BaseScene
{
    public override SceneType SceneType => SceneType.Title;
    
    public override async UniTask InitializeAsync()
    {
        await UniTask.CompletedTask;
    }

    public override UniTask CleanupAsync()
    {
        return default;
    }
}