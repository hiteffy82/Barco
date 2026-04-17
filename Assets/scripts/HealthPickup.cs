using UnityEngine;

namespace TrilloBit3sIndieGames
{
    public class HealthPickup : MonoBehaviour
    {
        [Header("Configuração")]
        public float healAmount = 25f;
        public float respawnTime = 10f;

        [Header("Efeito")]
        public GameObject pickupEffect;
        public AudioClip pickupSound;

        private AudioSource audioSource;
        private Collider col;
        private Renderer rend;

        private Vector3 originalScale;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            col = GetComponent<Collider>();
            rend = GetComponent<Renderer>();

            originalScale = transform.localScale; // guarda escala inicial
        }

        void Update() { if (Time.timeScale == 0f) return; }

        private void OnTriggerEnter(Collider other)
        {
            Health health = other.GetComponent<Health>();

            if (health != null)
            {
                if (health.currentHealth >= health.maxHealth)
                    return;

                health.Heal(healAmount);

                if (pickupEffect != null)
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);

                if (audioSource != null && pickupSound != null)
                    audioSource.PlayOneShot(pickupSound);

                StartCoroutine(Respawn());
            }
        }

        System.Collections.IEnumerator Respawn()
        {
            col.enabled = false;
            if (rend != null) rend.enabled = false;

            yield return new WaitForSeconds(respawnTime);

            col.enabled = true;
            if (rend != null) rend.enabled = true;

            // animação de aparecer
            transform.localScale = Vector3.zero;

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 2f;
                transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                yield return null;
            }
        }
    }
}