using Cysharp.Threading.Tasks;

public class SampleScene : BaseScene
{
    public override SceneType SceneType => SceneType.Sample;
    
    public override async UniTask InitializeAsync()
    {
        await UniTask.CompletedTask;
    }

    public override UniTask CleanupAsync()
    {
        return default;
    }
}