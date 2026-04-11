using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject pauseMenuUI;               // painel do pause
    [SerializeField] private string nomeCenaMenu = "Menu";

    [Header("Opções")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused;
    private CanvasGroup pauseCG;
    private EventSystem evt;

    void Awake()
    {
        if (pauseMenuUI)
        {
            pauseCG = pauseMenuUI.GetComponent<CanvasGroup>();
            if (!pauseCG) pauseCG = pauseMenuUI.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        EnsureEventSystem();
        HidePausePanel();
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ---------- Botão: CONTINUAR ----------
    public void ResumeGame()
    {
        isPaused = false;
        HidePausePanel();
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // ---------- Botão: MENU PRINCIPAL ----------
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(nomeCenaMenu);
    }

    // ---------- Botão: SAIR ----------
    public void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("Jogo encerrado.");
    }

    // ---------- Lógica de Pause ----------
    private void PauseGame()
    {
        isPaused = true;
        ShowPausePanel();
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void ShowPausePanel()
    {
        if (!pauseMenuUI) return;
        pauseMenuUI.SetActive(true);
        if (pauseCG)
        {
            pauseCG.alpha = 1f;
            pauseCG.interactable = true;
            pauseCG.blocksRaycasts = true;
        }
    }

    private void HidePausePanel()
    {
        if (!pauseMenuUI) return;
        if (pauseCG)
        {
            pauseCG.alpha = 0f;
            pauseCG.interactable = false;
            pauseCG.blocksRaycasts = false;
        }
        pauseMenuUI.SetActive(false);
    }

    private void EnsureEventSystem()
    {
        evt = EventSystem.current;
        if (!evt)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
            evt = go.GetComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (!evt.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>())
        {
            evt.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
#else
        if (!evt.GetComponent<StandaloneInputModule>())
        {
            evt.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }
}
