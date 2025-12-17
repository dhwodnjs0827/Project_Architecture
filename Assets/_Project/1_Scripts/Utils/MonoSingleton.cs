using UnityEngine;

/// <summary>
/// MonoBehaviour 기반 싱글톤 베이스 클래스
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T: Component
{
    private static T instance;
    private static bool isApplicationQuitting = false;
    
    /// <summary>
    /// DontDestroyOnLoad 설정 (기본값: true)
    /// </summary>
    protected virtual bool IsDontDestroyOnLoad => true;

    public static T Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                return null;
            }
            
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();

                if (instance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    instance = go.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        RemoveDuplicates();
    }

    protected virtual void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }
    
    protected virtual void OnDestroy()
    {
        instance = null;
    }

    /// <summary>
    /// 중복 인스턴스 제거 및 DontDestroyOnLoad 설정
    /// </summary>
    private void RemoveDuplicates()
    {
        if (instance == null)
        {
            instance = this as T;
            if (IsDontDestroyOnLoad)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(this);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
