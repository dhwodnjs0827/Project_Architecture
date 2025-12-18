using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

/// <summary>
/// 리소스를 관리하는 매니저 클래스
/// <para>Strategy Pattern 사용으로 Resources, Addressable 대응 가능</para>
/// <para>ADDRESSABLE 심볼 사용</para>
/// </summary>
public class ResourceManager : MonoSingleton<ResourceManager>, IResourceHandler
{
    private IResourceHandler handler;
    private readonly IDictionary<string, Object> resourceCache = new Dictionary<string, Object>();  // 리소스 캐시

    protected override void Awake()
    {
        base.Awake();
        InitResourceHandler();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ReleaseAll();
    }

    /// <summary>
    /// 단일 리소스 로드
    /// </summary>
    /// <param name="path">리소스 경로</param>
    /// <typeparam name="T">UnityEngine.Object</typeparam>
    public async UniTask<T> LoadAsync<T>(string path) where T : Object
    {
        // 캐싱된 리소스 검사
        if (resourceCache.TryGetValue(path, out var resource))
        {
            return resource as T;
        }

        resource = await handler.LoadAsync<T>(path);
        if (resource != null)
        {
            resourceCache.Add(path, resource);
        }
        else
        {
            CDebug.LogError($"[ResourceManager] {path}에 리소스가 없습니다.");
        }

        return (T)resource;
    }

    /// <summary>
    /// 디렉토리(라벨) 리소스들 로드
    /// </summary>
    /// <param name="path">디렉토리(라벨) 경로</param>
    /// <typeparam name="T">UnityEngine.Object</typeparam>
    public async UniTask<T[]> LoadAllAsync<T>(string path) where T : Object
    {
        var resources = await handler.LoadAllAsync<T>(path);
        if (resources == null)
        {
            CDebug.LogError($"[ResourceManager] {path}에 리소스가 없습니다.");
        }

        return resources;
    }

    /// <summary>
    /// 리소스 해제
    /// </summary>
    public void Release(Object obj)
    {
        // 캐시에서 제거
        var key = resourceCache.FirstOrDefault(x => x.Value == obj).Key;
        if (key != null)
        {
            resourceCache.Remove(key);
        }

        handler.Release(obj);
    }

    /// <summary>
    /// 리소스 핸들러 객체 생성
    /// </summary>
    private void InitResourceHandler()
    {
#if ADDRESSABLE
        handler = new AddressableHandler();
#else
        handler = new ResourcesHandler();
#endif
    }
    
    /// <summary>
    /// 모든 리소스 메모리 해제 및 캐시 정리
    /// </summary>
    private void ReleaseAll()
    {
        foreach (var resource in resourceCache.Values)
        {
            handler.Release(resource);
        }
        resourceCache.Clear();
    }
}