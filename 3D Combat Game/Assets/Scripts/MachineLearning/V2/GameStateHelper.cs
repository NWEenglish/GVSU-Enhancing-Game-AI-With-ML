using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;

namespace Assets.Scripts.MachineLearning.V2
{
    public static class GameStateHelper
    {
        public static int GetIndexForPostStates(string stateID)
        {
            return stateID.LastIndexOf("-") + 1;
        }

        public static string GetGameState(int maxPoints, int redTeamPoints, int blueTeamPoints, Dictionary<int, TeamType> postTeams)
        {
            string retGameState = string.Empty;

            string redTeamState = GetTeamPointStateValue(maxPoints, redTeamPoints);
            string blueTeamState = GetTeamPointStateValue(maxPoints, blueTeamPoints);

            string stateOfPosts = string.Empty;

            for (int index = 0; index < postTeams.Count; index++)
            {
                int postNumber = index + 1;
                TeamType postTeam = postTeams[postNumber];
                stateOfPosts = $"{stateOfPosts}{TeamTypeHelper.GetTeamChar(postTeam)}";
            }

            retGameState = $"{redTeamState}-{blueTeamState}-{stateOfPosts}"; // Outputs like "95-20-RRRNB"

            return new string(retGameState.ToArray());
        }

        private static string GetTeamPointStateValue(int maxPoints, int teamPoints)
        {
            decimal percentage = ((decimal)teamPoints / (decimal)maxPoints) * 100m;
            decimal roundedPercentage = ((int)percentage / 5) * 5;
            string retValue = roundedPercentage.ToString();

            return retValue;
        }
    }
}
