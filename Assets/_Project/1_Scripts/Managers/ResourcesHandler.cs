using Cysharp.Threading.Tasks;
using UnityEngine;

public class ResourcesHandler : IResourceHandler
{
    public async UniTask<T> LoadAsync<T>(string path) where T : Object
    {
        var resource = await Resources.LoadAsync<T>(path);
        if (resource == null)
        {
            return null;
        }
        return resource as T;
    }

    public async UniTask<T[]> LoadAllAsync<T>(string path) where T : Object
    {
        var resources = Resources.LoadAll<T>(path);
        if (resources == null)
        {
            return null;
        }
        await UniTask.CompletedTask;
        return resources;
    }

    public void Release(Object obj)
    {
        if (obj is GameObject)
        {
            // GameObject는 unloadAsset 불가
            return;
        }
        Resources.UnloadAsset(obj);
    }
}
