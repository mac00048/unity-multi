
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RespawnConsoleUI : MonoBehaviour
{
    public static RespawnConsoleUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup console;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private TMP_Text linePrefab;

    [Header("Typing")]
    [SerializeField] private float characterDelay = 0.02f;

    [Header("Scroll")]
    [SerializeField] private float scrollDelay = 0.02f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Hide();
    }

    // =========================================================
    // SHOW
    // =========================================================

    public void Show()
    {
        if (console == null)
            return;

        console.alpha = 1f;
        console.interactable = false;
        console.blocksRaycasts = false;
    }

    // =========================================================
    // HIDE
    // =========================================================

    public void Hide()
    {
        if (console == null)
            return;

        console.alpha = 0f;
        console.interactable = false;
        console.blocksRaycasts = false;
    }

    // =========================================================
    // CLEAR
    // =========================================================

    public void Clear()
    {
        if (content == null)
            return;

        StopAllCoroutines();

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        StartCoroutine(RefreshLayout());
    }

    // =========================================================
    // ADD LINE
    // =========================================================

    public void AddLine(string text)
    {
        if (content == null)
        {
            Debug.LogWarning(
                "RespawnConsoleUI: Content is not assigned.",
                this
            );

            return;
        }

        if (linePrefab == null)
        {
            Debug.LogWarning(
                "RespawnConsoleUI: Line Prefab is not assigned.",
                this
            );

            return;
        }

        TMP_Text line = Instantiate(
            linePrefab,
            content
        );

        line.text = text;

        // Сначала показываем пустую строку,
        // затем печатаем её посимвольно.
        line.text = "";

        StartCoroutine(
            TypeLine(line, text)
        );
    }

    // =========================================================
    // TYPE LINE
    // =========================================================

    private IEnumerator TypeLine(
        TMP_Text line,
        string text)
    {
        if (line == null)
            yield break;

        foreach (char character in text)
        {
            if (line == null)
                yield break;

            line.text += character;

            // Обновляем Layout после каждого символа,
            // чтобы ScrollRect видел изменение размера.
            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }

            yield return new WaitForSeconds(
                characterDelay
            );
        }

        yield return StartCoroutine(
            ScrollToBottom()
        );
    }

    // =========================================================
    // SCROLL TO BOTTOM
    // =========================================================

    private IEnumerator ScrollToBottom()
    {
        if (scrollRect == null)
            yield break;

        yield return null;

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 0f;

        yield return new WaitForSeconds(
            scrollDelay
        );

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 0f;
    }

    // =========================================================
    // REFRESH LAYOUT
    // =========================================================

    private IEnumerator RefreshLayout()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}

