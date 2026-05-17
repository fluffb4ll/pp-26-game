using Camera;
using Interfaces;
using UnityEngine;

namespace UI
{
    public abstract class InfoUI : MonoBehaviour, IUIInfo
    {
        [SerializeField] private GameObject infoCanvas;
        [SerializeField] private float uiMovementRadius = 1.5f;
        [SerializeField] private float uiMovementSpeed = 10f;
        
        private Vector3 _infoCanvasDefaultPosition;
        private bool _isInfoCanvasInDefaultPos = true;
        
        private SimpleFollowCamera _camera;
        
        protected virtual void Awake()
        {
            _camera = SimpleFollowCamera.Instance;
        }
        
        protected virtual void Start()
        {
            _infoCanvasDefaultPosition = infoCanvas.transform.localPosition;
        }

        protected virtual void Update()
        {
            if (!_isInfoCanvasInDefaultPos)
                ReturnInfoCanvasToDefaultPos();
        }

        protected virtual void OnEnable()
        {
            _camera.OnCamRotation += RotateCanvas;
        }

        protected virtual void OnDisable()
        {
            _camera.OnCamRotation -= RotateCanvas;
        }
        
        /// <inheritdoc/>
        public void RotateCanvas(Quaternion rotation)
        {
            infoCanvas.transform.rotation = rotation;
        }
        
        /// <inheritdoc/>
        public void MoveCanvas(Vector3 pos)
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
        
        /// <inheritdoc/>
        public void ReturnInfoCanvasToDefaultPos()
        {
            infoCanvas.transform.localPosition = Vector3.Lerp(
                infoCanvas.transform.localPosition,
                _infoCanvasDefaultPosition, 
                Time.deltaTime * uiMovementSpeed);

            if (infoCanvas.transform.localPosition == _infoCanvasDefaultPosition)
                _isInfoCanvasInDefaultPos = true;
        }
        
        /// <inheritdoc/>
        public void SetInfoCanvasActiveState(bool newState)
        {
            infoCanvas.SetActive(newState);
        }
    }
}