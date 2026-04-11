using System.Collections;
using TMPro;
using UnityEngine;

public class StageAnnouncementUI : MonoBehaviour
{
    [Header("Referências UI")]
    public GameObject announcementPanel;
    public CanvasGroup panelCanvasGroup;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI messageText;

    [Header("Áudio")]
    public AudioSource audioSource;
    public AudioClip announcementSfx;

    [Header("Tempos")]
    public float levelIntroDuration = 3f;
    public float waveIntroDuration = 2.2f;
    public float finalMessageDuration = 4f;

    [Header("Animação")]
    public float fadeDuration = 0.35f;
    public float scalePunchDuration = 0.25f;
    public float startScaleMultiplier = 1.15f;

    private Coroutine currentRoutine;

    void Start()
    {
        HidePanelInstant();
    }

    public void ShowLevelIntro(int levelNumber)
    {
        ShowAnnouncement(
            "LEVEL " + levelNumber,
            "",
            "Defeat all enemies to keep the beanstalk growing.",
            levelIntroDuration
        );
    }

    public void ShowWaveIntro(int waveNumber)
    {
        ShowAnnouncement(
            "",
            "WAVE " + waveNumber,
            "Get ready for the next wave of monsters.",
            waveIntroDuration
        );
    }

    public void ShowFinalObjective()
    {
        ShowAnnouncement(
            "",
            "",
            "Climb the beanstalk.",
            finalMessageDuration
        );
    }

    public void ShowAnnouncement(string level, string wave, string message, float duration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AnnouncementRoutine(level, wave, message, duration));
    }

    IEnumerator AnnouncementRoutine(string level, string wave, string message, float duration)
    {
        announcementPanel.SetActive(true);

        levelText.gameObject.SetActive(!string.IsNullOrEmpty(level));
        waveText.gameObject.SetActive(!string.IsNullOrEmpty(wave));

        levelText.text = level;
        waveText.text = wave;
        messageText.text = message;

        panelCanvasGroup.alpha = 0f;

        levelText.transform.localScale = Vector3.one * startScaleMultiplier;
        waveText.transform.localScale = Vector3.one * startScaleMultiplier;
        messageText.transform.localScale = Vector3.one * startScaleMultiplier;

        PlayAnnouncementSound();

        yield return StartCoroutine(FadeCanvasGroup(panelCanvasGroup, 0f, 1f, fadeDuration));
        yield return StartCoroutine(ScaleTextBounce());

        float visibleTime = duration - (fadeDuration * 2f);
        if (visibleTime < 0.1f)
            visibleTime = 0.1f;

        yield return new WaitForSeconds(visibleTime);

        yield return StartCoroutine(FadeCanvasGroup(panelCanvasGroup, 1f, 0f, fadeDuration));

        announcementPanel.SetActive(false);
        currentRoutine = null;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float time)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    IEnumerator ScaleTextBounce()
    {
        float elapsed = 0f;

        Vector3 startScale = Vector3.one * startScaleMultiplier;
        Vector3 endScale = Vector3.one;

        while (elapsed < scalePunchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scalePunchDuration;

            Vector3 currentScale = Vector3.Lerp(startScale, endScale, t);

            if (levelText.gameObject.activeSelf)
                levelText.transform.localScale = currentScale;

            if (waveText.gameObject.activeSelf)
                waveText.transform.localScale = currentScale;

            messageText.transform.localScale = currentScale;

            yield return null;
        }

        if (levelText.gameObject.activeSelf)
            levelText.transform.localScale = Vector3.one;

        if (waveText.gameObject.activeSelf)
            waveText.transform.localScale = Vector3.one;

        messageText.transform.localScale = Vector3.one;
    }

    void PlayAnnouncementSound()
    {
        if (audioSource != null && announcementSfx != null)
        {
            audioSource.PlayOneShot(announcementSfx);
        }
    }

    public void HidePanelInstant()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        panelCanvasGroup.alpha = 0f;
        announcementPanel.SetActive(false);
        currentRoutine = null;
    }
}