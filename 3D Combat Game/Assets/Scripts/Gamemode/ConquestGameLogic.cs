using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Constants;
using Assets.Scripts.Entities;
using Assets.Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Gamemode.Conquest
{
    public class ConquestGameLogic : MonoBehaviour
    {
        public Dictionary<TeamType, int> TeamPoints { get; private set; } = new Dictionary<TeamType, int>();
        public List<CommandPostLogic> CommandPosts { get; private set; } = new List<CommandPostLogic>();

        public int MaxPointsPerTeam => 500;

        private const int PointsPerTick = 1;
        private const float TickLengthSec = 2.5f;
        private const float GameOverProcessLength = 20f;

        private float LastTimePointsDistributed;
        private GameObject GameOverPanel;
        private float? GameOverProcessStart;
        private GameSettings GameSettings;
        private bool HasSavedData = false;

        private bool IsRunningGameOverProcess => GameOverProcessStart != null;
        public bool IsGameOver => TeamPoints.Any(kvp => kvp.Value >= MaxPointsPerTeam);

        private void Start()
        {
            foreach (TeamType team in TeamTypeHelper.GetPlayableTeams())
            {
                TeamPoints.Add(team, 0);
            }

            CommandPosts = Resources.FindObjectsOfTypeAll<CommandPostLogic>().ToList();
            LastTimePointsDistributed = Time.time;

            GameOverPanel = GameObject.Find(HUD.GameOverScreen);
            GameOverPanel.SetActive(false);

            GameSettings = GameObject.Find(Objects.GameSettings).GetComponent<GameSettings>();
        }

        private void FixedUpdate()
        {
            if (!IsRunningGameOverProcess && !IsGameOver)
            {
                UpdatePoints();
            }
            else if (IsGameOver)
            {
                if (!IsRunningGameOverProcess)
                {
                    StartGameOverProcess();
                }
                else
                {
                    ContinueGameOverProcess();
                }
            }
        }

        public TeamType? GetWinningTeam()
        {
            TeamType? winningTeam = null;

            List<TeamType> winningTeams = TeamPoints
                .Where(kvp => kvp.Value >= MaxPointsPerTeam)
                .Select(kvp => kvp.Key)
                .ToList();

            if (winningTeams.Count == 1)
            {
                winningTeam = winningTeams.First();
            }

            return winningTeam;
        }

        private void StartGameOverProcess()
        {
            GameOverProcessStart = Time.time;
            GameOverPanel.SetActive(true);
            var gameOverTextMesh = GameObject.Find(HUD.GameOverMessage).GetComponent<TextMeshProUGUI>();

            List<TeamType> winningTeams = TeamPoints
                .Where(kvp => kvp.Value >= MaxPointsPerTeam)
                .Select(kvp => kvp.Key)
                .ToList();

            // Tie
            if (winningTeams.Count() > 1)
            {
                gameOverTextMesh.text = "Game Ended in a Tie";
            }
            // One winning team
            else if (winningTeams.Count() == 1)
            {
                TeamType winningTeam = winningTeams.First();
                string winnningTeamName = TeamTypeHelper.GetSimpleTeamName(winningTeam);
                gameOverTextMesh.text = $"{winnningTeamName} Team Won";
            }

            // Play music based on what team the player was on
            Player player = GameObject.Find(Objects.Player).GetComponent<Player>();
            bool isPlayerWinner = winningTeams.Contains(player.Team);
            player.NotifyOfGameResult(isPlayerWinner);
        }

        private void ContinueGameOverProcess()
        {
            bool shouldEndGame = Time.time >= GameOverProcessStart + GameOverProcessLength;

            if (shouldEndGame)
            {
                if (!HasSavedData)
                {
                    GameSettings.StartDataNormalization();
                    HasSavedData = true;
                }
                else
                {
                    SceneManager.LoadScene(Scenes.MainMenu);
                }
            }
        }

        private void UpdatePoints()
        {
            if (LastTimePointsDistributed + TickLengthSec <= Time.time)
            {
                CommandPosts
                    .Where(post => TeamTypeHelper.GetPlayableTeams().Contains(post.ControllingTeam))
                    .ToList()
                    .ForEach(post => TeamPoints[post.ControllingTeam] = CalculateUpdatedPoints(TeamPoints[post.ControllingTeam]));

                LastTimePointsDistributed = Time.time;
            }
        }

        private int CalculateUpdatedPoints(int currentPoints)
        {
            int retPoints = currentPoints + PointsPerTick;
            retPoints = retPoints > MaxPointsPerTeam
                ? MaxPointsPerTeam
                : retPoints;

            return retPoints;
        }
    }
}
