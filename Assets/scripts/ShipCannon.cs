using UnityEngine;

public class ShipCannon : MonoBehaviour
{
    [Header("Referências")]
    public GameObject cannonballPrefab;
    public Transform firePoint;

    [Header("Movimento do Canhão")]
    public float rotationSpeed = 50f;
    public float minAngle = -10f;
    public float maxAngle = 45f;

    [Header("Disparo")]
    public float force = 500f;

    private float currentAngle = 0f;

    void Update()
    {
        // SUBIR (NUMPAD 8)
        if (Input.GetKey(KeyCode.Keypad8))
        {
            currentAngle += rotationSpeed * Time.deltaTime;
        }

        // DESCER (NUMPAD 2)
        if (Input.GetKey(KeyCode.Keypad2))
        {
            currentAngle -= rotationSpeed * Time.deltaTime;
        }

        // Limita o ângulo
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // Aplica rotação (eixo X)
        transform.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);

        // DISPARO (NUMPAD 5)
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject ball = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * force, ForceMode.Impulse);
        }
    }
}


/*
using UnityEngine;

public class ShipCannon : MonoBehaviour
{
    [Header("Referências")]
    public GameObject cannonballPrefab;
    public Transform firePoint;

    [Header("Configuração")]
    public float force = 500f;

    void Update()
    {
        // Teclado (U) ou Gamepad (botão padrão "Fire1")
        if (Input.GetKeyDown(KeyCode.U) || Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject ball = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * force);
        }
    }
}
*/