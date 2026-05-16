using Camera;
using Interfaces;
using Player;
using UnityEngine;

namespace Boat
{
    public class BoatUI : MonoBehaviour, IUIPrompts
    {
        [SerializeField] private GameObject inputPromptCanvas;
        [SerializeField] private float uiMovementRadius;
        [SerializeField] private float uiMovementSpeed;
    
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

        /// <inheritdoc/>
        public void ShowPrompts()
        {
            _playerMovement.OnMovement += MoveInputPrompt;
            inputPromptCanvas.SetActive(true);
        }

        /// <inheritdoc/>
        public void HidePrompts()
        {
            _playerMovement.OnMovement -= MoveInputPrompt;
            inputPromptCanvas.SetActive(false);
        }
        
        /// <summary>
        /// Вращает <c>Canvas</c> относительно поворота камеры
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
    }
}
