//Nao precisamos ver a UI do inimigo por enquanto
using UnityEngine;
using UnityEngine.UI;

namespace TrilloBit3sIndieGames
{
    public class EnemyHealthUI : MonoBehaviour
    {
        public Health health;
        public Image healthFill;

        void Start()
        {
            if (health != null)
            {
                health.OnDamage += UpdateHealth;
                health.OnDeath += UpdateHealthZero;
            }
        }

        void UpdateHealth(float damage)
        {
            UpdateBar();
        }

        void UpdateHealthZero()
        {
            UpdateBar();
        }

        void UpdateBar()
        {
            if (health == null || healthFill == null) return;

            float percent = health.currentHealth / health.maxHealth;
            healthFill.fillAmount = percent;
        }

        void OnDestroy()
        {
            if (health != null)
            {
                health.OnDamage -= UpdateHealth;
                health.OnDeath -= UpdateHealthZero;
            }
        }
    }
}
