using Assets.Scripts.Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        private GameObject PauseScreen;
        private bool IsGamePaused;

        private void Start()
        {
            PauseScreen = GameObject.Find(MenuScreens.PauseScreen);
            ResumeGame();
        }

        private void Update()
        {
            bool isEscHit = Input.GetKeyDown(KeyCode.Escape);
            if (isEscHit)
            {
                if (IsGamePaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        public void PauseGame()
        {
            UpdatePauseScreen(true);
        }

        public void ResumeGame()
        {
            UpdatePauseScreen(false);
        }

        public void QuitGame()
        {
            SceneManager.LoadScene(Scenes.MainMenu);
        }

        private void UpdatePauseScreen(bool shouldPause)
        {
            IsGamePaused = shouldPause;
            PauseScreen.SetActive(shouldPause);
            Cursor.lockState = shouldPause
                ? CursorLockMode.None
                : CursorLockMode.Locked;
        }
    }
}
