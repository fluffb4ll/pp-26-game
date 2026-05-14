using Camera;
using Interfaces;
using Player;
using TMPro;
using UnityEngine;

namespace Workbench
{
    /// <summary>
    /// Управляет элементами интерфейса, связанными со станком
    /// </summary>
    public class WorkbenchUI : MonoBehaviour, IUIPrompts
    {
        [SerializeField] private Workbench workbenchController;
        [SerializeField] private GameObject infoCanvas;
        [SerializeField] private GameObject inputPromptCanvas;
        [SerializeField] private TextMeshProUGUI produceCounter;
        [SerializeField] private float uiMovementRadius;
        [SerializeField] private float uiMovementSpeed;
        
        private Vector3 _infoCanvasDefaultPosition;
        private bool _isInfoCanvasInDefaultPos = true;
        
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
            _infoCanvasDefaultPosition = infoCanvas.transform.localPosition;
        }

        private void Update()
        {
            if (!_isInfoCanvasInDefaultPos)
                ReturnInfoCanvasToDefaultPos();
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

        /// <summary>
        /// Обновляет счётчик хранимого в станке ресурса 
        /// </summary>
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
        
        /// <summary>
        /// Вращает <c>Canvas</c> относительно поворота камеры
        /// </summary>
        /// <param name="rotation">Вращение камеры</param>
        private void RotateCanvas(Quaternion rotation)
        {
            infoCanvas.transform.rotation = rotation;
            inputPromptCanvas.transform.rotation = rotation;
        }

        /// <summary>
        /// Двигает промпт взаимодействия в определённом радиусе вокруг объекта относительно позиции игрока
        /// </summary>
        /// <param name="pos">Позиция игрока в мире</param>
        private void MoveInputPrompt(Vector3 pos)
        {
            var playerLocalPos = transform.InverseTransformPoint(pos);
            var inputPromptCanvasPos = inputPromptCanvas.transform.localPosition;
            var direction = playerLocalPos.normalized;
            var targetPos = direction * uiMovementRadius;
            targetPos.x = Mathf.Clamp(targetPos.x, -uiMovementRadius, uiMovementRadius);
            targetPos.y = inputPromptCanvasPos.y;
            targetPos.z = Mathf.Clamp(targetPos.z, -uiMovementRadius, uiMovementRadius);
            inputPromptCanvas.transform.localPosition = Vector3.Lerp(
                inputPromptCanvasPos, 
                targetPos, 
                Time.deltaTime * uiMovementSpeed);
        }
        
        /// <summary>
        /// Двигает информационную панель в определённом радиусе вокруг объекта относительно позиции игрока
        /// </summary>
        /// <param name="pos">Позиция игрока в мире</param>
        private void MoveInfoCanvas(Vector3 pos)
        {
            var playerLocalPos = transform.InverseTransformPoint(pos);
            var infoCanvasPos = infoCanvas.transform.localPosition;
            var direction = playerLocalPos.normalized;
            var targetPos = direction * uiMovementRadius;
            targetPos.x = Mathf.Clamp(targetPos.x, -uiMovementRadius, uiMovementRadius);
            targetPos.y = infoCanvasPos.y;
            targetPos.z = Mathf.Clamp(targetPos.z, -uiMovementRadius, uiMovementRadius);
            infoCanvas.transform.localPosition = Vector3.Lerp(
                infoCanvasPos, 
                targetPos, 
                Time.deltaTime * uiMovementSpeed);
        }
        
        /// <summary>
        /// Возвращает <c>InfoCanvas</c> в его начальную позицию
        /// </summary>
        private void ReturnInfoCanvasToDefaultPos()
        {
            infoCanvas.transform.localPosition = Vector3.Lerp(
                infoCanvas.transform.localPosition,
                _infoCanvasDefaultPosition, 
                Time.deltaTime * uiMovementSpeed);

            if (infoCanvas.transform.localPosition == _infoCanvasDefaultPosition)
                _isInfoCanvasInDefaultPos = true;
        }
        
        /// <inheritdoc/>
        public void ShowPrompts()
        {
            _playerMovement.OnMovement += MoveInputPrompt;
            _playerMovement.OnMovement += MoveInfoCanvas;
            inputPromptCanvas.SetActive(true);
        }

        /// <inheritdoc/>
        public void HidePrompts()
        {
            _playerMovement.OnMovement -= MoveInputPrompt;
            _playerMovement.OnMovement -= MoveInfoCanvas;
            inputPromptCanvas.SetActive(false);
            _isInfoCanvasInDefaultPos = false;
            ReturnInfoCanvasToDefaultPos();
        }
    }
}
