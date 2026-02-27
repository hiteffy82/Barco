using UnityEngine;

namespace TrilloBit3sIndieGames
{
    [RequireComponent(typeof(MeshFilter))]
    public class WaterEffect : MonoBehaviour
    {
        //-----------------------------------------------------------------------------
        // Ajustar buoyancyMultiplier do FlutuadorDeAgua caso alterar valor aqui
        public float waveSpeed = 3;//1.0f
        public float waveHeight = 3;//0.5f
        public float waveFrequency = 1.0f;
        //-----------------------------------------------------------------------------

        private MeshFilter meshFilter;
        private Mesh waterMesh;
        private Vector3[] originalVertices;

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();

            if (meshFilter == null)
            {
                Debug.LogError("WaterEffect precisa de MeshFilter!");
                return;
            }

            // Usar sharedMesh para NÃO clonar mesh infinito
            waterMesh = meshFilter.sharedMesh;

            if (waterMesh == null)
            {
                Debug.LogError("sharedMesh está NULL!");
                return;
            }

            // Copia original dos vértices
            originalVertices = waterMesh.vertices;
        }

        void Update()
        {
            if (waterMesh == null || originalVertices == null || originalVertices.Length == 0) 
                return;
        }

        void FixedUpdate()
        {
            UpdateWaterMesh(Time.fixedTime);
        }

        void UpdateWaterMesh(float time)
        {
            Vector3[] verts = waterMesh.vertices;

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 v = originalVertices[i];
                v.y = Mathf.Sin(time * waveFrequency + v.x * 0.1f + v.z * 0.1f + waveSpeed) * waveHeight;
                verts[i] = v;
            }

            waterMesh.vertices = verts;
            waterMesh.RecalculateNormals();
        }

        // Pega altura real do vértice mais próximo naquele ponto
        public float GetHeight(Vector3 worldPos)
        {
            if (waterMesh == null)
                return transform.position.y;

            Vector3 local = transform.InverseTransformPoint(worldPos);
            Vector3[] verts = waterMesh.vertices;

            float bestY = 0f;
            float bestDist = float.MaxValue;

            for (int i = 0; i < verts.Length; i++)
            {
                float dx = verts[i].x - local.x;
                float dz = verts[i].z - local.z;
                float d = dx * dx + dz * dz;
                if (d < bestDist)
                {
                    bestDist = d;
                    bestY = verts[i].y;
                }
            }

            return transform.TransformPoint(new Vector3(0, bestY, 0)).y;
        }
    }
}