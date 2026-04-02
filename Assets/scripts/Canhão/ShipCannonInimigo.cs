  using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TrilloBit3sIndieGames
{
    public class ShipCannonInimigo : MonoBehaviour
    {
        private Transform ammoTarget;

        [Header("Cano do Canhão")]
        public Transform cannonBarrel; // parte que sobe e desce

        [Header("Referências")]
        public GameObject cannonballPrefab;
        public Transform firePoint;

        [Header("Áudio")]
        public AudioSource audioSource;
        public AudioClip shootSound;

        [Header("Munição")]
        public int maxAmmo = 10;
        public int currentAmmo = 10;

        [Header("Configuração Vertical")]
        public float rotationSpeed = 50f; // suavidade
        public float minPitch = -45f;     // ângulo mínimo para baixo
        public float maxPitch = 45f;     // ângulo máximo para cima

        [Header("Disparo")]
        public float force = 30f; //campo de disparo maior
        public float bulletLifetime = 10f; //campo de disparo maior
        public float spawnOffset = 1.5f;
        public float fireRate = 1f;

        private float currentAngle;
        private float nextFireTime;

        [Header("UI")]
        public TextMeshProUGUI ammoTextQtd;
        public TextMeshProUGUI messageTextEmpty;
        public float messageDuration = 2f;

        [Header("Áudio Extra")]
        public AudioClip emptyAmmoSound;
        public AudioClip pickupAmmoSound;

        [Header("UI - Ícones de Munição")]
        public GameObject ammoIconPrefab;
        public Transform ammoContainer;

        [Header("UI - Ícones de Munição (Manual)")]
        public Image[] ammoIcons;

        [Header("UI - Canhão")]
        public RectTransform cannonUI;

        [Header("UI - Disparo")]
        public GameObject cannonMuzzleFlashUI;
        public float muzzleFlashDuration = 0.1f;
       
        void Start()
        {
            currentAmmo = maxAmmo;

            float angle = transform.localEulerAngles.x;
            currentAngle = (angle > 180f) ? angle - 360f : angle;

            UpdateAmmoUI();
        }

        void Update()
        {
            UpdateCannonUI();
        }

        public void Shoot()
        {
            // posição de spawn (fora do canhão)
            Vector3 spawnPos = firePoint.position + firePoint.forward * spawnOffset;

            GameObject ball = Instantiate(cannonballPrefab, spawnPos, firePoint.rotation);

            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // evita bug de herdar movimento estranho
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // direção com leve elevação (IMPORTANTE)
                Vector3 shootDir = firePoint.forward + Vector3.up * 0.1f;
                shootDir.Normalize();

                // força de disparo (IMPULSO REAL)
                rb.AddForce(shootDir * force, ForceMode.Impulse);
            }

            // ignora colisão com o próprio barco
            Collider ballCol = ball.GetComponent<Collider>();
            Collider cannonCol = GetComponentInParent<Collider>();

            if (ballCol != null && cannonCol != null)
            {
                Physics.IgnoreCollision(ballCol, cannonCol);
            }

            // destrói depois de um tempo
            Destroy(ball, bulletLifetime);

            // som
            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }

        // public void Mirar(Transform alvo)
        // {
        //     if (alvo == null) return;

        //     Vector3 dir = alvo.position - firePoint.position;

        //     float angle = Mathf.Atan2(dir.y, dir.magnitude) * Mathf.Rad2Deg;

        //     currentAngle = Mathf.Clamp(angle, minAngle, maxAngle);

        //     Vector3 angles = transform.localEulerAngles;
        //     angles.x = currentAngle;
        //     transform.localEulerAngles = angles;
        // }

        // Mira no alvo (horizontal + vertical)
  
public void Mirar(Transform alvo)
{
    if (alvo == null || cannonBarrel == null) return;

    // calcula direção relativa ao cano
    Vector3 dir = alvo.position - cannonBarrel.position;

    // ângulo vertical
    float targetAngle = Mathf.Atan2(dir.y, new Vector2(dir.x, dir.z).magnitude) * Mathf.Rad2Deg;

    // limita o ângulo
    targetAngle = Mathf.Clamp(targetAngle, minPitch, maxPitch);

    // aplica suavidade e corrige inversão
    Vector3 localEuler = cannonBarrel.localEulerAngles;
    if (localEuler.x > 180f) localEuler.x -= 360f;

    // Inverte o sinal aqui
    localEuler.x = Mathf.Lerp(localEuler.x, -targetAngle, Time.deltaTime * rotationSpeed);

    cannonBarrel.localEulerAngles = localEuler;
}



        // Função para adicionar munição
        public bool AddAmmo(int amount)
        {
            if (currentAmmo >= maxAmmo)
            {
                ShowMessage("Munição cheia!");
                return false;
            }

            int ammoAntes = currentAmmo;

            currentAmmo += amount;
            currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);

            int ganhoReal = currentAmmo - ammoAntes;

            UpdateAmmoUI();

            if (audioSource != null && pickupAmmoSound != null)
                audioSource.PlayOneShot(pickupAmmoSound);

            ShowMessage("Munição + " + ganhoReal);

            return true;
        }

        void UpdateAmmoUI()
        {
            // Texto
            if (ammoTextQtd != null)
            {
                ammoTextQtd.text = "Munição: " + currentAmmo;
            }

            // Ícones
            for (int i = 0; i < ammoIcons.Length; i++)
            {
                if (ammoIcons[i] != null)
                {
                    Color c = ammoIcons[i].color;
                    c.a = (i < currentAmmo) ? 1f : 0.2f; // cheio ou "apagado"
                    ammoIcons[i].color = c;
                }
            }
        }

        void ShowMessage(string msg)
        {
            if (messageTextEmpty != null)
            {
                StopAllCoroutines();
                StartCoroutine(MessageRoutine(msg));
            }
        }

        System.Collections.IEnumerator MessageRoutine(string msg)
        {
            messageTextEmpty.text = msg;
            messageTextEmpty.gameObject.SetActive(true);

            yield return new WaitForSeconds(messageDuration);

            messageTextEmpty.gameObject.SetActive(false);
        }

        void UpdateCannonUI()
        {
            if (cannonUI == null) return;

            // Inverte se necessário (depende do sprite)
            float uiAngle = -currentAngle;

            cannonUI.localRotation = Quaternion.Euler(0, 0, uiAngle);
        }

        void TriggerMuzzleFlashUI()
        {
            if (cannonMuzzleFlashUI == null) return;

            StopCoroutine("MuzzleFlashRoutine");
            StartCoroutine("MuzzleFlashRoutine");
        }

        System.Collections.IEnumerator MuzzleFlashRoutine()
        {
            cannonMuzzleFlashUI.SetActive(true);

            yield return new WaitForSeconds(muzzleFlashDuration);

            cannonMuzzleFlashUI.SetActive(false);
        }

        void OnDrawGizmosSelected()
        {
            if (firePoint == null) return;

            float range = force * bulletLifetime;

            Gizmos.color = Color.yellow;

            Vector3 origin = firePoint.position;

            Vector3 left = Quaternion.Euler(0, -10, 0) * firePoint.forward * range;
            Vector3 right = Quaternion.Euler(0, 10, 0) * firePoint.forward * range;

            Gizmos.DrawLine(origin, origin + left);
            Gizmos.DrawLine(origin, origin + right);
            Gizmos.DrawLine(origin + left, origin + right);
        }
    }
}

