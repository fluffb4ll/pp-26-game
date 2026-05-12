using Camera;
using Interfaces;
using Player;
using TMPro;
using UnityEngine;

namespace Workbench
{
    public class WorkbenchUI : MonoBehaviour, ITriggerable
    {
        [SerializeField] private Workbench workbenchController;
        [SerializeField] private GameObject infoCanvas;
        [SerializeField] private GameObject inputPromptCanvas;
        [SerializeField] private TextMeshProUGUI produceCounter;
        [SerializeField] private float inputPromptMovementRadius;
        [SerializeField] private float inputPromptMovementSpeed;
        [SerializeField] private float zAxisBorders;
        
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
            UpdateProduceCounter();
        }

        private void OnEnable()
        {
            workbenchController.OnProduceUpdate += UpdateProduceCounter;
            _camera.OnCamRotation += RotateCanvas;
        }

        private void OnDisable()
        {
            workbenchController.OnProduceUpdate -= UpdateProduceCounter;
            _camera.OnCamRotation -= RotateCanvas;
        }
        
        /// <inheritdoc/>
        public void Execute(PlayerController playerController)
        {
            _playerMovement.OnMovement += MoveInputPrompt;
            inputPromptCanvas.SetActive(true);
        }
        
        /// <inheritdoc/>
        public void Exit(PlayerController playerController)
        {
            _playerMovement.OnMovement -= MoveInputPrompt;
            inputPromptCanvas.SetActive(false);
        }

        private void UpdateProduceCounter()
        {
            var amount = Mathf.RoundToInt(workbenchController.storedProduce);
            
            var newValue = amount switch
            {
                > 1000000000 => (amount / 1000000000.0).ToString("F1") + "B",
                > 1000000 => (amount / 1000000.0).ToString("F1") + "M",
                > 10000 => (amount / 1000.0).ToString("F1") + "K",
                _ => amount.ToString()
            };

            produceCounter.text = newValue;
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
            targetPos.y = inputPromptCanvasPos.y;
            targetPos.z = Mathf.Clamp(targetPos.z, -zAxisBorders, zAxisBorders);
            inputPromptCanvas.transform.localPosition = Vector3.Lerp(inputPromptCanvasPos, targetPos, Time.deltaTime * inputPromptMovementSpeed);
        }
    }
}
