using System.Collections;
using TMPro;
using UnityEngine;

public class GrizeldaBoot : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup bootScreen;
    [SerializeField] private TMP_Text introText;
    [SerializeField] private RectTransform terminalContent;
    [SerializeField] private TMP_Text linePrefab;

    [Header("Intro")]
    [SerializeField] private string developerName = "GRIZELDA STUDIOS";
    [SerializeField] private float introTime = 2f;

    [Header("Text")]
    [SerializeField] private float characterDelay = 0.02f;
    [SerializeField] private float fastMultiplier = 5f;
    [SerializeField] private float lineDelay = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioClip startupSound;
    [SerializeField] private AudioClip ambience;
    [SerializeField] private AudioClip lineSound;

    [SerializeField] private float ambienceVolume = 0.5f;

    [Header("Fade")]
    [SerializeField] private float fadeTime = 0.5f;

    private AudioSource ambienceSource;
    private AudioSource soundSource;

    private readonly string[] bootLines =
    {
        "> GRIZELDA CORE BOOT SEQUENCE",
        "> ----------------------------",

        "[SYSTEM] Initializing tactical interface...",
        "[SYSTEM] Communication systems........ OK",
        "[SYSTEM] Network connection........... OK",
        "[SYSTEM] Personnel database........... LOADED",
        "[SYSTEM] Combat protocol.............. LOADED",
        "[SYSTEM] Battlefield data............. LOADED",

        "[CORE] Mounting GRIZELDA CORE........ OK",
        "[CORE] Checking integrity............ OK",

        "ACCESS LEVEL: ARENA FIGHTER",
        "NETWORK STATUS: SECURE",
        "AUTHENTICATION: VERIFIED",

        "> Running diagnostics...",

        "MEMORY........................ NOMINAL",
        "CPU........................... NOMINAL",
        "NETWORK....................... NOMINAL",
        "CORE.......................... NOMINAL",

        "> Finalizing GRIZELDA CORE..."
    };

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Audio Sources
        ambienceSource = gameObject.AddComponent<AudioSource>();
        soundSource = gameObject.AddComponent<AudioSource>();

        ambienceSource.loop = true;
        ambienceSource.playOnAwake = false;
        ambienceSource.volume = ambienceVolume;

        soundSource.playOnAwake = false;

        // Boot screen
        bootScreen.alpha = 1f;
        bootScreen.interactable = true;
        bootScreen.blocksRaycasts = true;

        // Intro hidden
        if (introText != null)
        {
            introText.alpha = 0f;
        }

        // Start boot
        StartCoroutine(BootSequence());
    }

    // =========================================================
    // BOOT SEQUENCE
    // =========================================================

    private IEnumerator BootSequence()
    {
        // =====================================================
        // 1. STARTUP SOUND
        // =====================================================

        if (startupSound != null)
        {
            soundSource.PlayOneShot(startupSound);

            // Ждём окончания звука запуска
            yield return new WaitForSeconds(
                startupSound.length
            );
        }


        // =====================================================
        // 2. AMBIENCE
        // =====================================================

        if (ambience != null)
        {
            ambienceSource.clip = ambience;
            ambienceSource.volume = ambienceVolume;
            ambienceSource.Play();
        }


        // =====================================================
        // 3. DEVELOPER INTRO
        // =====================================================

        if (introText != null)
        {
            introText.text =
                "A GAME BY\n\n" +
                developerName;

            introText.alpha = 1f;

            yield return new WaitForSeconds(
                introTime
            );

            introText.alpha = 0f;
        }


        // =====================================================
        // 4. TERMINAL
        // =====================================================

        foreach (string line in bootLines)
        {
            yield return StartCoroutine(
                PrintLine(line)
            );

            yield return new WaitForSeconds(
                lineDelay
            );
        }


        // =====================================================
        // 5. FINAL MESSAGE
        // =====================================================

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(
            PrintLine(
                "GRIZELDA BASE INITIALIZED....100%"
            )
        );

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(
            PrintLine(
                "> SYSTEM READY."
            )
        );


        // =====================================================
        // 6. FADE AUDIO
        // =====================================================

        yield return StartCoroutine(
            FadeAudio()
        );


        // =====================================================
        // 7. FADE SCREEN
        // =====================================================

        yield return StartCoroutine(
            FadeScreen()
        );

        gameObject.SetActive(false);
    }

    // =========================================================
    // PRINT LINE
    // =========================================================

    private IEnumerator PrintLine(string text)
    {
        TMP_Text line = Instantiate(
            linePrefab,
            terminalContent
        );

        line.text = "";

        foreach (char c in text)
        {
            line.text += c;

            float delay = characterDelay;

            // Space ускоряет только печать текста
            if (Input.GetKey(KeyCode.Space))
            {
                delay /= fastMultiplier;
            }

            yield return new WaitForSeconds(
                delay
            );
        }

        // Звук после завершения строки
        if (lineSound != null)
        {
            soundSource.PlayOneShot(lineSound);
        }
    }

    // =========================================================
    // FADE AUDIO
    // =========================================================

    private IEnumerator FadeAudio()
    {
        if (ambienceSource == null)
            yield break;

        float startVolume =
            ambienceSource.volume;

        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeTime
                );

            ambienceSource.volume =
                Mathf.Lerp(
                    startVolume,
                    0f,
                    t
                );

            yield return null;
        }

        ambienceSource.volume = 0f;
        ambienceSource.Stop();
    }

    // =========================================================
    // FADE SCREEN
    // =========================================================

    private IEnumerator FadeScreen()
    {
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeTime
                );

            bootScreen.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    t
                );

            yield return null;
        }

        bootScreen.alpha = 0f;

        bootScreen.interactable = false;
        bootScreen.blocksRaycasts = false;
    }
}