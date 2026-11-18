using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [SerializeField] Button openButton;
    [SerializeField] GameObject panelRoot;
    [SerializeField] CanvasGroup panelCanvasGroup;
    [SerializeField] RectTransform popupTransform;
    [SerializeField] Toggle vibrationToggle;
    [SerializeField] Image vibrationTrack;
    [SerializeField] RectTransform vibrationKnob;
    [SerializeField] TMP_Text vibrationStateLabel;
    [SerializeField] Toggle soundToggle;
    [SerializeField] Image soundTrack;
    [SerializeField] RectTransform soundKnob;
    [SerializeField] TMP_Text soundStateLabel;
    [SerializeField] Sprite enabledTrackSprite;
    [SerializeField] Sprite disabledTrackSprite;
    [SerializeField] float knobTravel = 80f;
    [SerializeField] float stateLabelOffset = 34f;
    [SerializeField] Button closeButton;
    [SerializeField] Button doneButton;
    [SerializeField] AudioSource musicSource;

    Coroutine transition;
    Vector2 shownPosition;

    void Awake()
    {
        shownPosition = popupTransform.anchoredPosition;
        RefreshToggleValues();

        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        doneButton.onClick.AddListener(Close);
        vibrationToggle.onValueChanged.AddListener(SetVibration);
        soundToggle.onValueChanged.AddListener(SetSound);

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        ApplySound(Services.Settings.SoundOn);
        panelRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(Open);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }

        if (doneButton != null)
        {
            doneButton.onClick.RemoveListener(Close);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.RemoveListener(SetVibration);
        }

        if (soundToggle != null)
        {
            soundToggle.onValueChanged.RemoveListener(SetSound);
        }
    }

    void Open()
    {
        RefreshToggleValues();
        StartTransition(true);
    }

    void Close() => StartTransition(false);

    void SetVibration(bool enabled)
    {
        Services.Settings.VibrationOn = enabled;
        UpdateToggleVisual(vibrationTrack, vibrationKnob, vibrationStateLabel, enabled);

        if (enabled)
        {
            Handheld.Vibrate();
        }
    }

    void SetSound(bool enabled)
    {
        Services.Settings.SoundOn = enabled;
        UpdateToggleVisual(soundTrack, soundKnob, soundStateLabel, enabled);
        ApplySound(enabled);
    }

    void RefreshToggleValues()
    {
        bool vibrationOn = Services.Settings.VibrationOn;
        bool soundOn = Services.Settings.SoundOn;

        vibrationToggle.SetIsOnWithoutNotify(vibrationOn);
        soundToggle.SetIsOnWithoutNotify(soundOn);
        UpdateToggleVisual(vibrationTrack, vibrationKnob, vibrationStateLabel, vibrationOn);
        UpdateToggleVisual(soundTrack, soundKnob, soundStateLabel, soundOn);
    }

    void UpdateToggleVisual(Image track, RectTransform knob, TMP_Text stateLabel, bool enabled)
    {
        track.sprite = enabled ? enabledTrackSprite : disabledTrackSprite;
        track.color = Color.white;
        knob.anchoredPosition = new Vector2(enabled ? knobTravel : -knobTravel, 0f);
        stateLabel.rectTransform.anchoredPosition = new Vector2(enabled ? -stateLabelOffset : stateLabelOffset, 0f);
        stateLabel.text = enabled ? "ON" : "OFF";
    }

    void StartTransition(bool opening)
    {
        if (!opening && !panelRoot.activeSelf)
        {
            return;
        }

        if (transition != null)
        {
            StopCoroutine(transition);
        }

        if (opening)
        {
            if (!panelRoot.activeSelf)
            {
                panelCanvasGroup.alpha = 0f;
                popupTransform.anchoredPosition = shownPosition + Vector2.down * 32f;
                panelRoot.SetActive(true);
            }

            panelRoot.transform.SetAsLastSibling();
        }

        panelCanvasGroup.blocksRaycasts = true;
        panelCanvasGroup.interactable = true;
        transition = StartCoroutine(AnimatePanel(opening));
    }

    IEnumerator AnimatePanel(bool opening)
    {
        const float duration = 0.18f;
        float elapsed = 0f;
        float startAlpha = panelCanvasGroup.alpha;
        float targetAlpha = opening ? 1f : 0f;
        Vector2 startPosition = popupTransform.anchoredPosition;
        Vector2 targetPosition = opening ? shownPosition : shownPosition + Vector2.down * 32f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            popupTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        panelCanvasGroup.alpha = targetAlpha;
        popupTransform.anchoredPosition = targetPosition;
        transition = null;

        if (!opening)
        {
            panelRoot.SetActive(false);
        }
    }

    void ApplySound(bool enabled)
    {
        if (!enabled || musicSource.clip == null)
        {
            musicSource.Stop();
            return;
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
}
