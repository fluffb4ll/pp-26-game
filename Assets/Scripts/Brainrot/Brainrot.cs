using UnityEngine;

public class Brainrot : MonoBehaviour
{
    [SerializeField] public float produce;
    [SerializeField] public float lifetime;
    [SerializeField] private BrainrotLib data;

    [SerializeField] public Rarity rarity;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var rolledConfig = data.getRandomizedRarity();
        
        rarity = rolledConfig.rarity;
        produce = data.baseProduce * rolledConfig.produceMult;
        lifetime = data.baseLifetime * rolledConfig.lifetimeMult;
        Debug.Log($"Pulled: {rarity} {data.type}.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
