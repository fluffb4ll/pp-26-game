using Interfaces;
using UnityEngine;

namespace Enemy
{
    public class DefaultEnemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private GameObject spawnableBrainrot;
        [SerializeField] private float timeBeforeDestroyingBody;
        [SerializeField] private float deathAnimationSpeed;
    
        public int maxHealth;
        public int currentHealth;

        public int damage;
        private bool _isDying;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (_isDying)
            {
                if (transform.rotation.x > -0.5f)
                    PlayDeathAnimation(deathAnimationSpeed);
                else if (timeBeforeDestroyingBody > 0)
                    timeBeforeDestroyingBody -= Time.deltaTime;
                else
                    Destroy(gameObject);
            }
        }

        /// <inheritdoc/>
        public void TakeDamage(int damageAmount)
        {
            currentHealth -= damageAmount;
            
            if (currentHealth <= 0)
                Die();
        }

        /// <inheritdoc/>
        public void Heal(int healAmount)
        {
            currentHealth += healAmount;
            
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
    
        /// <inheritdoc/>
        public void Die()
        {
            currentHealth = 0;
            _isDying = true;
            SpawnBrainrot();
            //charController.enabled = playerMovement.enabled = playerInteraction.enabled = false;
        }
    
        /// <summary>
        /// Проигрывает анимацию смерти - переворачивает врага на "спину"
        /// </summary>
        /// <param name="rotationSpeed">Скорость вращения</param>
        private void PlayDeathAnimation(float rotationSpeed)
        {
            Quaternion currentRot = transform.rotation;
            Quaternion targetRot = currentRot * Quaternion.AngleAxis(90f, new Vector3(-90f, currentRot.y, 0f));
            transform.rotation = Quaternion.RotateTowards(currentRot, targetRot, Time.deltaTime * rotationSpeed);
        }

        /// <summary>
        /// Спавнит брейнрота после смерти персонажа
        /// </summary>
        private void SpawnBrainrot()
        {
            Instantiate(spawnableBrainrot, transform.position, Quaternion.identity);
        }
    }
}
