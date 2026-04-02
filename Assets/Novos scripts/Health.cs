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

        void Die()
        {
            OnDeath?.Invoke(); // avisa quem estiver ouvindo
        }
    }
}
