using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;

namespace Assets.Scripts.MachineLearning.Helpers
{
    public static class GameStateHelper
    {
        public static string GetGameState(int maxPoints, int redTeamPoints, int blueTeamPoints, Dictionary<int, TeamType> postTeams)
        {
            var retGameState = new List<char>
            {
                GetTeamPointStateValue(maxPoints, redTeamPoints),
                GetTeamPointStateValue(maxPoints, blueTeamPoints)
            };

            for (int index = 0; index < postTeams.Count; index++)
            {
                int postNumber = index + 1;
                TeamType postTeam = postTeams[postNumber];
                retGameState.Add(TeamTypeHelper.GetTeamChar(postTeam));
            }

            return new string(retGameState.ToArray());
        }

        private static char GetTeamPointStateValue(int maxPoints, int teamPoints)
        {
            char retValue = '*'; // * => 100% or winner

            if (teamPoints < maxPoints)
            {
                decimal percentile = (decimal)teamPoints / (decimal)maxPoints;
                retValue = (percentile * 10m).ToString().First();
            }

            return retValue;
        }
    }
}
