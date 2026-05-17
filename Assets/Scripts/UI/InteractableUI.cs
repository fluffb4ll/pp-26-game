using Camera;
using Interfaces;
using Player;
using UnityEngine;

namespace UI
{
    public class InteractableUI : MonoBehaviour, IUIPrompts
    {
        [SerializeField] private GameObject inputPromptCanvas;
        [SerializeField] private float uiMovementRadius;
        [SerializeField] private float uiMovementSpeed = 10f;
        [SerializeField] private new Collider collider;
        
        [SerializeField] private InfoUI uiInfoComponent; 
            
        private SimpleFollowCamera _camera;
        private PlayerMovement _playerMovement;
        private bool hasInfoComponent;
        
        private void Awake()
        {
            _camera = SimpleFollowCamera.Instance;
            _playerMovement = PlayerMovement.Instance;
        }

        private void Start()
        {
            inputPromptCanvas.SetActive(false);
            hasInfoComponent = uiInfoComponent is not null;
        }
    
        private void OnEnable()
        {
            _camera.OnCamRotation += RotateCanvas;
        }

        private void OnDisable()
        {
            _camera.OnCamRotation -= RotateCanvas;
        }
        
        /// <inheritdoc/>
        public void ShowInteractionPrompts()
        {
            if (hasInfoComponent)
                _playerMovement.OnMovement += uiInfoComponent.MoveCanvas;
            _playerMovement.OnMovement += MoveInputPrompt;
            inputPromptCanvas.SetActive(true);
        }
        
        /// <inheritdoc/>
        public void HideInteractionPrompts()
        {
            _playerMovement.OnMovement -= MoveInputPrompt;
            inputPromptCanvas.SetActive(false);
            if (!hasInfoComponent) return;
            _playerMovement.OnMovement -= uiInfoComponent.MoveCanvas;
            uiInfoComponent.SetIsInDefaultSpot();
            uiInfoComponent.ReturnInfoCanvasToDefaultPos();
        }
        
        /// <summary>
        /// Вращает <c>inputPromptCanvas</c> относительно поворота камеры
        /// </summary>
        /// <param name="rotation">Вращение камеры</param>
        private void RotateCanvas(Quaternion rotation)
        {
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
        /// Выключает компоненты, связанные с UI
        /// </summary>
        public void DisableUIComponents()
        {
            inputPromptCanvas.SetActive(false);
            if (!hasInfoComponent) return;
            _playerMovement.OnMovement -= uiInfoComponent.MoveCanvas;
            uiInfoComponent.SetInfoCanvasActiveState(false);
            collider.enabled = false;
        }

        /// <summary>
        /// Включает компоненты, связанные с UI
        /// </summary>
        public void EnableUIComponents()
        {
            inputPromptCanvas.SetActive(true);
            if (!hasInfoComponent) return;
            _playerMovement.OnMovement += uiInfoComponent.MoveCanvas;
            uiInfoComponent.SetInfoCanvasActiveState(false);
            collider.enabled = true;
        }
    }
}