using UnityEngine;

namespace PurrLobby
{
    public class PauseController : MonoBehaviour
    {
        [Header("Pause UI")]
        [SerializeField] private CanvasGroup pausePanel;

        [Header("Settings")]
        [SerializeField] private GameSettingsController settingsController;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnStart = true;

        // Открыто ли меню паузы
        private bool paused;

        // =========================================================
        // START
        // =========================================================

        private void Start()
        {
            ClosePauseMenu();

            if (settingsController != null)
            {
                settingsController.CloseSettings();
            }

            if (lockCursorOnStart)
            {
                LockCursor();
            }
        }

        // =========================================================
        // UPDATE
        // =========================================================

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        // =========================================================
        // TOGGLE PAUSE
        // =========================================================

        public void TogglePause()
        {
            // Если Settings открыты,
            // ESC возвращает в Pause.
            if (settingsController != null &&
                settingsController.IsOpen())
            {
                CloseSettings();
                return;
            }

            if (paused)
            {
                Resume();
            }
            else
            {
                OpenPauseMenu();
            }
        }

        // =========================================================
        // OPEN PAUSE
        // =========================================================

        public void OpenPauseMenu()
        {
            paused = true;

            ShowPauseUI();

            UnlockCursor();

            Debug.Log("PAUSE OPENED");
        }

        // =========================================================
        // CLOSE PAUSE
        // =========================================================

        public void ClosePauseMenu()
        {
            paused = false;

            HidePauseUI();

            Debug.Log("PAUSE CLOSED");
        }

        // =========================================================
        // RESUME
        // =========================================================

        public void Resume()
        {
            Debug.Log("RESUME");

            // Сначала гарантированно снимаем состояние паузы.
            paused = false;

            // Закрываем Settings, если они открыты.
            if (settingsController != null &&
                settingsController.IsOpen())
            {
                settingsController.CloseSettings();
            }

            // Закрываем Pause UI.
            HidePauseUI();

            // Возвращаем управление игроку.
            LockCursor();

            Debug.Log(
                "RESUME COMPLETE. Input blocked = " +
                IsInputBlocked()
            );
        }

        // =========================================================
        // OPEN SETTINGS
        // =========================================================

        public void OpenSettings()
        {
            if (settingsController == null)
            {
                Debug.LogWarning(
                    "PauseController: Settings Controller is not assigned.",
                    this
                );

                return;
            }

            // Pause состояние выключаем,
            // потому что сейчас показываем Settings.
            paused = false;

            // Скрываем Pause.
            HidePauseUI();

            // Показываем Settings.
            settingsController.OpenSettings();

            // Курсор свободный.
            UnlockCursor();

            Debug.Log("SETTINGS OPENED");
        }

        // =========================================================
        // CLOSE SETTINGS
        // =========================================================

        public void CloseSettings()
        {
            if (settingsController != null)
            {
                settingsController.CloseSettings();
            }

            // После Settings возвращаем Pause.
            OpenPauseMenu();
        }

        // =========================================================
        // INPUT BLOCK
        // =========================================================

        public bool IsInputBlocked()
        {
            // Pause открыта.
            if (paused)
                return true;

            // Settings открыты.
            if (settingsController != null &&
                settingsController.IsOpen())
            {
                return true;
            }

            // Ничего не открыто.
            return false;
        }

        // =========================================================
        // IS PAUSED
        // =========================================================

        public bool IsPaused()
        {
            return paused;
        }

        // =========================================================
        // PAUSE UI
        // =========================================================

        private void ShowPauseUI()
        {
            if (pausePanel == null)
                return;

            pausePanel.alpha = 1f;
            pausePanel.interactable = true;
            pausePanel.blocksRaycasts = true;
        }

        private void HidePauseUI()
        {
            if (pausePanel == null)
                return;

            pausePanel.alpha = 0f;
            pausePanel.interactable = false;
            pausePanel.blocksRaycasts = false;
        }

        // =========================================================
        // CURSOR
        // =========================================================

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}