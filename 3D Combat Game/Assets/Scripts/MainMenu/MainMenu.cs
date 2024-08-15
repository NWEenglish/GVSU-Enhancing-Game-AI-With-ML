using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Gamemode;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                { MainMenuItem.MainMenu, GameObject.Find(Constants.MainMenu.MainScreen) },
                { MainMenuItem.Controls, GameObject.Find(Constants.MainMenu.Controls) },
                { MainMenuItem.Credits, GameObject.Find(Constants.MainMenu.Credits) },
                { MainMenuItem.ConfigureGame, GameObject.Find(Constants.MainMenu.ConfigureGame) },
            };

            GameSettings = GameObject.Find(Constants.Objects.GameSettings).GetComponent<GameSettings>();
            DontDestroyOnLoad(GameSettings);

            DisableAllScreens();
            ReturnToMainMenu();
        }

        public void StartGame()
        {
            // Get configure game settings
            string redTeamAILevel = GameObject.Find(Constants.MainMenu.RedTeamDropDown)?.GetComponentInChildren<TextMeshProUGUI>()?.text;
            string blueTeamAILevel = GameObject.Find(Constants.MainMenu.BlueTeamDropDown)?.GetComponentInChildren<TextMeshProUGUI>()?.text;
            string playerTeam = GameObject.Find(Constants.MainMenu.PlayerTeamDropDown)?.GetComponentInChildren<TextMeshProUGUI>()?.text;

            GameSettings.Configure(redTeamAILevel, blueTeamAILevel, playerTeam);

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
