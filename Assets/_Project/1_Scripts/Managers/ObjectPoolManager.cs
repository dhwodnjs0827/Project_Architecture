using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// ObjectPool을 관리하는 매니저 클래스
/// </summary>
public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
{
    private Dictionary<GameObject, IObjectPool<GameObject>> pools = new(); // 프리팹별 pool 관리
    private Dictionary<GameObject, GameObject> prefabLookup = new(); // 인스턴스 - 프리팹 매핑 (Release 시, 원본 프리팹 찾는 용도)
    private Dictionary<GameObject, Transform> poolParents = new(); // pool 부모

    /// <summary>
    /// 풀에서 오브젝트 가져오기
    /// </summary>
    /// <param name="prefab">생성할 오브젝트의 프리팹 원본</param>
    public T Get<T>(T prefab) where T : Component
    {
        var obj = Get(prefab.gameObject);
        return obj.GetComponent<T>();
    }

    /// <summary>
    /// 풀에서 오브젝트 가져오기
    /// </summary>
    /// <param name="prefab">생성할 오브젝트의 프리팹 원본</param>
    public GameObject Get(GameObject prefab)
    {
        var pool = GetOrCreatePool(prefab);
        return pool.Get();
    }

    /// <summary>
    /// 오브젝트를 풀에 반환
    /// </summary>
    /// <param name="component">반환할 오브젝트 컴포넌트</param>
    public void Release<T>(T component) where T : Component
    {
        Release(component.gameObject);
    }

    /// <summary>
    /// 오브젝트를 풀에 반환
    /// </summary>
    /// <param name="obj">반환할 오브젝트</param>
    public void Release(GameObject obj)
    {
        if (!prefabLookup.TryGetValue(obj, out var prefab))
        {
            CDebug.LogWarning("[ObjectPoolManager] 풀에 등록되지 않은 오브젝트입니다.");
            Destroy(obj);
            return;
        }

        pools[prefab].Release(obj);
    }

    /// <summary>
    /// 오브젝트 미리 생성
    /// </summary>
    /// <param name="prefab">생성할 오브젝트의 프리팹 원본</param>
    /// <param name="count">초기 생성 개수</param>
    /// <param name="maxSize">Pool 최대 크기</param>
    public void Preload<T>(T prefab, int count = 10, int maxSize = 100) where T : Component
    {
        Preload(prefab.gameObject, count, maxSize);
    }

    /// <summary>
    /// 오브젝트 미리 생성
    /// </summary>
    /// <param name="prefab">생성할 오브젝트의 프리팹 원본</param>
    /// <param name="count">초기 생성 개수</param>
    /// <param name="maxSize">Pool 최대 크기</param>
    public void Preload(GameObject prefab, int count = 10, int maxSize = 100)
    {
        var pool = GetOrCreatePool(prefab, count, Math.Max(count, maxSize));
        var preloaded = new List<GameObject>(count);

        for (int i = 0; i < count; i++)
        {
            preloaded.Add(pool.Get());
        }

        foreach (var obj in preloaded)
        {
            pool.Release(obj);
        }
    }

    /// <summary>
    /// 특정 프리팹의 풀 정리
    /// </summary>
    /// <param name="prefab">정리할 풀의 프리팹</param>
    public void Clear(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var pool))
        {
            pool.Clear();
            pools.Remove(prefab);
        }

        if (poolParents.TryGetValue(prefab, out var parent))
        {
            Destroy(parent.gameObject);
            poolParents.Remove(prefab);
        }
    }

    /// <summary>
    /// 모든 풀 정리
    /// </summary>
    public void ClearAll()
    {
        foreach (var pool in pools.Values)
        {
            pool.Clear();
        }

        pools.Clear();

        foreach (var parent in poolParents.Values)
        {
            Destroy(parent.gameObject);
        }

        poolParents.Clear();
        prefabLookup.Clear();
    }

    /// <summary>
    /// pool 반환(없으면 새로 생성 후, 반환)
    /// </summary>
    private IObjectPool<GameObject> GetOrCreatePool(GameObject prefab, int defaultCapacity = 10, int maxSize = 100)
    {
        if (!pools.TryGetValue(prefab, out var pool))
        {
            pool = CreatePool(prefab, defaultCapacity, maxSize);
            pools[prefab] = pool;
        }

        return pool;
    }

    /// <summary>
    /// pool 생성
    /// </summary>
    private IObjectPool<GameObject> CreatePool(GameObject prefab, int defaultCapacity = 10, int maxSize = 100)
    {
        // 풀 오브젝트 부모 생성
        var parentGo = new GameObject($"[Pool] {prefab.name}");
        parentGo.transform.SetParent(transform);
        poolParents[prefab] = parentGo.transform;

        return new ObjectPool<GameObject>(
            createFunc: () => CreatePooledObject(prefab),
            actionOnGet: OnGetFromPool,
            actionOnRelease: obj => OnReleaseToPool(obj, prefab),
            actionOnDestroy: OnDestroyPooledObject,
            collectionCheck: true, // 중복 반환 체크
            defaultCapacity: defaultCapacity, // 초기 수용량
            maxSize: maxSize // 최대 pool 크기
        );
    }

    /// <summary>
    /// Pool 오브젝트 생성
    /// </summary>
    private GameObject CreatePooledObject(GameObject prefab)
    {
        var obj = Instantiate(prefab, poolParents[prefab]);
        prefabLookup[obj] = prefab;
        return obj;
    }

    /// <summary>
    /// Pool에서 가져올 때 콜백
    /// </summary>
    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);

        // IPoolable 콜백 호출
        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnGet();
        }
    }

    /// <summary>
    /// Pool에 반환할 때 콜백
    /// </summary>
    private void OnReleaseToPool(GameObject obj, GameObject prefab)
    {
        // IPoolable 콜백 호출
        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnRelease();
        }

        obj.SetActive(false);
        obj.transform.SetParent(poolParents[prefab]);
    }

    /// <summary>
    /// pool에 반환 시, pool이 가득 찼을 때 콜백
    /// </summary>
    private void OnDestroyPooledObject(GameObject obj)
    {
        prefabLookup.Remove(obj);
        Destroy(obj);
    }
}