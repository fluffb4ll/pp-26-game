using Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform cameraTransform; 
    [SerializeField] private InputActionReference interact;
    
    public Transform brainrotCarryPoint;
    public Brainrot heldBrainrot;

    private InputAction _interactAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_interactAction is null)
            _interactAction = interact.action;
    }

    void OnEnable()
    {
        if (_interactAction is null)
            _interactAction = interact.action;
        _interactAction.Enable();
        _interactAction.performed += OnInteract;
    }

    void OnDisable()
    {
        _interactAction.Disable();
        _interactAction.performed -= OnInteract;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnInteract(InputAction.CallbackContext context) => PerformRaycast();

    private void PerformRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        
            interactable?.Interact(this);
        }
    }
}
