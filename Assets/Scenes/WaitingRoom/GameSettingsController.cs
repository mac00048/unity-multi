using UnityEngine;

namespace PurrLobby
{
    public class GameSettingsController : MonoBehaviour
    {
        [Header("Settings UI")]
        [SerializeField] private CanvasGroup settingsPanel;

        private bool isOpen;

        private void Start()
        {
            CloseSettings();
        }

        public void OpenSettings()
        {
            if (settingsPanel == null)
                return;

            isOpen = true;

            settingsPanel.alpha = 1f;
            settingsPanel.interactable = true;
            settingsPanel.blocksRaycasts = true;
        }

        public void CloseSettings()
        {
            if (settingsPanel == null)
                return;

            isOpen = false;

            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
        }

        public void ToggleSettings()
        {
            if (isOpen)
                CloseSettings();
            else
                OpenSettings();
        }

        public bool IsOpen()
        {
            return isOpen;
        }
    }
}