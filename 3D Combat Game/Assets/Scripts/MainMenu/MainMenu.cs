using System.Collections.Generic;
using Assets.Scripts.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        private Dictionary<MainMenuItem, GameObject> MenuItems;
        private GameObject CurrentActiveScreen;

        private void Start()
        {
            MenuItems = new Dictionary<MainMenuItem, GameObject>()
            {
                { MainMenuItem.MainMenu, GameObject.Find(Constants.MainMenu.MainScreen) },
                { MainMenuItem.Controls, GameObject.Find(Constants.MainMenu.Controls) },
                { MainMenuItem.Credits, GameObject.Find(Constants.MainMenu.Credits) },
            };

            DisableAllScreens();
            ReturnToMainMenu();
        }

        //public void StartGame()
        //{
        //    SceneManager.LoadScene(Scenes.ConqustGameMode);
        //}

        public void ShowStartGameOptions()
        {
            // TODO
        }

        public void ShowControls()
        {
            EnableScreen(MenuItems.GetValueOrDefault(MainMenuItem.Controls));
        }

        public void ShowCredits()
        {
            EnableScreen(MenuItems.GetValueOrDefault(MainMenuItem.Credits));
        }

        public void ReturnToMainMenu()
        {
            EnableScreen(MenuItems.GetValueOrDefault(MainMenuItem.MainMenu));
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif

            Application.Quit();
        }

        private void DisableAllScreens()
        {
            foreach (var item in MenuItems.Values)
            {
                item.SetActive(false);
            }
        }

        private void EnableScreen(GameObject screenHolder)
        {
            if (CurrentActiveScreen != null)
            {
                CurrentActiveScreen.SetActive(false);
            }

            CurrentActiveScreen = screenHolder;
            CurrentActiveScreen.SetActive(true);
        }
    }
}
