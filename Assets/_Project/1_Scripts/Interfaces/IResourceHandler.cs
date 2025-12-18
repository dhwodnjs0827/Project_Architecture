using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 리소스 관리 인터페이스 (Strategy Interface)
/// </summary>
public interface IResourceHandler
{
    public UniTask<T> LoadAsync<T>(string path) where T : Object;
    
    public UniTask<T[]> LoadAllAsync<T>(string path) where T : Object;

    public void Release(Object obj);
}