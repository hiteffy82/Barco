using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace TrilloBit3sIndieGames
{
    public class Seletor : MonoBehaviour
    {
        public enum MenuAction
        {
            Restart,
            Options,
            Quit
        }

        [System.Serializable]
        public class MenuButton
        {
            public Button button;
            public TextMeshProUGUI buttonText;
            public MenuAction action;
        }

        [Header("Menu")]
        public MenuButton[] menuButtons;

        [Header("Config")]
        public string cenaRestart;
        public float delayLoad = 1.4f;

        private int selectedOption = 0;
        private bool isLoading = false;

        private float axisCooldown = 0.25f;
        private float axisTimer = 0f;

        private void Start()
        {
            if (menuButtons == null || menuButtons.Length == 0)
            {
                Debug.LogWarning("GameOverMenu iniciado sem botões configurados.");
                enabled = false;
                return;
            }

            SetupButtons();
            UpdateButtonHighlight();
        }

        private void Update()
        {
            if (isLoading) return;

            axisTimer -= Time.unscaledDeltaTime;

            HandleNavigation();
            HandleConfirm();
        }

        // ----------------- SETUP -----------------
        private void SetupButtons()
        {
            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i] == null ||
                    menuButtons[i].button == null ||
                    menuButtons[i].buttonText == null)
                {
                    Debug.LogError($"MenuButtons[{i}] está mal configurado.");
                    continue;
                }

                int index = i;

                // Click (mouse / touch)
                menuButtons[i].button.onClick.AddListener(() =>
                {
                    selectedOption = index;
                    UpdateButtonHighlight();
                    ActivateSelectedButton();
                });

                // Hover (mouse)
                EventTrigger trigger = menuButtons[i].button.GetComponent<EventTrigger>();
                if (trigger == null)
                    trigger = menuButtons[i].button.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };

                entry.callback.AddListener((_) =>
                {
                    selectedOption = index;
                    UpdateButtonHighlight();
                });

                trigger.triggers.Add(entry);
            }
        }

        // ----------------- INPUT -----------------
        private void HandleNavigation()
        {
            if (axisTimer > 0f) return;

            if (Input.GetKeyDown(KeyCode.UpArrow))
                MoveSelection(-1);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                MoveSelection(1);

            float vertical = Input.GetAxis("Vertical");
            if (vertical > 0.5f)
                MoveSelection(-1);
            else if (vertical < -0.5f)
                MoveSelection(1);
        }

        private void MoveSelection(int direction)
        {
            axisTimer = axisCooldown;

            selectedOption = Mathf.Clamp(
                selectedOption + direction,
                0,
                menuButtons.Length - 1
            );

            UpdateButtonHighlight();

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(
                    menuButtons[selectedOption].button.gameObject
                );
            }
        }

        private void HandleConfirm()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                ActivateSelectedButton();

            for (int i = 0; i <= 19; i++)
            {
                if (Input.GetKeyDown("joystick button " + i))
                {
                    ActivateSelectedButton();
                    break;
                }
            }
        }

        // ----------------- VISUAL -----------------
        private void UpdateButtonHighlight()
        {
            for (int i = 0; i < menuButtons.Length; i++)
            {
                Color c = menuButtons[i].buttonText.color;
                c.a = (i == selectedOption) ? 1f : 0.5f;
                menuButtons[i].buttonText.color = c;
            }
        }

        // ----------------- ACTIONS -----------------
        private void ActivateSelectedButton()
        {
            if (isLoading) return;
            isLoading = true;

            switch (menuButtons[selectedOption].action)
            {
                case MenuAction.Restart:
                    StartCoroutine(RestartScene());
                    break;

                case MenuAction.Options:
                    OpenOptions();
                    break;

                case MenuAction.Quit:
                    StartCoroutine(QuitGame());
                    break;
            }
        }

        private void OpenOptions()
        {
            isLoading = false;
           // Debug.Log("Abrir menu de opções");
          // Aqui você ativa outro Canvas ou estado
        }

        private IEnumerator RestartScene()
        {
            yield return new WaitForSecondsRealtime(delayLoad);
            SceneManager.LoadScene(cenaRestart);
        }

        private IEnumerator QuitGame()
        {
            yield return new WaitForSecondsRealtime(delayLoad);
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
