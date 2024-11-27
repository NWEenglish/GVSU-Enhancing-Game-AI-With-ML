using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Enums
{
    public enum TeamType
    {
        BlueTeam,
        RedTeam,
        Neutral
    }

    public static class TeamTypeHelper
    {
        private static List<TeamType> PlayableTeams = new List<TeamType>()
        {
            TeamType.BlueTeam,
            TeamType.RedTeam
        };

        public static IEnumerable<TeamType> GetPlayableTeams()
        {
            return PlayableTeams;
        }

        public static string GetSimpleTeamName(TeamType team)
        {
            string retName = team switch
            {
                TeamType.BlueTeam => "Blue",
                TeamType.RedTeam => "Red",
                _ => throw new ArgumentException(nameof(team))
            };

            return retName;
        }

        public static TeamType GetTeamType(string teamName)
        {
            TeamType retTeamType;

            if (teamName == "None")
            {
                retTeamType = TeamType.Neutral;
            }
            else if (teamName == "Red Team")
            {
                retTeamType = TeamType.RedTeam;
            }
            else if (teamName == "Blue Team")
            {
                retTeamType = TeamType.BlueTeam;
            }
            else
            {
                throw new ArgumentException(nameof(teamName));
            }

            return retTeamType;
        }

        public static char GetTeamChar(TeamType team)
        {
            return team.ToString().ToUpper().First();
        }

        public static TeamType GetEnemyTeam(TeamType currentTeam)
        {
            return currentTeam == TeamType.RedTeam
                ? TeamType.BlueTeam
                : TeamType.RedTeam;
        }
    }
}
