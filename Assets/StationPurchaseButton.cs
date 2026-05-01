using UnityEngine;
using TMPro;

public class StationPurchaseButton : MonoBehaviour
{
    [Header("Стоимость")]
    public int startPrice = 50;
    public float priceMultiplier = 1.53f;
    private int currentPrice;

    [Header("Префаб станка")]
    public GameObject stationPrefab;

    [Header("Массивы позиций (одинаковая длина)")]
    public Transform[] spawnPoints;     // позиции для станков: [0] – первый станок, [1] – второй и т.д.
    public Transform[] buttonPositions; // позиции для кнопки: [0] – начальная, [1] – после 1 покупки, ...

    private int currentIndex = 0;

    [Header("UI (должен быть дочерним объектом кнопки)")]
    public TextMeshProUGUI priceText;
    public GameObject promptPanel;

    [Header("Экономика")]
    public PlayerEconomy playerEconomy;
    public NotificationUI notificationUI;

    private bool playerInRange = false;

    void Start()
    {
        if (buttonPositions.Length > 0)
            transform.position = buttonPositions[0].position;
        else
            UnityEngine.Debug.LogError("Нет позиций для кнопки!");

        currentPrice = startPrice;
        if (playerEconomy == null)
            playerEconomy = FindFirstObjectByType<PlayerEconomy>();

        UpdatePriceDisplay();
        if (promptPanel != null) promptPanel.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
            TryPurchase();
    }

    void TryPurchase()
    {
        if (playerEconomy == null) return;

        if (currentIndex >= spawnPoints.Length)
        {
            UnityEngine.Debug.Log("Все станки куплены. Кнопка самоуничтожится.");
            Destroy(gameObject);
            return;
        }

        if (playerEconomy.totalCoins < currentPrice)
        {
            notificationUI?.ShowMessage("Не хватает монет!");
            return;
        }

        // Списать монеты
        playerEconomy.AddCoins(-currentPrice);

        // Создать станок в текущей точке спавна
        Instantiate(stationPrefab, spawnPoints[currentIndex].position, spawnPoints[currentIndex].rotation);

        // Переместить кнопку на следующую позицию
        currentIndex++;
        if (currentIndex < buttonPositions.Length)
            transform.position = buttonPositions[currentIndex].position;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Повысить цену
        currentPrice = Mathf.RoundToInt(currentPrice * priceMultiplier);
        UpdatePriceDisplay();
    }

    void UpdatePriceDisplay()
    {
        if (priceText != null)
            priceText.text = $"Цена: {currentPrice}";
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptPanel != null) promptPanel.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptPanel != null) promptPanel.SetActive(false);
        }
    }
}