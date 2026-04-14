using UnityEngine;

public class Brainrot : MonoBehaviour, IInteractable
{
    public float produce;
    public float lifetime;
    [SerializeField] private BrainrotLib data;

    public Rarity rarity;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var rolledConfig = data.getRandomizedRarity();
        
        rarity = rolledConfig.rarity;
        produce = data.baseProduce * rolledConfig.produceMult;
        lifetime = data.baseLifetime * rolledConfig.lifetimeMult;
        Debug.Log($"Pulled: {rarity} {data.type}.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log("Collided with player");
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact(PlayerInteraction player)
    {
        if (player.heldBrainrot is null)
            PickUp(player);
    }

    private void PickUp(PlayerInteraction player)
    {
        player.heldBrainrot = this;
        
        transform.SetParent(player.brainrotCarryPoint);
        transform.localPosition = Vector3.zero;
    }
}
