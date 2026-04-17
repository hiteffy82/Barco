using UnityEngine;
using UnityEngine.InputSystem;

namespace TrilloBit3sIndieGames
{
    public class PauseManager : MonoBehaviour
    {
        public GameObject pauseUI;

        private bool isPaused = false;

        void Update()
        {
            var gamepad = Gamepad.current;

            bool pausePressed = false;

            // Gamepad (Start)
            if (gamepad != null && gamepad.startButton.wasPressedThisFrame)
                pausePressed = true;

            // Teclado (ESC)
            if (Input.GetKeyDown(KeyCode.P))//Escape
                pausePressed = true;

            if (pausePressed)
                TogglePause();
        }

        void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
                Pause();
            else
                Resume();
        }

        void Pause()
        {
            Time.timeScale = 0f;

            // PAUSA TODOS OS SONS
            AudioListener.pause = true;

            if (pauseUI != null)
                pauseUI.SetActive(true);

            // Cursor.lockState = CursorLockMode.None;
            // Cursor.visible = true;
        }

        void Resume()
        {
            Time.timeScale = 1f;

            // VOLTA OS SONS
            AudioListener.pause = false;

            if (pauseUI != null)
                pauseUI.SetActive(false);

            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }
    }
}