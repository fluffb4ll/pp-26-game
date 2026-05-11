using Camera;
using TMPro;
using UnityEngine;

namespace Workbench
{
    public class WorkbenchUI : MonoBehaviour
    {
        [SerializeField] private Workbench workbenchController;
        [SerializeField] private GameObject canvas;
        [SerializeField] private TextMeshProUGUI produceCounter;

        private SimpleFollowCamera _camera;

        private void Awake()
        {
            _camera = SimpleFollowCamera.Instance;
        }
        
        private void Start()
        {
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
            canvas.transform.rotation = rotation;
        }
    }
}
