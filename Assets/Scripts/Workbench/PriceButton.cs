using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using YG;

public class PriceButton : MonoBehaviour
{
    [SerializeField] private int price = 50;
    [SerializeField] private UnityEvent onBuy;
    [SerializeField] private InputActionReference purchaseAction;

    private bool playerInRange = false;

    private void OnEnable()
    {
        if (purchaseAction != null)
            purchaseAction.action.performed += OnPurchasePerformed;
    }

    private void OnDisable()
    {
        if (purchaseAction != null)
            purchaseAction.action.performed -= OnPurchasePerformed;
    }

    private void OnPurchasePerformed(InputAction.CallbackContext context)
    {
        TryBuy();
    }

    private void TryBuy()
    {
        int currentCoins = YG2.saves.coins;
        if (playerInRange && currentCoins >= price)
        {
            YG2.saves.coins = currentCoins - price;
            Debug.Log($"Bought for {price} coins. Remaining: {YG2.saves.coins}");
            onBuy?.Invoke();
        }
        else if (playerInRange)
        {
            Debug.Log("Not enough coins.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}