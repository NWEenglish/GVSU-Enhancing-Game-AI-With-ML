using System;
using Assets.Scripts.Enums;
using Assets.Scripts.Extensions;
using UnityEngine;

namespace Assets.Scripts.Gamemode
{
    public class GameSettings : MonoBehaviour
    {
        public BotAILevel? RedTeamAILevel { get; private set; }
        public BotAILevel? BlueTeamAILevel { get; private set; }
        public TeamType? PlayersTeam { get; private set; }

        public void Configure(string redTeamAILevel, string blueTeamAILevel, string playersTeam)
        {
            redTeamAILevel.ThrowIfNullOrEmpty(nameof(redTeamAILevel));
            blueTeamAILevel.ThrowIfNullOrEmpty(nameof(blueTeamAILevel));
            playersTeam.ThrowIfNullOrEmpty(nameof(playersTeam));

            RedTeamAILevel = BotAILevelHelper.GetBotAILevel(redTeamAILevel);
            BlueTeamAILevel = BotAILevelHelper.GetBotAILevel(blueTeamAILevel);
            PlayersTeam = TeamTypeHelper.GetTeamType(playersTeam);
        }
    }
}
