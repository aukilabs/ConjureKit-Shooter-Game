using UnityEngine;
using UnityEngine.UI;

namespace ConjureKitShooter.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image healthBar;
        [SerializeField] private Gradient healthBarColor;
        [SerializeField] private Image heartIcon;


        public void ShowHealthBar(bool show)
        {
            StopAllCoroutines();
        
            this.LerpFloat(show ? 0f : 1f, show ? 1f: 0f, 0.5f, val =>
            {
                canvasGroup.alpha = val;
            });
        }

        public void SetHealthBarAlpha(float val)
        {
            canvasGroup.alpha = val;
        }

        public void UpdateHealth(float normalizeHealth)
        {
            healthBar.color = healthBarColor.Evaluate(normalizeHealth);
            healthBar.fillAmount = normalizeHealth;
        }
    }
}