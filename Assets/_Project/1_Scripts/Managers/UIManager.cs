using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    private ResourceManager resourceManager;
    private const string UI_RESOURCE_PATH = "UI/";
    
    private Dictionary<UIType, Canvas> canvases;
    private Dictionary<string, UIBase> openedUI = new();
    private Dictionary<string, UIBase> closedUI = new();
    
    private bool isInitialized = false;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Cleanup();
    }

    /// <summary>
    /// UI 열기
    /// </summary>
    public async UniTask<T> OpenAsync<T>(params object[] args) where T: UIBase
    {
        if (!isInitialized)
        {
            CDebug.LogError("[UIManager] UIManager 초기화가 안됐습니다.");
            return null;
        }
        
        // UI가 열려있으면 해당 UI 반환
        UIBase ui = GetUI<T>();
        if (ui != null)
        {
            return (T)ui;
        }
        
        var uiName = typeof(T).Name;

        // 닫힌 UI 풀에 있으면 해당 UI 반환
        if (closedUI.TryGetValue(uiName, out ui))
        {
            closedUI.Remove(uiName);
            openedUI.Add(uiName, ui);
            ui.transform.SetAsLastSibling();
            ui.Open(args);
            return ui as T;
        }
        
        if (resourceManager == null)
        {
            CDebug.LogError("[UIManager] ResourceManager가 null입니다.");
            return null;
        }
        
        // UI Prefab 리소스 불러오기
        var resourcePath = $"{UI_RESOURCE_PATH}{uiName}";
        var prefab = await resourceManager.LoadAsync<T>(resourcePath);

        if (prefab == null)
        {
            CDebug.LogError($"[UIManager] {resourcePath}에 리소스가 없습니다.");
            return null;
        }
        
        // UI 종류에 맞게 부모 캔버스 설정
        var targetCanvas = canvases[prefab.UIType];
        ui = Instantiate(prefab, targetCanvas.transform);

        if (ui.IsActiveOnLoad)
        {
            // 열린 UI 풀에 등록
            openedUI.Add(uiName, ui);
            ui.transform.SetAsLastSibling();
            ui.Open(args);
        }
        else
        {
            closedUI.Add(uiName, ui);
        }
        
        return (T)ui;
    }

    /// <summary>
    /// UI 닫기
    /// </summary>
    public void Close<T>(T uiBase, params object[] args) where T: UIBase
    {
        var uiName = typeof(T).Name;
        if (!openedUI.ContainsKey(uiName))
        {
            CDebug.LogWarning("[UIManager] 존재하지 않는 UI를 닫을려고 했습니다.");
            return;
        }

        openedUI.Remove(uiName);
        uiBase.Close(args);
        
        if (uiBase.IsDestroyOnClose)
        {
            Destroy(uiBase.gameObject);
        }
        else
        {
            closedUI.Add(uiName, uiBase);
        }
    }

    /// <summary>
    /// 열려있는 UI 가져오기
    /// </summary>
    public T GetUI<T>() where T : UIBase
    {
        openedUI.TryGetValue(typeof(T).Name, out var ui);
        return ui as T;
    }

    /// <summary>
    /// UIManager 초기화
    /// </summary>
    private void Initialize()
    {
        resourceManager = ResourceManager.Instance;
        
        InitializeUICanvas().Forget();
    }
    
    /// <summary>
    /// Canvas 초기화
    /// </summary>
    private async UniTaskVoid InitializeUICanvas()
    {
        if (resourceManager == null)
        {
            CDebug.LogError("[UIManager] ResourceManager가 null입니다.");
            return;
        }
        
        var hudPrefab = await resourceManager.LoadAsync<Canvas>("UI/@HUD");
        var uiPrefab = await resourceManager.LoadAsync<Canvas>("UI/@UI");
        var popupPrefab = await resourceManager.LoadAsync<Canvas>("UI/@Popup");
        var tooltipPrefab = await resourceManager.LoadAsync<Canvas>("UI/@Tooltip");
        var loadingPrefab = await resourceManager.LoadAsync<Canvas>("UI/@Loading");
        var systemPrefab = await resourceManager.LoadAsync<Canvas>("UI/@System");

        canvases = new()
        {
            { UIType.HUD, Instantiate(hudPrefab) },
            { UIType.UI, Instantiate(uiPrefab) },
            { UIType.Popup, Instantiate(popupPrefab) },
            { UIType.Tooltip, Instantiate(tooltipPrefab) },
            { UIType.Loading, Instantiate(loadingPrefab) },
            { UIType.System, Instantiate(systemPrefab) }
        };
        
        foreach (var kvp in canvases)
        {
            // Order in Layer 설정
            kvp.Value.sortingOrder = (int)kvp.Key;
            
            // 씬 전환 시, 유지
            DontDestroyOnLoad(kvp.Value.gameObject);
        }
        
        // 초기화 완료
        isInitialized = true;
    }

    /// <summary>
    /// UI 정리
    /// </summary>
    private void Cleanup()
    {
        foreach (var kvp in openedUI)
        {
            Destroy(kvp.Value.gameObject);
        }

        foreach (var kvp in closedUI)
        {
            Destroy(kvp.Value.gameObject);
        }
        
        openedUI.Clear();
        closedUI.Clear();
    }
}
