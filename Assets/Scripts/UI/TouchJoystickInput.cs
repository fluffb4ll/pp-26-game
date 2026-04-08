using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TouchJoystickInput : MonoBehaviour
{
    [SerializeField] private string canvasName = "TouchCanvas";
    [SerializeField] private string eventSystemName = "EventSystem";
    [SerializeField] private string joystickName = "MoveJoystick";
    [SerializeField] private Vector2 joystickSize = new(220f, 220f);
    [SerializeField] private float handleSize = 96f;
    [SerializeField] private Vector2 screenOffset = new(120f, 120f);
    [SerializeField] private float backgroundOpacity = 0.18f;
    [SerializeField] private float handleOpacity = 0.45f;
    [SerializeField] private bool showInEditor = true;

    private Canvas canvas;
    private RectTransform joystickRoot;
    private RectTransform handle;
    private Sprite circleSprite;
    private Gamepad virtualGamepad;
    private Vector2 currentInput;
    private Vector2 lastQueuedInput = new(float.NaN, float.NaN);
    private bool wasVisible;

    private void Awake()
    {
        EnsureVirtualGamepad();
        EnsureEventSystem();
        BuildUi();
        ApplyVisibility(true);
        QueueStick(Vector2.zero);
    }

    private void Update()
    {
        ApplyVisibility(false);
        QueueStick(currentInput);
    }

    private void OnDisable()
    {
        SetInput(Vector2.zero);
        QueueStick(Vector2.zero);
    }

    private void OnDestroy()
    {
        SetInput(Vector2.zero);
        QueueStick(Vector2.zero);

        if (virtualGamepad != null && virtualGamepad.added)
        {
            InputSystem.RemoveDevice(virtualGamepad);
        }

        if (circleSprite != null)
        {
            Destroy(circleSprite.texture);
            Destroy(circleSprite);
        }
    }

    public void BeginDrag(int pointerId, Vector2 screenPosition)
    {
        if (!wasVisible)
        {
            return;
        }

        UpdateInput(screenPosition);
    }

    public void Drag(int pointerId, Vector2 screenPosition)
    {
        if (!wasVisible)
        {
            return;
        }

        UpdateInput(screenPosition);
    }

    public void EndDrag(int pointerId)
    {
        SetInput(Vector2.zero);
    }

    private void EnsureVirtualGamepad()
    {
        if (virtualGamepad != null && virtualGamepad.added)
        {
            return;
        }

        virtualGamepad = InputSystem.AddDevice<Gamepad>("Virtual Touch Gamepad");
    }

    private void EnsureEventSystem()
    {
        EventSystem sceneEventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (sceneEventSystem == null)
        {
            GameObject eventSystemObject = new(eventSystemName, typeof(EventSystem), typeof(InputSystemUIInputModule));
            sceneEventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = sceneEventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = sceneEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        foreach (BaseInputModule module in sceneEventSystem.GetComponents<BaseInputModule>())
        {
            if (module != inputModule)
            {
                module.enabled = false;
            }
        }
    }

    private void BuildUi()
    {
        canvas = GameObject.Find(canvasName)?.GetComponent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new(canvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        Transform existingRoot = canvas.transform.Find(joystickName);
        if (existingRoot != null)
        {
            joystickRoot = existingRoot as RectTransform;
        }
        else
        {
            joystickRoot = CreateJoystickRoot(canvas.transform);
        }

        TouchJoystickArea touchArea = joystickRoot.GetComponent<TouchJoystickArea>();
        if (touchArea == null)
        {
            touchArea = joystickRoot.gameObject.AddComponent<TouchJoystickArea>();
        }

        touchArea.Initialize(this);

        if (handle == null)
        {
            Transform existingHandle = joystickRoot.Find("Handle");
            handle = existingHandle as RectTransform ?? CreateHandle(joystickRoot);
        }
    }

    private RectTransform CreateJoystickRoot(Transform parent)
    {
        GameObject root = new(joystickName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TouchJoystickArea));
        RectTransform rectTransform = root.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = joystickSize;
        rectTransform.anchoredPosition = screenOffset;

        Image image = root.GetComponent<Image>();
        image.sprite = GetCircleSprite();
        image.type = Image.Type.Simple;
        image.color = new Color(0f, 0f, 0f, backgroundOpacity);

        return rectTransform;
    }

    private RectTransform CreateHandle(RectTransform parent)
    {
        GameObject handleObject = new("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rectTransform = handleObject.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(handleSize, handleSize);
        rectTransform.anchoredPosition = Vector2.zero;

        Image image = handleObject.GetComponent<Image>();
        image.sprite = GetCircleSprite();
        image.type = Image.Type.Simple;
        image.raycastTarget = false;
        image.color = new Color(1f, 1f, 1f, handleOpacity);

        return rectTransform;
    }

    private void ApplyVisibility(bool force)
    {
        bool shouldShow = ShouldShowJoystick();
        if (!force && shouldShow == wasVisible)
        {
            return;
        }

        wasVisible = shouldShow;

        if (joystickRoot != null)
        {
            joystickRoot.gameObject.SetActive(shouldShow);
        }

        if (!shouldShow)
        {
            SetInput(Vector2.zero);
        }
    }

    private bool ShouldShowJoystick()
    {
        return Application.isMobilePlatform || Touchscreen.current != null || (showInEditor && Application.isEditor);
    }

    private void UpdateInput(Vector2 screenPosition)
    {
        if (joystickRoot == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickRoot,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint);

        Vector2 radius = joystickRoot.sizeDelta * 0.5f;
        Vector2 clampedPoint = new(
            Mathf.Clamp(localPoint.x, -radius.x, radius.x),
            Mathf.Clamp(localPoint.y, -radius.y, radius.y));

        Vector2 normalizedInput = new(
            radius.x > 0f ? clampedPoint.x / radius.x : 0f,
            radius.y > 0f ? clampedPoint.y / radius.y : 0f);

        if (normalizedInput.sqrMagnitude > 1f)
        {
            normalizedInput.Normalize();
        }

        SetInput(normalizedInput);
    }

    private void SetInput(Vector2 input)
    {
        currentInput = input;

        if (handle == null || joystickRoot == null)
        {
            return;
        }

        Vector2 radius = joystickRoot.sizeDelta * 0.5f;
        handle.anchoredPosition = new Vector2(input.x * radius.x, input.y * radius.y);
    }

    private void QueueStick(Vector2 stickValue)
    {
        if (virtualGamepad == null || !virtualGamepad.added)
        {
            return;
        }

        if (lastQueuedInput == stickValue)
        {
            return;
        }

        InputSystem.QueueStateEvent(virtualGamepad, new GamepadState { leftStick = stickValue });
        lastQueuedInput = stickValue;
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
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= radius ? Color.white : Color.clear;
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
}

internal sealed class TouchJoystickArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private TouchJoystickInput owner;
    private int activePointerId = int.MinValue;

    public void Initialize(TouchJoystickInput touchJoystickInput)
    {
        owner = touchJoystickInput;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        activePointerId = eventData.pointerId;
        owner?.BeginDrag(activePointerId, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
        {
            return;
        }

        owner?.Drag(activePointerId, eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
        {
            return;
        }

        owner?.EndDrag(activePointerId);
        activePointerId = int.MinValue;
    }
}
