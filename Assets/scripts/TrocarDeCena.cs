using System.Collections; // Necessário para coroutines
using UnityEngine; //funções básicas da Unity
using UnityEngine.SceneManagement; 
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TrilloBit3sIndieGames
{
    public class TrocarDeCena : MonoBehaviour
    {
        [Header("Configuração da Cena")]
        [Tooltip("Nome da cena que será carregada")]
        public string sceneName;

        [Header("Tempos de Espera")]
        [Tooltip("Tempo antes de trocar de cena")]
        public float loadDelay = 1.4f;

        [Tooltip("Tempo antes de sair do jogo")]
        public float quitDelay = 1.7f;

        [Header("UI")]
        [Tooltip("Painel de créditos que será ativado/desativado")]
        public GameObject creditsPanel;

        [Header("UI Navegação")]
        public Button firstSelectedButton;

        private bool isCreditsOpen = false;

        public Health health;
        public TrocarDeCena trocarDeCena;

        private bool morreu = false;

        public PauseManager pauseManager;

        void Start()
        {
            if (health != null)
            {
                health.OnDeath += OnPlayerDeath;
            }
        }
          
        void Update()
        {
            var gamepad = Gamepad.current;

            //if (gamepad == null) return;

            // Controle
            if (gamepad != null && gamepad.selectButton.wasPressedThisFrame)
            {
                ToggleCredits();
            }

            // Teclado (opcional)
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCredits();
            }
        }

        // Inicia o processo de troca de cena
        public void LoadScene()
        {
            StartCoroutine(LoadSceneCoroutine());
        }

        // Inicia o processo de saída do jogo
        public void QuitGame()
        {
            StartCoroutine(QuitGameCoroutine());

            // Para o jogo no editor da Unity (não funciona em build)
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        // Alterna a exibição do painel de créditos
        public void ToggleCredits()
        {
            isCreditsOpen = !isCreditsOpen;

            creditsPanel.SetActive(isCreditsOpen);

            if (isCreditsOpen)
            {
                // Cursor.lockState = CursorLockMode.None;
                // Cursor.visible = true;

                // seleciona botão automaticamente
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
            }
            else
            {
                // Cursor.lockState = CursorLockMode.Locked;
                // Cursor.visible = false;

                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        // Coroutine responsável por carregar a cena com atraso
        private IEnumerator LoadSceneCoroutine()
        {
            // Aguarda o tempo definido
            yield return new WaitForSecondsRealtime(loadDelay);

            // Carrega a cena informada
            SceneManager.LoadScene(sceneName);
        }

        // Coroutine responsável por sair do jogo com atraso
        private IEnumerator QuitGameCoroutine()
        {
            // Aguarda o tempo definido
            yield return new WaitForSeconds(quitDelay);

            // Fecha o jogo (só funciona no build)
            Application.Quit();
        }

        void OnDestroy()
        {
            if (health != null)
                health.OnDeath -= OnPlayerDeath;
        }

        void OnPlayerDeath()
        {
            if (morreu) return;
            morreu = true;
            LoadScene();
        }

         void TestDeath()
        {
            GetComponent<Health>().TakeDamage(999);
        }
    }
}