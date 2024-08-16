using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Extensions;
using UnityEngine;

namespace Assets.Scripts.Gamemode
{
    public class GameSettings : MonoBehaviour
    {
        public TeamType? PlayersTeam { get; private set; }
        public Dictionary<TeamType, BotAILevel> TeamAILevels { get; private set; } = new Dictionary<TeamType, BotAILevel>();

        public void Configure(string redTeamAILevel, string blueTeamAILevel, string playersTeam)
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
        }
    }
}
