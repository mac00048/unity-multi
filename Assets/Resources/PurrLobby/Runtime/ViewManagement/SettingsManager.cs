using UnityEngine;

namespace PurrLobby
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        // =========================================================
        // PLAYER PREFS KEYS
        // =========================================================

        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";
        private const string FULLSCREEN_KEY = "Fullscreen";
        private const string POSTERIZATION_KEY = "Posterization";


        // =========================================================
        // DEFAULT VALUES
        // =========================================================

        private const float DEFAULT_MUSIC_VOLUME = 1f;
        private const float DEFAULT_SFX_VOLUME = 1f;

        private const bool DEFAULT_FULLSCREEN = true;
        private const bool DEFAULT_POSTERIZATION = false;


        // =========================================================
        // CURRENT VALUES
        // =========================================================

        public float MusicVolume { get; private set; }
        public float SFXVolume { get; private set; }

        public bool Fullscreen { get; private set; }
        public bool Posterization { get; private set; }


        // =========================================================
        // START
        // =========================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            LoadSettings();
        }


        // =========================================================
        // MUSIC
        // =========================================================

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);

            PlayerPrefs.SetFloat(
                MUSIC_VOLUME_KEY,
                MusicVolume
            );

            PlayerPrefs.Save();

            ApplyMusicVolume();
        }


        // =========================================================
        // SFX
        // =========================================================

        public void SetSFXVolume(float value)
        {
            SFXVolume = Mathf.Clamp01(value);

            PlayerPrefs.SetFloat(
                SFX_VOLUME_KEY,
                SFXVolume
            );

            PlayerPrefs.Save();
        }


        // =========================================================
        // FULLSCREEN
        // =========================================================

        public void SetFullscreen(bool value)
        {
            Fullscreen = value;

            PlayerPrefs.SetInt(
                FULLSCREEN_KEY,
                value ? 1 : 0
            );

            PlayerPrefs.Save();

            Screen.fullScreen = value;
        }


        // =========================================================
        // POSTERIZATION
        // =========================================================

        public void SetPosterization(bool value)
        {
            Posterization = value;

            PlayerPrefs.SetInt(
                POSTERIZATION_KEY,
                value ? 1 : 0
            );

            PlayerPrefs.Save();

            // Эффект пока НЕ применяем.
            // Позже здесь подключим PosterizationManager.
        }


        // =========================================================
        // LOAD
        // =========================================================

        private void LoadSettings()
        {
            MusicVolume = PlayerPrefs.GetFloat(
                MUSIC_VOLUME_KEY,
                DEFAULT_MUSIC_VOLUME
            );

            SFXVolume = PlayerPrefs.GetFloat(
                SFX_VOLUME_KEY,
                DEFAULT_SFX_VOLUME
            );

            Fullscreen =
                PlayerPrefs.GetInt(
                    FULLSCREEN_KEY,
                    DEFAULT_FULLSCREEN ? 1 : 0
                ) == 1;

            Posterization =
                PlayerPrefs.GetInt(
                    POSTERIZATION_KEY,
                    DEFAULT_POSTERIZATION ? 1 : 0
                ) == 1;

            ApplySettings();
        }


        // =========================================================
        // APPLY ALL
        // =========================================================

        private void ApplySettings()
        {
            ApplyMusicVolume();

            Screen.fullScreen = Fullscreen;

            // Posterization пока ничего не делает.
            // Подключим позже.
        }


        // =========================================================
        // MUSIC APPLICATION
        // =========================================================

        private void ApplyMusicVolume()
        {
            /*
             * Пока здесь ничего не делаем.
             *
             * Позже подключим сюда AudioManager,
             * который будет управлять всей музыкой игры.
             */
        }


        // =========================================================
        // RESET
        // =========================================================

        public void ResetSettings()
        {
            MusicVolume = DEFAULT_MUSIC_VOLUME;
            SFXVolume = DEFAULT_SFX_VOLUME;

            Fullscreen = DEFAULT_FULLSCREEN;
            Posterization = DEFAULT_POSTERIZATION;

            PlayerPrefs.SetFloat(
                MUSIC_VOLUME_KEY,
                MusicVolume
            );

            PlayerPrefs.SetFloat(
                SFX_VOLUME_KEY,
                SFXVolume
            );

            PlayerPrefs.SetInt(
                FULLSCREEN_KEY,
                Fullscreen ? 1 : 0
            );

            PlayerPrefs.SetInt(
                POSTERIZATION_KEY,
                Posterization ? 1 : 0
            );

            PlayerPrefs.Save();

            ApplySettings();
        }
    }
}