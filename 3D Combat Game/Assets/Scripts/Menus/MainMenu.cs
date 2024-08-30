using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Gamemode;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        private Dictionary<MainMenuItem, GameObject> MenuItems;
        private GameObject CurrentActiveScreen;
        private GameSettings GameSettings;

        private void Start()
        {
            MenuItems = new Dictionary<MainMenuItem, GameObject>()
            {
                { MainMenuItem.MainMenu, GameObject.Find(Constants.MenuScreens.MainScreen) },
                { MainMenuItem.Controls, GameObject.Find(Constants.MenuScreens.Controls) },
                { MainMenuItem.Credits, GameObject.Find(Constants.MenuScreens.Credits) },
                { MainMenuItem.ConfigureGame, GameObject.Find(Constants.MenuScreens.ConfigureGame) },
            };

            GameSettings = GameObject.Find(Constants.Objects.GameSettings).GetComponent<GameSettings>();
            DontDestroyOnLoad(GameSettings);

            DisableAllScreens();
            ReturnToMainMenu();
            Cursor.lockState = CursorLockMode.None;
        }

        public void StartGame()
        {
            // Get configure game settings
            string redTeamAILevel = GameObject.Find(Constants.MenuScreens.RedTeamDropDown)?.GetComponentInChildren<TextMeshProUGUI>()?.text;
            string blueTeamAILevel = GameObject.Find(Constants.MenuScreens.BlueTeamDropDown)?.GetComponentInChildren<TextMeshProUGUI>()?.text;
            string playerTeam = GameObject.Find(Constants.MenuScreens.PlayerTeamDropDown)?.GetComponentInChildren<TextMeshProUGUI>()?.text;
            bool isNonStopMode = GameObject.Find(Constants.MenuScreens.NonStopModeToggle)?.GetComponentInChildren<Toggle>()?.isOn ?? false;

            GameSettings.Configure(redTeamAILevel, blueTeamAILevel, playerTeam, isNonStopMode);

            // Launch game
            SceneManager.LoadScene(Constants.Scenes.ConqustGameMode);
        }

        public void ShowConfigureGame()
        {
            EnableScreen(MenuItems.GetValueOrDefault(MainMenuItem.ConfigureGame));
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
