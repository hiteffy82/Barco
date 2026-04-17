using UnityEngine;

namespace TrilloBit3sIndieGames
{
    [RequireComponent(typeof(Rigidbody))]
    public class FlutuadorDeAgua : MonoBehaviour
    {
        public float AirDrag = 1f;
        public float WaterDrag = 4f;
        public Transform[] FloatPoints;

        public float buoyancyMultiplier = 12f; 

        private Rigidbody rb;
        private WaterEffect water;
        private Vector3[] waterLinePoints;
        private Vector3 centerOffset;

        void Update() { if (Time.timeScale == 0f) return; }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            water = FindObjectOfType<WaterEffect>();

            if (FloatPoints != null && FloatPoints.Length > 0)
            {
                waterLinePoints = new Vector3[FloatPoints.Length];
                for (int i = 0; i < FloatPoints.Length; i++)
                    waterLinePoints[i] = FloatPoints[i].position;
                centerOffset = FisicaDoBarquinho.GetCenter(waterLinePoints) - transform.position;
            }
            else
            {
                waterLinePoints = new Vector3[0];
                centerOffset = Vector3.zero;
                Debug.LogWarning($"[{name}] FlutuadorDeAgua: nenhum FloatPoint atribuído.");
            }
        }

        void FixedUpdate()
        {
            if (FloatPoints == null || FloatPoints.Length == 0) return;
            if (water == null) water = FindObjectOfType<WaterEffect>();
            if (water == null) return;

            bool anyUnder = false;
            for (int i = 0; i < FloatPoints.Length; i++)
            {
                if (FloatPoints[i] == null) continue;
                Vector3 fpWorld = FloatPoints[i].position;
                float waterH = water.GetHeight(fpWorld);
                waterLinePoints[i] = new Vector3(fpWorld.x, waterH, fpWorld.z);

                float depth = waterH - fpWorld.y;
                if (depth > 0f)
                {
                    float depthForce = Mathf.Clamp(depth, 0f, 2f);
  
                    Vector3 force = Vector3.up * depthForce * rb.mass * 9.81f * buoyancyMultiplier / FloatPoints.Length;

                    rb.AddForceAtPosition(force, fpWorld, ForceMode.Force);
                    anyUnder = true;
                }
            }

            rb.drag = anyUnder ? WaterDrag : AirDrag;

            // align up to water normal for nicer behavior
            if (waterLinePoints.Length >= 3)
            {
                Vector3 targetUp = FisicaDoBarquinho.GetNormal(waterLinePoints);
                Quaternion q = Quaternion.FromToRotation(transform.up, targetUp);
                Vector3 torque = Vector3.Cross(transform.up, targetUp);
                rb.AddTorque(torque * 5f, ForceMode.Force);

            }
        }

        private void OnDrawGizmos()
        {
            if (FloatPoints == null) return;
            if (water == null) water = FindObjectOfType<WaterEffect>();

            for (int i = 0; i < FloatPoints.Length; i++)
            {
                if (FloatPoints[i] == null) continue;
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(FloatPoints[i].position, 0.08f);

                if (water != null)
                {
                    Vector3 p = FloatPoints[i].position;
                    p.y = water.GetHeight(p);
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(p, Vector3.one * 0.12f);
                }
            }
        }
    }
}