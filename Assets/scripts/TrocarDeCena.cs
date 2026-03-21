using System.Collections; // Necessário para coroutines
using UnityEngine; //funções básicas da Unity
using UnityEngine.SceneManagement; // Gerenciamento de cenas

// Classe responsável por trocar de cena e sair do jogo
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

    private bool isCreditsOpen = false;

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

        // Ativa ou desativa o painel
        creditsPanel.SetActive(isCreditsOpen);
    }

    // Coroutine responsável por carregar a cena com atraso
    private IEnumerator LoadSceneCoroutine()
    {
        // Aguarda o tempo definido
        yield return new WaitForSeconds(loadDelay);

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
}


// using System.Collections; // Necessário para coroutines
// using UnityEngine; //funções básicas da Unity
// using UnityEngine.SceneManagement; // Gerenciamento de cenas

// // Classe responsável por trocar de cena e sair do jogo
// public class TrocarDeCena : MonoBehaviour
// {
//     [Header("Configuração da Cena")]
//     [Tooltip("Nome da cena que será carregada")]
//     public string sceneName;

//     [Header("Tempos de Espera")]
//     [Tooltip("Tempo antes de trocar de cena")]
//     public float loadDelay = 1.4f;

//     [Tooltip("Tempo antes de sair do jogo")]
//     public float quitDelay = 1.7f;

//     // Inicia o processo de troca de cena
//     public void LoadScene()
//     {
//         StartCoroutine(LoadSceneCoroutine());
//     }

//     // Inicia o processo de saída do jogo
//     public void QuitGame()
//     {
//         StartCoroutine(QuitGameCoroutine());

//         // Para o jogo no editor da Unity (não funciona em build)
//         #if UNITY_EDITOR
//             UnityEditor.EditorApplication.isPlaying = false;
//         #endif
//     }

//     // Coroutine responsável por carregar a cena com atraso
//     private IEnumerator LoadSceneCoroutine()
//     {
//         // Aguarda o tempo definido
//         yield return new WaitForSeconds(loadDelay);

//         // Carrega a cena informada
//         SceneManager.LoadScene(sceneName);
//     }

//     // Coroutine responsável por sair do jogo com atraso
//     private IEnumerator QuitGameCoroutine()
//     {
//         // Aguarda o tempo definido
//         yield return new WaitForSeconds(quitDelay);

//         // Fecha o jogo (só funciona no build)
//         Application.Quit();
//     }
// }