using UnityEngine;
using UnityEngine.UI;   // для обычного Text и Slider

public class Machine : MonoBehaviour
{
    [Header("Производство")]
    public float productionTime = 1f;
    public GameObject coinPrefab;
    public Transform productionPoint;
    public Slider progressBar;

    [Header("Брейнрот")]
    public bool hasBrainRot = false;
    public GameObject brainRotVisual;

    [Header("Накопленные монеты и UI")]
    public int accumulatedCoins = 0;
    public Text coinsDisplay;        // обычный UI.Text (Legacy)
    public Text statusDisplay;       // обычный UI.Text

    [Header("Экономика")]
    public PlayerEconomy playerEconomy;

    private float productionTimer = 0f;
    private bool isProducing = false;

    void Start()
    {
        if (playerEconomy == null)
            playerEconomy = FindFirstObjectByType<PlayerEconomy>();
        UpdateUI();
        if (progressBar != null) progressBar.value = 0f;
    }

    void Update()
    {
        if (hasBrainRot)
        {
            isProducing = true;
            productionTimer += Time.deltaTime;
            if (progressBar != null)
                progressBar.value = productionTimer / productionTime;

            if (productionTimer >= productionTime)
            {
                productionTimer = 0f;
                accumulatedCoins++;
                UpdateUI();
                SpawnFlyingCoin();
                UnityEngine.Debug.Log($"Произведена монета. Накоплено: {accumulatedCoins}");
            }
        }
        else
        {
            if (isProducing)
            {
                isProducing = false;
                productionTimer = 0f;
                if (progressBar != null) progressBar.value = 0f;
            }
        }
    }

    public void CollectCoins()
    {
        if (accumulatedCoins > 0)
        {
            if (playerEconomy != null)
                playerEconomy.AddCoins(accumulatedCoins);
            else
                UnityEngine.Debug.LogError("PlayerEconomy не найден!");

            accumulatedCoins = 0;
            UpdateUI();
        }
    }

    public bool InsertBrainRot()
    {
        if (!hasBrainRot)
        {
            hasBrainRot = true;
            if (brainRotVisual != null) brainRotVisual.SetActive(true);
            UpdateUI();
            return true;
        }
        return false;
    }

    public bool RemoveBrainRot()
    {
        if (hasBrainRot)
        {
            hasBrainRot = false;
            if (brainRotVisual != null) brainRotVisual.SetActive(false);
            UpdateUI();
            return true;
        }
        return false;
    }

    private void SpawnFlyingCoin()
    {
        if (coinPrefab == null || productionPoint == null) return;
        GameObject coin = Instantiate(coinPrefab, productionPoint.position, Quaternion.identity);
        FlyingCoin flying = coin.AddComponent<FlyingCoin>();
        flying.target = playerEconomy != null ? playerEconomy.transform : null;
        flying.duration = 0.5f;
    }

    private void UpdateUI()
    {
        if (coinsDisplay != null)
            coinsDisplay.text = $"Монет: {accumulatedCoins}";
        if (statusDisplay != null)
            statusDisplay.text = hasBrainRot ? "Брейнрот есть" : "Брейнрот нет";
    }
}