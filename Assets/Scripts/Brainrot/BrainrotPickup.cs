using UnityEngine;
using YG;

public class BrainrotPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            YG2.SetState("player_brainrot", 1);
            UnityEngine.Debug.Log("Player got brainrot!");
            Destroy(gameObject);
        }
    }
}