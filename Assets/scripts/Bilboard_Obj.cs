using UnityEngine;

namespace TrilloBit3sIndieGames
{
    public class Bilboard_Obj : MonoBehaviour
    {
        public Transform alvo;

        void Start()
        {
            if (alvo == null)
                alvo = Camera.main.transform;
        }
        
        void Update() { if (Time.timeScale == 0f) return; }

        void LateUpdate()
        {
            Vector3 direcao = alvo.position - transform.position;

            Quaternion rotCompleta = Quaternion.LookRotation(direcao);

            Vector3 angulos = rotCompleta.eulerAngles;

            transform.rotation = Quaternion.Euler(0f, angulos.y + 180f, 0f);
        }
    }
}