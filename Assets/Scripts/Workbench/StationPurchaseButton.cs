using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using YG;

public class StationPurchaseButton : MonoBehaviour
{
    [Header("Стоимость")]
    [SerializeField] private int startPrice = 50;
    [SerializeField] private float priceMultiplier = 1.53f;
    private int currentPrice;

    [Header("Префаб станка")]
    [SerializeField] private GameObject stationPrefab;

    [Header("Позиции спавна кнопки и станка")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] buttonPositions;

    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject promptPanel;

    [Header("Input System Action")]
    [SerializeField] private InputActionReference purchaseAction;

    private int currentIndex = 0;
    private bool playerInRange = false;

    private void Awake()
    {
        if (purchaseAction == null)
        {
            Debug.LogError("❌ Purchase Input Action не назначен в StationPurchaseButton");
            return;
        }
        purchaseAction.action.Enable();
        Debug.Log($"✅ Purchase action enabled: {purchaseAction.action.name}");

        if (spawnPoints == null || spawnPoints.Length == 0)
            Debug.LogError("❌ Spawn Points не назначены");
        if (buttonPositions == null || buttonPositions.Length == 0)
            Debug.LogError("❌ Button Positions не назначены");
        if (stationPrefab == null)
            Debug.LogError("❌ Station Prefab не назначен");
        if (priceText == null)
            Debug.LogError("❌ Price Text не назначен");
    }

    private void Start()
    {
        InitializePosition();
        currentPrice = startPrice;
        UpdatePriceDisplay();
        HidePrompt();
    }

    private void OnEnable()
    {
        if (purchaseAction != null)
            purchaseAction.action.performed += OnPurchasePerformed;
    }

    private void OnDisable()
    {
        if (purchaseAction != null)
        {
            purchaseAction.action.performed -= OnPurchasePerformed;
            purchaseAction.action.Disable();
        }
    }

    public void TryPurchase()
    {
        Debug.Log($"TryPurchase вызван. playerInRange={playerInRange}, currentPrice={currentPrice}, currentIndex={currentIndex}");

        if (!playerInRange)
        {
            Debug.Log("Игрок не в зоне действия кнопки");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Невозможно купить: spawnPoints не назначен");
            return;
        }

        if (currentIndex >= spawnPoints.Length)
        {
            Debug.Log("Все станции уже куплены. Кнопка самоуничтожается.");
            Destroy(gameObject);
            return;
        }

        int playerCoins = GetPlayerCoins();
        Debug.Log($"У игрока монет: {playerCoins}, цена: {currentPrice}");

        if (playerCoins < currentPrice)
        {
            Debug.Log("Не хватает монет!");
            ShowNotification("Не хватает монет.");
            return;
        }

        SetPlayerCoins(playerCoins - currentPrice);
        Debug.Log($"Списано {currentPrice} монет. Новый баланс: {playerCoins - currentPrice}");

        if (stationPrefab == null)
        {
            Debug.LogError("Station Prefab не назначен, станок не создан");
            return;
        }
        InstantiateStation();
        MoveButtonToNextPosition();
        IncreasePrice();
    }

    private void InitializePosition()
    {
        if (buttonPositions != null && buttonPositions.Length > 0)
            transform.position = buttonPositions[0].position;
        else
            Debug.LogError("Нет позиций для кнопки покупки станции");
    }

    private void UpdatePriceDisplay()
    {
        if (priceText != null)
            priceText.text = $"Цена: {currentPrice}";
    }

    private void HidePrompt()
    {
        if (promptPanel != null)
            promptPanel.SetActive(false);
    }

    private void ShowPrompt(bool show)
    {
        if (promptPanel != null)
            promptPanel.SetActive(show);
    }

    private void OnPurchasePerformed(InputAction.CallbackContext context)
    {
        if (this == null) return;
        Debug.Log("🎯 OnPurchasePerformed - клавиша E нажата");
        TryPurchase();
    }

    private int GetPlayerCoins()
    {
        return YG2.saves.coins;
    }

    private void SetPlayerCoins(int amount)
    {
        YG2.saves.coins = amount;
        //Принудительная установка
        Debug.Log($"SetPlayerCoins: установлено {amount} монет");
    }

    private void ShowNotification(string message)
    {
        Debug.Log("Уведомление: " + message);
    }

    private void InstantiateStation()
    {
        Transform spawnPoint = spawnPoints[currentIndex];
        Instantiate(stationPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"Станок создан на позиции {currentIndex}");
    }

    private void MoveButtonToNextPosition()
    {
        currentIndex++;
        if (currentIndex < buttonPositions.Length)
        {
            transform.position = buttonPositions[currentIndex].position;
            Debug.Log($"Кнопка перемещена на позицию {currentIndex}");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Кнопка уничтожена (все станки куплены)");
        }
    }

    private void IncreasePrice()
    {
        currentPrice = Mathf.RoundToInt(currentPrice * priceMultiplier);
        UpdatePriceDisplay();
        Debug.Log($"Цена увеличена до {currentPrice}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter: {other.tag}");
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowPrompt(true);
            Debug.Log("Игрок вошёл в зону покупки");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"OnTriggerExit: {other.tag}");
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ShowPrompt(false);
            Debug.Log("Игрок покинул зону покупки");
        }
    }
}