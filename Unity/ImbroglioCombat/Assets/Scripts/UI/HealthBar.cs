using UnityEngine;
using UnityEngine.UI;

namespace ImbroglioCombat.UI
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Health Bar Components")]
        public Slider healthSlider;
        public Image fillImage;
        
        [Header("Colors")]
        public Gradient healthGradient;
        public Color fullHealthColor = Color.green;
        public Color halfHealthColor = Color.yellow;
        public Color lowHealthColor = Color.red;

        [Header("Settings")]
        public Vector3 offset = new Vector3(0, 1.5f, 0);
        public bool followTarget = true;
        
        private Transform targetTransform;
        private int maxHealth;
        private int currentHealth;
        private Camera mainCamera;

        void Awake()
        {
            if (healthSlider == null)
            {
                healthSlider = GetComponentInChildren<Slider>();
            }

            if (fillImage == null && healthSlider != null)
            {
                fillImage = healthSlider.fillRect.GetComponent<Image>();
            }

            mainCamera = Camera.main;
            targetTransform = transform.parent;
        }

        void LateUpdate()
        {
            if (followTarget && targetTransform != null)
            {
                transform.position = targetTransform.position + offset;
                
                // Make health bar face camera
                if (mainCamera != null)
                {
                    transform.rotation = mainCamera.transform.rotation;
                }
            }
        }

        public void SetMaxHealth(int max)
        {
            maxHealth = max;
            
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = max;
            }
            
            currentHealth = max;
            UpdateColor();
        }

        public void UpdateHealth(int health)
        {
            currentHealth = health;
            
            if (healthSlider != null)
            {
                healthSlider.value = health;
            }
            
            UpdateColor();
        }

        void UpdateColor()
        {
            if (fillImage == null || maxHealth == 0)
            {
                return;
            }

            float healthPercent = (float)currentHealth / maxHealth;

            if (healthGradient != null)
            {
                fillImage.color = healthGradient.Evaluate(healthPercent);
            }
            else
            {
                // Simple color interpolation
                if (healthPercent > 0.5f)
                {
                    fillImage.color = Color.Lerp(halfHealthColor, fullHealthColor, (healthPercent - 0.5f) * 2f);
                }
                else
                {
                    fillImage.color = Color.Lerp(lowHealthColor, halfHealthColor, healthPercent * 2f);
                }
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}

