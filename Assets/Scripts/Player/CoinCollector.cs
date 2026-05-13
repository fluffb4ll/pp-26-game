using UnityEngine;
using YG;

public class CoinCollector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            YG2.saves.coins += 1;
            UnityEngine.Debug.Log($"Монета собрана. Всего: {YG2.saves.coins}");
            Destroy(gameObject);
        }
    }
}