using UnityEngine;
using YG;

public class Workbench : MonoBehaviour, IInteractable
{
    public float baseProduce;
    public float produceStoreCap;
    public float storedProduce;
    
    [SerializeField] private Transform brainrotInsertionPos;
    [SerializeField] private Brainrot insertedBrainrot;

    private float _diff = 0.0001f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CalculateProduce();
    }
    
    /// <summary>
    /// Вырабатывает монетки за единицу времени и снижает ресурс вставленного брейнрота 
    /// </summary>
    private void CalculateProduce()
    {
        if (insertedBrainrot is null || produceStoreCap - storedProduce < _diff)
            return;
        
        storedProduce += (baseProduce + insertedBrainrot.produce) * Time.deltaTime;
        
        if (storedProduce > produceStoreCap)
            storedProduce = produceStoreCap;
        
        insertedBrainrot.lifetime -= Time.deltaTime;

        if (!(insertedBrainrot.lifetime <= 0))
            return;
        
        Destroy(insertedBrainrot.gameObject);
        insertedBrainrot = null;
    }

    public void Interact(PlayerInteraction player)
    {
        if (player.heldBrainrot is not null)
            InsertBrainrot(player);
        else
        {
            YG2.saves.coins += Mathf.RoundToInt(storedProduce);
            storedProduce = 0;
            Debug.Log($"New balance: {YG2.saves.coins}");
        }
    }

    private void InsertBrainrot(PlayerInteraction player)
    {
        insertedBrainrot = player.heldBrainrot;
        player.heldBrainrot = null;
        
        insertedBrainrot.transform.SetParent(brainrotInsertionPos);
        insertedBrainrot.transform.position = brainrotInsertionPos.position;
    }
}
