using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    // Carrega uma cena pelo nome
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Nome da cena está vazio.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // Recarrega a cena atual
    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // Sai do jogo
    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}