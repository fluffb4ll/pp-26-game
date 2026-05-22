using System.Collections.Generic;
using Helpers;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Managers
{
    /// <summary>
    /// Управляет UI
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private RectTransform safeAreaRoot;
        [SerializeField] private GameObject touchControlsRoot;
        [SerializeField] private bool showTouchControlsInEditor = true;
        [SerializeField] private List<GameObject> submenus;
        [SerializeField] private GameObject hpBar;
        [SerializeField] private RectTransform hpBarFill;
        [SerializeField] private TextMeshProUGUI coinCount;
        [SerializeField] private bool blockTouchControls;
        [SerializeField] private GameObject interactButton;
        
        private GameManager _gameManager;
        private PlayerController _playerController;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private float _hpBarFillMaxWidth;
        private GameObject _activeSubmenu;

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
            
            _gameManager = GameManager.Instance;
            _playerController = PlayerController.Instance;
            
            RefreshLayout(true);
        }
        

        private void Start()
        {
            _hpBarFillMaxWidth = hpBarFill.rect.width;
            SetSubmenusDefaultState();
            UpdateCoinCount();
            ToggleInteractButton(false);
        }

        private void OnEnable()
        {
            _gameManager.OnGameStateStart += ToggleHpBar;
            _gameManager.OnGameStateEnd += ToggleHpBar;
            _gameManager.OnCoinsChanged += UpdateCoinCount;
            _playerController.OnTakeDamage += UpdateHpBarFill;
            _playerController.OnHeal += UpdateHpBarFill;
        }

        private void OnDisable()
        {
            _gameManager.OnGameStateStart -= ToggleHpBar;
            _gameManager.OnGameStateEnd -= ToggleHpBar;
            _gameManager.OnCoinsChanged -= UpdateCoinCount;
            _playerController.OnTakeDamage -= UpdateHpBarFill;
            _playerController.OnHeal -= UpdateHpBarFill;
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
        /// применяем safe area и видимость touch стиков
        /// </summary>
        private void RefreshLayout(bool force)
        {
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            var safeArea = Screen.safeArea;

            if (!force && screenSize == _lastScreenSize && safeArea == _lastSafeArea)
                return;

            _lastScreenSize = screenSize;
            _lastSafeArea = safeArea;

            ApplySafeArea(safeArea);
            touchControlsRoot.SetActive(ShouldShowTouchControls());
        }

        /// <summary>
        /// переводит safe area из пикселей в anchors
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
        /// показывает стики только там где нужен touch
        /// </summary>
        private bool ShouldShowTouchControls()
        {
            return !blockTouchControls && (Application.isMobilePlatform || !ReferenceEquals(Touchscreen.current, null) || (showTouchControlsInEditor && Application.isEditor));
        }
        
        /// <summary>
        /// Отключает видимость всех подменю
        /// </summary>
        private void SetSubmenusDefaultState()
        {
            foreach (var submenu in submenus)
                submenu.SetActive(false);
        }

        /// <summary>
        /// Открывает/закрывает панель в зависимости от её текущего состояния
        /// </summary>
        /// <param name="panel">Требуемая панель</param>
        public void TogglePanel(GameObject panel)
        {
            panel.SetActive(!panel.activeSelf);
            
            if (!panel.activeSelf)
            {
                _activeSubmenu = null;
                return;
            }
            
            _activeSubmenu?.SetActive(false);
            _activeSubmenu = panel;
        }
        
        /// <summary>
        /// Показывает или скрывает хп при входе или выходе из боя соответственно
        /// </summary>
        /// <param name="gameState">Следующий/предыдущий <see cref="GameState"/></param>
        private void ToggleHpBar(GameState gameState)
        {
            if (gameState == GameState.Combat)
                hpBar.SetActive(!hpBar.activeSelf);
        }
        
        /// <summary>
        /// Обновляет отображаемое хп в зависимости от нового значения здоровья игрока
        /// </summary>
        /// <param name="hpPercent">Новый процент здоровья игрока</param>
        private void UpdateHpBarFill(float hpPercent)
        {
            hpBarFill.offsetMax -= new Vector2(_hpBarFillMaxWidth * (1.0f - hpPercent) + hpBarFill.sizeDelta.x, 0);
        }
        
        /// <summary>
        /// Обновляет отображаемое число монет, имеющихся у игрока
        /// </summary>
        private void UpdateCoinCount()
        {
            var amount = _gameManager.GetCoinsAmount();
            var data = ResourceCountHelper.CountShortener(amount);
            coinCount.SetText(data.formatTemplate, data.value);
        }

        /// <summary>
        /// Изменяет 
        /// </summary>
        /// <param name="state"></param>
        public void ToggleInteractButton(bool state)
        {
            interactButton.SetActive(state);
        }
    }
}
