using UnityEngine;

public class Workbench : MonoBehaviour
{
    [SerializeField] public float baseProduce;
    [SerializeField] public float produceStoreCap;
    
    [SerializeField] public Transform brainrotInsertionPos;
    [SerializeField] public Brainrot insertedBrainrot;
    [SerializeField] public float storedProduce;

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
}
