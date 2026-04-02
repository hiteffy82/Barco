using UnityEngine;

namespace TrilloBit3sIndieGames
{
    public class AmmoPickup : MonoBehaviour
    {
        [Header("Configuração")]
        public int ammoAmount = 5;
        public float respawnTime = 10f;

        private Collider col;
        private Renderer rend;

        void Start()
        {
            col = GetComponent<Collider>();
            rend = GetComponent<Renderer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            ShipCannon cannon = other.GetComponentInChildren<ShipCannon>();

            if (cannon != null && col.enabled)
            {
                bool pegou = cannon.AddAmmo(ammoAmount);

                if (pegou)
                {
                    StartCoroutine(Respawn());
                }
            }
        }

        System.Collections.IEnumerator Respawn()
        {
            col.enabled = false;
            rend.enabled = false;

            yield return new WaitForSeconds(respawnTime);

            col.enabled = true;
            rend.enabled = true;

            // efeito de "aparecendo"
            transform.localScale = Vector3.zero;

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 2f;
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }
        }
    }
}