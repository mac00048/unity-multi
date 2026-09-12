using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomUI : MonoBehaviour
{
    public static WaitingRoomUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Button startButton;

    private void Awake()
    {
        Instance = this;

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (startButton != null)
            startButton.gameObject.SetActive(true);
    }

    public void SetPlayers(int current, int max)
    {
        if (playersText == null)
            return;

        playersText.text = $"Players: {current} / {max}";
    }

    public void SetCountdown(float value)
    {
        if (countdownText == null)
            return;

        if (value <= 0f)
        {
            countdownText.gameObject.SetActive(false);
            return;
        }

        countdownText.gameObject.SetActive(true);
        countdownText.text = Mathf.CeilToInt(value).ToString();
    }
}