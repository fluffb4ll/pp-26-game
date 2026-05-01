using UnityEngine;
using UnityEngine.Events;

public class PriceButton : MonoBehaviour
{
    public int price = 50;
    public UnityEvent onBuy;

    private PlayerEconomy playerEconomy;
    private bool playerInRange = false;

    void Start()
    {
        playerEconomy = FindFirstObjectByType<PlayerEconomy>(); 
        if (playerEconomy == null)
            UnityEngine.Debug.LogError("PlayerEconomy не найден в сцене!");
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryBuy();
        }
    }

    void TryBuy()
    {
        if (playerEconomy != null && playerEconomy.totalCoins >= price)
        {
            playerEconomy.AddCoins(-price);
            UnityEngine.Debug.Log($"Куплено за {price} монет. Осталось: {playerEconomy.totalCoins}");
            onBuy?.Invoke();
        }
        else
        {
            UnityEngine.Debug.Log("Не хватает монет!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}