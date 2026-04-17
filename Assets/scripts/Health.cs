using UnityEngine;
using System;

namespace TrilloBit3sIndieGames
{
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float currentHealth;

        public event Action OnDeath;
        public event Action<float> OnDamage;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        void Update() { if (Time.timeScale == 0f) return; }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;

            OnDamage?.Invoke(amount);

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (currentHealth <= 0f) return; // opcional: não cura morto

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        void Die()
        {
            OnDeath?.Invoke(); // avisa quem estiver ouvindo
        }
    }
}