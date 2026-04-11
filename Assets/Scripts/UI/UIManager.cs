using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// следит за ui сцены и держит touch в safe area
/// </summary>
[DisallowMultipleComponent]
public sealed class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private RectTransform safeAreaRoot;
    [SerializeField] private GameObject touchControlsRoot;
    [SerializeField] private bool showTouchControlsInEditor = true;

    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

    /// <summary>
    /// регистрируем менеджер и сразу раскладываем ui
    /// </summary>
    private void Awake()
    {
        if (!ReferenceEquals(Instance, null) && !ReferenceEquals(Instance, this))
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RefreshLayout(true);
    }

    /// <summary>
    /// чистим ссылку когда ui удаляется
    /// </summary>
    private void OnDestroy()
    {
        if (ReferenceEquals(Instance, this))
            Instance = null;
    }

    /// <summary>
    /// проверяем размер экрана и safe area
    /// </summary>
    private void Update()
    {
        RefreshLayout(false);
    }

    /// <summary>
    /// применяем safe area и видимость touch
    /// </summary>
    private void RefreshLayout(bool force)
    {
        Vector2Int screenSize = new(Screen.width, Screen.height);
        Rect safeArea = Screen.safeArea;

        if (!force && screenSize == _lastScreenSize && safeArea == _lastSafeArea)
            return;

        _lastScreenSize = screenSize;
        _lastSafeArea = safeArea;

        ApplySafeArea(safeArea);
        touchControlsRoot.SetActive(ShouldShowTouchControls());
    }

    /// <summary>
    /// переводим safe area из пикселей в anchors
    /// </summary>
    private void ApplySafeArea(Rect safeArea)
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        safeAreaRoot.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        safeAreaRoot.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// показываем стики только там где нужен touch
    /// </summary>
    private bool ShouldShowTouchControls()
    {
        return Application.isMobilePlatform || Touchscreen.current is not null || (showTouchControlsInEditor && Application.isEditor);
    }
}
