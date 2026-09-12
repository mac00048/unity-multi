using UnityEngine;
using UnityEngine.UI;

namespace PurrLobby
{
    public class SettingsView : View
    {
        [Header("Sliders")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Toggles")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle posterizationToggle;

        public override void OnShow()
        {
            LoadUI();
        }

        private void LoadUI()
        {
            if (SettingsManager.Instance == null)
                return;

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(
                    SettingsManager.Instance.MusicVolume
                );
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(
                    SettingsManager.Instance.SFXVolume
                );
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.SetIsOnWithoutNotify(
                    SettingsManager.Instance.Fullscreen
                );
            }

            if (posterizationToggle != null)
            {
                posterizationToggle.SetIsOnWithoutNotify(
                    SettingsManager.Instance.Posterization
                );
            }
        }

        public void OnMusicVolumeChanged(float value)
        {
            if (SettingsManager.Instance == null)
                return;

            SettingsManager.Instance.SetMusicVolume(value);
        }

        public void OnSFXVolumeChanged(float value)
        {
            if (SettingsManager.Instance == null)
                return;

            SettingsManager.Instance.SetSFXVolume(value);
        }

        public void OnFullscreenChanged(bool value)
        {
            if (SettingsManager.Instance == null)
                return;

            SettingsManager.Instance.SetFullscreen(value);
        }

        public void OnPosterizationChanged(bool value)
        {
            if (SettingsManager.Instance == null)
                return;

            SettingsManager.Instance.SetPosterization(value);
        }

        public void Back()
        {
            ViewManager viewManager =
                FindFirstObjectByType<ViewManager>();

            if (viewManager != null)
            {
                viewManager.OnSettingsBackClicked();
            }
        }
    }
}