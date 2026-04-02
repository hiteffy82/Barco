using System.Collections;
using UnityEngine;

namespace TrilloBit3sIndieGames
{
    public class EnemyDeath : MonoBehaviour
    {
        private Health health;

        void Awake()
        {
            health = GetComponent<Health>();
        }

        void Start()
        {
            if (health != null)
            {
                health.OnDeath += Die;
            }
        }

        void Die()
        {
            StartCoroutine(DieRoutine());
        }

        IEnumerator DieRoutine()
        {
            // desativa colisão
            GetComponent<Collider>().enabled = false;

            // desativa física
            GetComponent<Rigidbody>().isKinematic = true;

            // toca animação / partículas aqui

            yield return new WaitForSeconds(1f);

            Destroy(gameObject);
        }
    }
}
