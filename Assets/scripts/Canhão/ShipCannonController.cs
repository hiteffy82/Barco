// Gerenciar canhoes para que possa usar um de cada lado
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

namespace TrilloBit3sIndieGames
{
    public class ShipCannonController : MonoBehaviour
    {
        public ShipCannon leftCannon;
        public ShipCannon rightCannon;

        private ShipCannon activeCannon;

        //Atualizar UI para troca de canhar
        [SerializeField] private TextMeshProUGUI globalAmmoText;
        [SerializeField] private Image[] globalAmmoIcons;

        [SerializeField] private TextMeshProUGUI cannonText;

        void Start()
        {
            activeCannon = rightCannon; // padrão
            SetActiveCannon(activeCannon);
        }

        // evitando Keyboard Ghosting / Key Rollover Limitation
        void Update()
        {
            if (Time.timeScale == 0f) return;

            var gamepad = Gamepad.current;

            float inputVertical = 0f;
            bool shootPressed = false;

            // GAMEPAD
            if (gamepad != null)
            {
                if (gamepad.dpad.left.wasPressedThisFrame)
                    SetActiveCannon(leftCannon);

                if (gamepad.dpad.right.wasPressedThisFrame)
                    SetActiveCannon(rightCannon);

                if (gamepad.dpad.up.isPressed)
                    inputVertical = 1f;

                if (gamepad.dpad.down.isPressed)
                    inputVertical = -1f;

                if (gamepad.buttonWest.wasPressedThisFrame)
                    shootPressed = true;
            }

            // TECLADO (NOVO MAPEAMENTO)
            // Movimento do canhão (I / K)
            if (Input.GetKey(KeyCode.I))
                inputVertical = 1f;

            if (Input.GetKey(KeyCode.K))
                inputVertical = -1f;

            // Disparo (Space)
            if (Input.GetKeyDown(KeyCode.Space))
                shootPressed = true;

            // Trocar canhão (J / L)
            if (Input.GetKeyDown(KeyCode.J))
                SetActiveCannon(leftCannon);

            if (Input.GetKeyDown(KeyCode.L))
                SetActiveCannon(rightCannon);

            // ENVIO FINAL
            if (activeCannon != null)
                activeCannon.HandleInput(inputVertical, shootPressed);
        }

        void SetActiveCannon(ShipCannon cannon)
        {
            activeCannon = cannon;

            leftCannon.SetActive(false);
            rightCannon.SetActive(false);

            activeCannon.SetActive(true);

            UpdateGlobalUI(); // sincroniza UI com o canhão atual
        }

        void UpdateGlobalUI()
        {
            if (activeCannon == null) return;

            // MUNIÇÃO
            if (globalAmmoText != null)
                globalAmmoText.text = "Munição: " + activeCannon.currentAmmo;

            for (int i = 0; i < globalAmmoIcons.Length; i++)
            {
                if (globalAmmoIcons[i] != null)
                {
                    Color c = globalAmmoIcons[i].color;
                    c.a = (i < activeCannon.currentAmmo) ? 1f : 0.2f;
                    globalAmmoIcons[i].color = c;
                }
            }

            // CANHÃO ATIVO
            if (cannonText != null)
            {
                if (activeCannon == leftCannon)
                    cannonText.text = "Canhão da Esquerda";
                else if (activeCannon == rightCannon)
                    cannonText.text = "Canhão da Direita";
            }
        }
    }
}