using Camera;
using Interfaces;
using Player;
using UnityEngine;

namespace Brainrot
{
    public class BrainrotUI : MonoBehaviour, IUIPrompts
    {
        [SerializeField] private GameObject infoCanvas;
        [SerializeField] private GameObject inputPromptCanvas;
        [SerializeField] private float inputPromptMovementRadius;
        [SerializeField] private float inputPromptMovementSpeed;
        [SerializeField] private new Collider collider;
    
        private SimpleFollowCamera _camera;
        private PlayerMovement _playerMovement;
        
        private void Awake()
        {
            _camera = SimpleFollowCamera.Instance;
            _playerMovement = PlayerMovement.Instance;
        }

        private void Start()
        {
            inputPromptCanvas.SetActive(false);
        }
    
        private void OnEnable()
        {
            _camera.OnCamRotation += RotateCanvas;
        }

        private void OnDisable()
        {
            _camera.OnCamRotation -= RotateCanvas;
        }

        public void ShowPrompts()
        {
            _playerMovement.OnMovement += MoveInputPrompt;
            inputPromptCanvas.SetActive(true);
        }

        public void HidePrompts()
        {
            _playerMovement.OnMovement -= MoveInputPrompt;
            inputPromptCanvas.SetActive(false);
        }
    
        private void RotateCanvas(Quaternion rotation)
        {
            infoCanvas.transform.rotation = rotation;
            inputPromptCanvas.transform.rotation = rotation;
        }

        private void MoveInputPrompt(Vector3 pos)
        {
            var playerLocalPos = transform.InverseTransformPoint(pos);
            var inputPromptCanvasPos = inputPromptCanvas.transform.localPosition;
            var direction = playerLocalPos.normalized;
            var targetPos = direction * inputPromptMovementRadius;
            targetPos.x = Mathf.Clamp(targetPos.x, -inputPromptMovementRadius, inputPromptMovementRadius);
            targetPos.y = inputPromptCanvasPos.y;
            targetPos.z = Mathf.Clamp(targetPos.z, -inputPromptMovementRadius, inputPromptMovementRadius);
            inputPromptCanvas.transform.localPosition = Vector3.Lerp(inputPromptCanvasPos, targetPos, Time.deltaTime * inputPromptMovementSpeed);
        }

        public void DisableComponentsOnInsertion()
        {
            infoCanvas.SetActive(false);
            inputPromptCanvas.SetActive(false);
            collider.enabled = false;
        }
    }
}
