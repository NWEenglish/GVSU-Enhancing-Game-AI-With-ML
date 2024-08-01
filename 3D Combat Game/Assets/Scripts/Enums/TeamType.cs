using System.Collections.Generic;

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
    }
}
