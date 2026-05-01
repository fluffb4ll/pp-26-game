using UnityEngine;
using TMPro;

public class PlayerEconomy : MonoBehaviour
{
    public int totalCoins = 0;
    public TextMeshProUGUI coinDisplay;  
    public bool hasBrainrot = false;  

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        if (coinDisplay != null)
            coinDisplay.text = $"Монет: {totalCoins}";
        UnityEngine.Debug.Log("Монет теперь: " + totalCoins);
    }

    public void SetBrainrot(bool value)
    {
        hasBrainrot = value;
        UnityEngine.Debug.Log("Брейнрот у игрока: " + hasBrainrot);
    }
}