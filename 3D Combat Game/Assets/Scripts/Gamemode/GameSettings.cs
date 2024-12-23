using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.Extensions;
using Assets.Scripts.MachineLearning.V5;
using UnityEngine;

namespace Assets.Scripts.Gamemode
{
    public class GameSettings : MonoBehaviour
    {
        public TeamType? PlayersTeam { get; private set; }
        public Dictionary<TeamType, BotAILevel> TeamAILevels { get; private set; } = new Dictionary<TeamType, BotAILevel>();

        private bool IsNonStopMode = false;
        private int GamesPlayed = 0;
        private int MaxGames => 1000;

        private DataNormalization DataNormalization = new DataNormalization();
        private Algorithms MLAlgorithms = new Algorithms();

        public void Configure(string redTeamAILevel, string blueTeamAILevel, string playersTeam, bool isNonStopMode)
        {
            redTeamAILevel.ThrowIfNullOrEmpty(nameof(redTeamAILevel));
            blueTeamAILevel.ThrowIfNullOrEmpty(nameof(blueTeamAILevel));
            playersTeam.ThrowIfNullOrEmpty(nameof(playersTeam));

            TeamAILevels = new Dictionary<TeamType, BotAILevel>()
            {
                { TeamType.RedTeam, BotAILevelHelper.GetBotAILevel(redTeamAILevel) },
                { TeamType.BlueTeam, BotAILevelHelper.GetBotAILevel(blueTeamAILevel) }
            };

            PlayersTeam = TeamTypeHelper.GetTeamType(playersTeam);
            IsNonStopMode = isNonStopMode;
        }

        public void StartDataNormalization()
        {
            GamesPlayed++;
            DataNormalization.StartProcess();
            TeamAILevels
                .Where(kvp => kvp.Value == BotAILevel.SmartAI)
                .ToList()
                .ForEach(kvp => MLAlgorithms.StartSaveProcess(kvp.Key));
        }

        public bool ShouldPlayAnotherGame()
        {
            return IsNonStopMode && GamesPlayed <= MaxGames;
        }
    }
}
