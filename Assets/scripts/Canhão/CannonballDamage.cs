using UnityEngine;
using TrilloBit3sIndieGames;

namespace TrilloBit3sIndieGames
{
    public class CannonballDamage : MonoBehaviour
    {
        public float damage = 25f;

        private void OnCollisionEnter(Collision collision)
        {
            Health target = collision.gameObject.GetComponentInParent<Health>();

            if (target != null)
            {
                target.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}