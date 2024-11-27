using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.MachineLearning.Models;

namespace Assets.Scripts.MachineLearning.V3
{
    public static class GameStateHelper
    {
        public static int Version => 3;

        public static List<int> DetermineStateDifferences(string currentState, string proposedState)
        {
            List<int> postsChanged = new List<int>();

            int startingPostIndex = proposedState.LastIndexOf("-") + 1;

            // Skip the first two since those are the percentiles
            for (int i = startingPostIndex; i < proposedState.Length; i++)
            {
                if (currentState.ElementAtOrDefault(i) != proposedState.ElementAtOrDefault(i))
                {
                    postsChanged.Add(i);
                }
            }

            return postsChanged;
        }

        public static IEnumerable<string> GetOrderedStates(Algorithms algorithm, TeamType team, string currentStateID, int numberOfSmartBots)
        {
            List<NormalizedGameState.GameState> nextStates = algorithm.GetNextStates(team, currentStateID);

            // Order states by value, and grab enough for 1 per bot
            return nextStates
                .OrderByDescending(state => state.Value)
                .Select(state => state.StateID)
                .Take(numberOfSmartBots);
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
