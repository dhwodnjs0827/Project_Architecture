using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

/// <summary>
/// Addressable API 사용 (Concrete Strategy)
/// <para>ResourceManager에서 사용</para>
/// </summary>
public class AddressableHandler : IResourceHandler
{
    public async UniTask<T> LoadAsync<T>(string path) where T : Object
    {
        var resource = await Addressables.LoadAssetAsync<T>(path);
        if (resource == null)
        {
            return null;
        }
        return resource;
    }

    public async UniTask<T[]> LoadAllAsync<T>(string path) where T : Object
    {
        var resources = await Addressables.LoadAssetsAsync<T>(path);
        if (resources == null)
        {
            return null;
        }
        return resources.ToArray();
    }

    public void Release(Object obj)
    {
        Addressables.Release(obj);
    }
}