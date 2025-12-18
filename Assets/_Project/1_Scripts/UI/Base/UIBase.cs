using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    [Header("UI Config")]
    [SerializeField] private UIType uiType = UIType.UI; // UI 종류
    [SerializeField] private bool isActiveOnLoad = true; // UI가 로드될 때 자동으로 활성화할지 여부
    [SerializeField] private bool isDestroyOnClose = false; // UI가 닫힐 때 파괴할지 여부
    
    public UIType UIType => uiType;
    public bool IsActiveOnLoad => isActiveOnLoad;
    public bool IsDestroyOnClose => isDestroyOnClose;

    public void Open(params object[] args)
    {
        gameObject.SetActive(true);
        Opened(args);
    }

    public void Close(params object[] args)
    {
        Closed(args);
        gameObject.SetActive(false);
    }

    protected abstract void Opened(params object[] args);

    protected abstract void Closed(params object[] args);
}
