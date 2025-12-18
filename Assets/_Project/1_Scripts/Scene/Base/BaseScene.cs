using Cysharp.Threading.Tasks;

public abstract class BaseScene
{
    public abstract SceneType SceneType { get; }

    public virtual UniTask InitializeAsync() => UniTask.CompletedTask;
    
    public virtual UniTask CleanupAsync() => UniTask.CompletedTask;
}
