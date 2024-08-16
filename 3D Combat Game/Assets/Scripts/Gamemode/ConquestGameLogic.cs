using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Constants;
using Assets.Scripts.Enums;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Gamemode.Conquest
{
    public class ConquestGameLogic : MonoBehaviour
    {
        public Dictionary<TeamType, int> TeamPoints { get; private set; } = new Dictionary<TeamType, int>();
        public List<CommandPostLogic> CommandPosts { get; private set; } = new List<CommandPostLogic>();

        private const int PointsPerTick = 1;
        private const int MaxPointsPerTeam = 500;
        private const float TickLengthSec = 2.5f;

        private float LastTimePointsDistributed;
        private GameObject GameOverPanel;

        private bool IsGameOver => TeamPoints.Any(kvp => kvp.Value >= MaxPointsPerTeam);

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
        }

        private void FixedUpdate()
        {
            if (!IsGameOver)
            {
                UpdatePoints();
            }
            else
            {
                HandleGameOver();
            }
        }

        private void HandleGameOver()
        {
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
