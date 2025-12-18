using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Resources API 사용 (Concrete Strategy)
/// <para>ResourceManager에서 사용</para>
/// </summary>
public class ResourcesHandler : IResourceHandler
{
    public async UniTask<T> LoadAsync<T>(string path) where T : Object
    {
        var request = Resources.LoadAsync<T>(path);
        await request;

        if (request.asset == null)
        {
            return null;
        }

        return request.asset as T;
    }

    public UniTask<T[]> LoadAllAsync<T>(string path) where T : Object
    {
        var resources = Resources.LoadAll<T>(path);
        return UniTask.FromResult(resources);
    }

    public void Release(Object obj)
    {
        if (obj is GameObject or Component)
        {
            // GameObject/Component는 unloadAsset 불가
            return;
        }
        
        Resources.UnloadAsset(obj);
    }
}
