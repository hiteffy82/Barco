using UnityEngine;

namespace TrilloBit3sIndieGames
{
    public class RotateObject : MonoBehaviour
    {
        public float rotationSpeed = 30.0f; // Velocidade de rotação em graus por segundo
        public Vector3 rotationAxis = Vector3.up; // Eixo de rotação padrão (Y)

        void Update()
        {
            // Calcule a rotação com base na velocidade e no eixo escolhido
            float rotationAngle = rotationSpeed * Time.deltaTime;

            // Aplique a rotação ao objeto em torno do eixo selecionado
            transform.Rotate(rotationAxis, rotationAngle);
        }
    }
}