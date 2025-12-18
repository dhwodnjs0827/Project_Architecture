using Cysharp.Threading.Tasks;

public abstract class BaseScene
{
    public abstract SceneType SceneType { get; }

    public abstract UniTask InitializeAsync();
    
    public abstract UniTask CleanupAsync();
}
