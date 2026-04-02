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

        void LateUpdate()
        {
            Vector3 direcao = alvo.position - transform.position;

            Quaternion rotCompleta = Quaternion.LookRotation(direcao);

            Vector3 angulos = rotCompleta.eulerAngles;

            transform.rotation = Quaternion.Euler(0f, angulos.y + 180f, 0f);
        }
    }
}


// usar com material
// using UnityEngine;

// namespace TrilloBit3sIndieGames
// {
//     public class Bilboard_Obj : MonoBehaviour
//     {
//         public Transform alvo; // normalmente a câmera

//         void Start()
//         {
//             if (alvo == null)
//                 alvo = Camera.main.transform;
//         }

//         void LateUpdate()
//         {
//             // direção do alvo
//             Vector3 direcao = alvo.position - transform.position;

//             // calcula rotação olhando para o alvo
//             Quaternion rotCompleta = Quaternion.LookRotation(direcao);

//             // mantém apenas o ângulo em Y
//             Vector3 angulos = rotCompleta.eulerAngles;
//             transform.rotation = Quaternion.Euler(0f, angulos.y, 0f);
//         }
//     }
// }
