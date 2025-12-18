using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    private ResourceManager resourceManager;
    private Dictionary<UIType, Canvas> canvases;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
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
            CDebug.LogError("[UIManager]: ResourceManager가 null입니다.]");
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
    }
}
