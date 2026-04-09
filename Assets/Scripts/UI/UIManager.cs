using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Vector2 referenceResolution = new(1920f, 1080f);
    [SerializeField] [Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;
    [SerializeField] private bool showTouchControlsInEditor = true;
    [SerializeField] private Vector2 joystickSize = new(240f, 240f);
    [SerializeField] private Vector2 joystickMargin = new(36f, 36f);
    [SerializeField] private float movementRange = 90f;
    [SerializeField] private float knobSize = 96f;
    [SerializeField] private float backgroundOpacity = 0.18f;
    [SerializeField] private float knobOpacity = 0.45f;

    private Canvas canvas;
    private RectTransform safeAreaRoot;
    private RectTransform touchControlsRoot;
    private RectTransform moveJoystickRoot;
    private Rect lastSafeArea = new(-1f, -1f, -1f, -1f);
    private Vector2Int lastScreenSize = new(-1, -1);
    private Sprite circleSprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SanitizeSettings();

        EnsureEventSystem();
        EnsureCanvas();
        EnsureSafeAreaRoot();
        EnsureTouchControls();
        RefreshLayout(true);
    }

    private void Update()
    {
        RefreshLayout(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (circleSprite != null)
        {
            Destroy(circleSprite.texture);
            Destroy(circleSprite);
        }
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        foreach (BaseInputModule module in eventSystem.GetComponents<BaseInputModule>())
        {
            if (module != inputModule)
            {
                module.enabled = false;
            }
        }
    }

    private void EnsureCanvas()
    {
        Transform existingCanvas = transform.Find("HUDCanvas");
        GameObject canvasObject;

        if (existingCanvas != null)
        {
            canvasObject = existingCanvas.gameObject;
        }
        else
        {
            canvasObject = new GameObject("HUDCanvas", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);
        }

        canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = canvasObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = matchWidthOrHeight;

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void EnsureSafeAreaRoot()
    {
        safeAreaRoot = EnsureRectTransform(canvas.transform, "SafeArea");
        safeAreaRoot.anchorMin = Vector2.zero;
        safeAreaRoot.anchorMax = Vector2.one;
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
    }

    private void EnsureTouchControls()
    {
        touchControlsRoot = EnsureRectTransform(safeAreaRoot, "TouchControls");
        touchControlsRoot.anchorMin = Vector2.zero;
        touchControlsRoot.anchorMax = Vector2.one;
        touchControlsRoot.offsetMin = Vector2.zero;
        touchControlsRoot.offsetMax = Vector2.zero;

        moveJoystickRoot = EnsureRectTransform(touchControlsRoot, "MoveJoystick");
        moveJoystickRoot.anchorMin = Vector2.zero;
        moveJoystickRoot.anchorMax = Vector2.zero;
        moveJoystickRoot.pivot = new Vector2(0.5f, 0.5f);
        moveJoystickRoot.sizeDelta = joystickSize;

        RectTransform background = EnsureRectTransform(moveJoystickRoot, "Background");
        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        background.offsetMin = Vector2.zero;
        background.offsetMax = Vector2.zero;

        Image backgroundImage = background.GetComponent<Image>();
        if (backgroundImage == null)
        {
            backgroundImage = background.gameObject.AddComponent<Image>();
        }

        backgroundImage.sprite = GetCircleSprite();
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.color = new Color(0f, 0f, 0f, backgroundOpacity);
        backgroundImage.raycastTarget = false;

        RectTransform stick = EnsureRectTransform(moveJoystickRoot, "Stick");
        stick.anchorMin = new Vector2(0.5f, 0.5f);
        stick.anchorMax = new Vector2(0.5f, 0.5f);
        stick.pivot = new Vector2(0.5f, 0.5f);
        stick.sizeDelta = joystickSize;
        stick.anchoredPosition = Vector2.zero;

        Image stickImage = stick.GetComponent<Image>();
        if (stickImage == null)
        {
            stickImage = stick.gameObject.AddComponent<Image>();
        }

        stickImage.color = new Color(1f, 1f, 1f, 0.001f);
        stickImage.raycastTarget = true;

        OnScreenStick onScreenStick = stick.GetComponent<OnScreenStick>();
        if (onScreenStick == null)
        {
            onScreenStick = stick.gameObject.AddComponent<OnScreenStick>();
        }

        onScreenStick.controlPath = "<Gamepad>/leftStick";
        onScreenStick.movementRange = movementRange;
        onScreenStick.behaviour = OnScreenStick.Behaviour.RelativePositionWithStaticOrigin;
        onScreenStick.useIsolatedInputActions = true;

        RectTransform knob = EnsureRectTransform(stick, "Knob");
        knob.anchorMin = new Vector2(0.5f, 0.5f);
        knob.anchorMax = new Vector2(0.5f, 0.5f);
        knob.pivot = new Vector2(0.5f, 0.5f);
        knob.sizeDelta = new Vector2(knobSize, knobSize);
        knob.anchoredPosition = Vector2.zero;

        Image knobImage = knob.GetComponent<Image>();
        if (knobImage == null)
        {
            knobImage = knob.gameObject.AddComponent<Image>();
        }

        knobImage.sprite = GetCircleSprite();
        knobImage.type = Image.Type.Simple;
        knobImage.color = new Color(1f, 1f, 1f, knobOpacity);
        knobImage.raycastTarget = false;
    }

    private void RefreshLayout(bool force)
    {
        if (canvas == null || safeAreaRoot == null || moveJoystickRoot == null)
        {
            return;
        }

        Vector2Int currentScreenSize = new(Screen.width, Screen.height);
        Rect currentSafeArea = Screen.safeArea;

        if (!force && currentScreenSize == lastScreenSize && currentSafeArea == lastSafeArea)
        {
            return;
        }

        lastScreenSize = currentScreenSize;
        lastSafeArea = currentSafeArea;

        ApplySafeArea(currentSafeArea);
        moveJoystickRoot.anchoredPosition = joystickMargin + joystickSize * 0.5f;
        touchControlsRoot.gameObject.SetActive(ShouldShowTouchControls());
    }

    private void ApplySafeArea(Rect safeArea)
    {
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            safeAreaRoot.anchorMin = Vector2.zero;
            safeAreaRoot.anchorMax = Vector2.one;
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
            return;
        }

        Vector2 anchorMin = new(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        Vector2 anchorMax = new(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);

        safeAreaRoot.anchorMin = anchorMin;
        safeAreaRoot.anchorMax = anchorMax;
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
    }

    private bool ShouldShowTouchControls()
    {
        return Application.isMobilePlatform || Touchscreen.current != null || (showTouchControlsInEditor && Application.isEditor);
    }

    private void SanitizeSettings()
    {
        referenceResolution.x = Mathf.Max(1f, referenceResolution.x);
        referenceResolution.y = Mathf.Max(1f, referenceResolution.y);
        joystickSize.x = Mathf.Max(120f, joystickSize.x);
        joystickSize.y = Mathf.Max(120f, joystickSize.y);
        joystickMargin.x = Mathf.Max(0f, joystickMargin.x);
        joystickMargin.y = Mathf.Max(0f, joystickMargin.y);
        movementRange = Mathf.Clamp(movementRange, 20f, Mathf.Min(joystickSize.x, joystickSize.y));
        knobSize = Mathf.Clamp(knobSize, 32f, Mathf.Min(joystickSize.x, joystickSize.y));
        backgroundOpacity = Mathf.Clamp01(backgroundOpacity);
        knobOpacity = Mathf.Clamp01(knobOpacity);
    }

    private Sprite GetCircleSprite()
    {
        if (circleSprite != null)
        {
            return circleSprite;
        }

        const int size = 128;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distanceToCenter = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distanceToCenter <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        circleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size);
        circleSprite.hideFlags = HideFlags.HideAndDontSave;

        return circleSprite;
    }

    private static RectTransform EnsureRectTransform(Transform parent, string childName)
    {
        Transform existingChild = parent.Find(childName);
        if (existingChild != null)
        {
            return (RectTransform)existingChild;
        }

        GameObject childObject = new(childName, typeof(RectTransform));
        childObject.transform.SetParent(parent, false);
        return childObject.GetComponent<RectTransform>();
    }
}
