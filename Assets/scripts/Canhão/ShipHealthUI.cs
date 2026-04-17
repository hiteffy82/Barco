using UnityEngine;
using UnityEngine.UI;

namespace TrilloBit3sIndieGames
{
    public class ShipHealthUI : MonoBehaviour
    {
        public Health health;
        public Image healthFill;

        void Update()
        {
            if (Time.timeScale == 0f) return;
            
            if (health == null || healthFill == null) return;

            float percent = health.currentHealth / health.maxHealth;
            healthFill.fillAmount = percent;
        }
    }
}
