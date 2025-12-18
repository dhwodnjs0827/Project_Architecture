using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IResourceHandler
{
    public UniTask<T> LoadAsync<T>(string path) where T : Object;
    
    public UniTask<T[]> LoadAllAsync<T>(string path) where T : Object;

    public void Release(Object obj);
}