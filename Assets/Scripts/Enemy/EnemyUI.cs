using Camera;
using UnityEngine;

namespace Enemy
{
    public class EnemyUI : MonoBehaviour
    {
        [SerializeField] private EnemyCombat enemyCombat;
        [SerializeField] private GameObject canvas;
        [SerializeField] private RectTransform hpBarFill;
        
        private float _hpBarFillMaxWidth;
        private SimpleFollowCamera _camera;

        private void Awake()
        {
            _camera = SimpleFollowCamera.Instance;
        }
        
        private void Start()
        {
            _hpBarFillMaxWidth = hpBarFill.rect.width;
        }

        private void OnEnable()
        {
            enemyCombat.OnHeal += UpdateHpBarFill;
            enemyCombat.OnTakeDamage += UpdateHpBarFill;
            enemyCombat.OnDeath += ToggleCanvas;
            _camera.OnCamRotation += RotateCanvas;
        }

        private void OnDisable()
        {
            enemyCombat.OnHeal -= UpdateHpBarFill;
            enemyCombat.OnTakeDamage -= UpdateHpBarFill;
            enemyCombat.OnDeath -= ToggleCanvas;
            _camera.OnCamRotation -= RotateCanvas;
        }

        private void ToggleCanvas()
        {
            canvas.SetActive(!canvas.activeSelf);
        }
        
        private void UpdateHpBarFill(float hpPercent)
        {
            hpBarFill.offsetMax -= new Vector2(_hpBarFillMaxWidth * (1.0f - hpPercent) + hpBarFill.sizeDelta.x, 0);
        }

        private void RotateCanvas(Quaternion rotation)
        {
            canvas.transform.rotation = rotation;
        }
    }
}
