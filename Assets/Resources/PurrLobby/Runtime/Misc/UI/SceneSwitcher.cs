using PurrNet;
using PurrNet.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrLobby
{
    public class SceneSwitcher : MonoBehaviour
    {
        [SerializeField] private LobbyManager lobbyManager;

        [PurrScene, SerializeField]
        private string nextScene;

        [SerializeField]
        private bool subscribeToOnAllReady = true;

        [SerializeField]
        private bool markLobbyStarted = false;

        private static bool _hasAlreadySwitched = false;

        private void Start()
        {
            if (subscribeToOnAllReady && lobbyManager != null)
            {
                lobbyManager.OnAllReady.AddListener(SwitchScene);
            }
        }

        private void OnDestroy()
        {
            if (lobbyManager != null)
                lobbyManager.OnAllReady.RemoveListener(SwitchScene);
        }

        public void SwitchScene()
        {
            if (_hasAlreadySwitched)
            {
                PurrLogger.LogWarning(
                    "SwitchScene already called - ignoring duplicate",
                    this
                );

                return;
            }

            _hasAlreadySwitched = true;

            if (string.IsNullOrEmpty(nextScene))
            {
                PurrLogger.LogError(
                    "Next scene name is not set!",
                    this
                );

                return;
            }

            PurrLogger.Log(
                $"Switching to scene: {nextScene}",
                this
            );

            if (markLobbyStarted && lobbyManager != null)
            {
                lobbyManager.SetLobbyStarted();
            }

            SceneManager.LoadSceneAsync(nextScene);
        }
    }
}