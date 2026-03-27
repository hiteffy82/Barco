using UnityEngine;

public class ShipCannon : MonoBehaviour
{
    [Header("Referências")]
    public GameObject cannonballPrefab;
    public Transform firePoint;

    [Header("Movimento do Canhão")]
    public float rotationSpeed = 50f;
    public float minAngle = -45f;
    public float maxAngle = 45f;

    [Header("Disparo")]
    public float force = 50f;
    public float spawnOffset = 0.7f;

    private float currentAngle;

    void Start()
    {
        float angle = transform.localEulerAngles.x;
        currentAngle = (angle > 180f) ? angle - 360f : angle;
    }

    void Update()
    {
        bool moved = false;

        if (Input.GetKey(KeyCode.Keypad8))
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            moved = true;
        }

        if (Input.GetKey(KeyCode.Keypad2))
        {
            currentAngle -= rotationSpeed * Time.deltaTime;
            moved = true;
        }

        if (moved)
        {
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        }

        Vector3 angles = transform.localEulerAngles;
        angles.x = currentAngle;
        transform.localEulerAngles = angles;

        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Vector3 spawnPos = firePoint.position + firePoint.forward * 1f;

        GameObject ball = Instantiate(cannonballPrefab, spawnPos, firePoint.rotation);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.velocity = firePoint.forward * 20f;
        }

        Collider ballCol = ball.GetComponent<Collider>();
        Collider cannonCol = GetComponentInParent<Collider>();

        if (ballCol != null && cannonCol != null)
        {
            Physics.IgnoreCollision(ballCol, cannonCol);
        }
    }
}