/*
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TrilloBit3sIndieGames
{
    public class ShipCannonInimigo : MonoBehaviour
    {
        [Header("Referências")]
        public GameObject cannonballPrefab;
        public Transform firePoint;

        [Header("Áudio")]
        public AudioSource audioSource;
        public AudioClip shootSound;

        [Header("Munição")]
        public int maxAmmo = 10;
        public int currentAmmo = 10;

        [Header("Movimento do Canhão")]
        public float rotationSpeed = 50f;
        public float minAngle = -45f;
        public float maxAngle = 45f;

        [Header("Disparo")]
        public float force = 30f; //campo de disparo maior
        public float bulletLifetime = 10f; //campo de disparo maior
        public float spawnOffset = 1.5f;
        public float fireRate = 1f;

        private float currentAngle;
        private float nextFireTime;

        [Header("UI")]
        public TextMeshProUGUI ammoTextQtd;
        public TextMeshProUGUI messageTextEmpty;
        public float messageDuration = 2f;

        [Header("Áudio Extra")]
        public AudioClip emptyAmmoSound;
        public AudioClip pickupAmmoSound;

        [Header("UI - Ícones de Munição")]
        public GameObject ammoIconPrefab;
        public Transform ammoContainer;

        [Header("UI - Ícones de Munição (Manual)")]
        public Image[] ammoIcons;

        [Header("UI - Canhão")]
        public RectTransform cannonUI;

        [Header("UI - Disparo")]
        public GameObject cannonMuzzleFlashUI;
        public float muzzleFlashDuration = 0.1f;

        private Transform ammoTarget;
        
        void Start()
        {
            currentAmmo = maxAmmo;

            float angle = transform.localEulerAngles.x;
            currentAngle = (angle > 180f) ? angle - 360f : angle;

            UpdateAmmoUI();
        }

        void Update()
        {
            UpdateCannonUI();
        }

        public void Shoot()
        {
            // posição de spawn (fora do canhão)
            Vector3 spawnPos = firePoint.position + firePoint.forward * spawnOffset;

            GameObject ball = Instantiate(cannonballPrefab, spawnPos, firePoint.rotation);

            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // evita bug de herdar movimento estranho
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // direção com leve elevação (IMPORTANTE)
                Vector3 shootDir = firePoint.forward + Vector3.up * 0.1f;
                shootDir.Normalize();

                // força de disparo (IMPULSO REAL)
                rb.AddForce(shootDir * force, ForceMode.Impulse);
            }

            // ignora colisão com o próprio barco
            Collider ballCol = ball.GetComponent<Collider>();
            Collider cannonCol = GetComponentInParent<Collider>();

            if (ballCol != null && cannonCol != null)
            {
                Physics.IgnoreCollision(ballCol, cannonCol);
            }

            // destrói depois de um tempo
            Destroy(ball, bulletLifetime);

            // som
            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }

        

        public void Mirar(Transform alvo)
        {
            if (alvo == null) return;

            Vector3 dir = alvo.position - firePoint.position;

            float angle = Mathf.Atan2(dir.y, dir.magnitude) * Mathf.Rad2Deg;

            currentAngle = Mathf.Clamp(angle, minAngle, maxAngle);

            Vector3 angles = transform.localEulerAngles;
            angles.x = currentAngle;
            transform.localEulerAngles = angles;
        }

        // Função para adicionar munição
        public bool AddAmmo(int amount)
        {
            if (currentAmmo >= maxAmmo)
            {
                ShowMessage("Munição cheia!");
                return false;
            }

            int ammoAntes = currentAmmo;

            currentAmmo += amount;
            currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);

            int ganhoReal = currentAmmo - ammoAntes;

            UpdateAmmoUI();

            if (audioSource != null && pickupAmmoSound != null)
                audioSource.PlayOneShot(pickupAmmoSound);

            ShowMessage("Munição + " + ganhoReal);

            return true;
        }

        void UpdateAmmoUI()
        {
            // Texto
            if (ammoTextQtd != null)
            {
                ammoTextQtd.text = "Munição: " + currentAmmo;
            }

            // Ícones
            for (int i = 0; i < ammoIcons.Length; i++)
            {
                if (ammoIcons[i] != null)
                {
                    Color c = ammoIcons[i].color;
                    c.a = (i < currentAmmo) ? 1f : 0.2f; // cheio ou "apagado"
                    ammoIcons[i].color = c;
                }
            }
        }

        void ShowMessage(string msg)
        {
            if (messageTextEmpty != null)
            {
                StopAllCoroutines();
                StartCoroutine(MessageRoutine(msg));
            }
        }

        System.Collections.IEnumerator MessageRoutine(string msg)
        {
            messageTextEmpty.text = msg;
            messageTextEmpty.gameObject.SetActive(true);

            yield return new WaitForSeconds(messageDuration);

            messageTextEmpty.gameObject.SetActive(false);
        }

        void UpdateCannonUI()
        {
            if (cannonUI == null) return;

            // Inverte se necessário (depende do sprite)
            float uiAngle = -currentAngle;

            cannonUI.localRotation = Quaternion.Euler(0, 0, uiAngle);
        }

        void TriggerMuzzleFlashUI()
        {
            if (cannonMuzzleFlashUI == null) return;

            StopCoroutine("MuzzleFlashRoutine");
            StartCoroutine("MuzzleFlashRoutine");
        }

        System.Collections.IEnumerator MuzzleFlashRoutine()
        {
            cannonMuzzleFlashUI.SetActive(true);

            yield return new WaitForSeconds(muzzleFlashDuration);

            cannonMuzzleFlashUI.SetActive(false);
        }

        void OnDrawGizmosSelected()
        {
            if (firePoint == null) return;

            float range = force * bulletLifetime;

            Gizmos.color = Color.yellow;

            Vector3 origin = firePoint.position;

            Vector3 left = Quaternion.Euler(0, -10, 0) * firePoint.forward * range;
            Vector3 right = Quaternion.Euler(0, 10, 0) * firePoint.forward * range;

            Gizmos.DrawLine(origin, origin + left);
            Gizmos.DrawLine(origin, origin + right);
            Gizmos.DrawLine(origin + left, origin + right);
        }
    }
}
*/