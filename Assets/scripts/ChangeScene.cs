using System.Collections; 
using UnityEngine; 
using UnityEngine.SceneManagement; 

public class ChangeScene : MonoBehaviour
{
  public string nomeDaCena;

  public void ChanceSCenes()
  {
	   StartCoroutine(Abrir());
  }

  public void Exit()
  {
	   StartCoroutine(Exits());
	   //esta linha fecha o Play após a morte do Jogador "USO TEMPORARIO"
	   #if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
	   #endif      
  }

  private IEnumerator Abrir()
  {
	   yield return new WaitForSeconds(1.4f);
	   SceneManager.LoadScene(nomeDaCena);
  }

  private IEnumerator Exits()
  {
	   yield return new WaitForSeconds(1.7f);
	   Application.Quit();
  }
}

