using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneSceneManager : MonoBehaviour
{
    [Header("Cena")]
    [Tooltip("Nome da próxima cena (igual ao Build Settings)")]
    public string nextSceneName;

    [Tooltip("Atraso opcional em segundos antes de carregar a próxima cena")]
    public float loadDelay = 0f;

    [Header("Vídeo")]
    public VideoPlayer videoPlayer;
    [Tooltip("Se marcado, dá Play no vídeo ao iniciar a cena")]
    public bool autoPlay = true;

    [Header("Skip")]
    [Tooltip("Botão de pular cutscene (opcional)")]
    public Button skipButton;
    [Tooltip("Tecla para pular (opcional)")]
    public KeyCode skipKey = KeyCode.Space;
    public bool allowSkip = true;

    private bool isLoading = false;

    void Awake()
    {
        if (videoPlayer != null)
        {
            // Quando o vídeo termina, esse evento dispara
            videoPlayer.loopPointReached += OnVideoEnded;
            videoPlayer.errorReceived += OnVideoError;
        }

        if (skipButton != null)
            skipButton.onClick.AddListener(Skip);
    }

    void Start()
    {
        if (autoPlay && videoPlayer != null)
        {
            // Garante que o vídeo está pronto antes de tocar (evita tela preta)
            if (!videoPlayer.isPrepared)
            {
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += OnPreparedThenPlay;
            }
            else
            {
                videoPlayer.Play();
                // Se o áudio do vídeo sair por AudioSource separado, também dar Play nele
                // (quando VideoPlayer.audioOutputMode = AudioSource e target configurado)
            }
        }
    }

    void Update()
    {
        if (allowSkip && Input.GetKeyDown(skipKey))
            Skip();
    }

    private void OnPreparedThenPlay(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPreparedThenPlay;
        vp.Play();
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        LoadNextScene();
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"VideoPlayer error: {message}");
        // Se quiser, ainda assim vai para a próxima cena:
        LoadNextScene();
    }

    public void Skip()
    {
        if (!allowSkip) return;
        // Para o vídeo (evita fantasma de áudio)
        if (videoPlayer != null)
        {
            if (videoPlayer.isPlaying) videoPlayer.Stop();
        }
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isLoading) return;
        isLoading = true;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("CutsceneSceneManager: 'nextSceneName' não definido.");
            return;
        }

        if (loadDelay > 0f)
            Invoke(nameof(LoadNow), loadDelay);
        else
            LoadNow();
    }

    private void LoadNow()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnded;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.prepareCompleted -= OnPreparedThenPlay;
        }
        if (skipButton != null)
            skipButton.onClick.RemoveListener(Skip);
    }
}
