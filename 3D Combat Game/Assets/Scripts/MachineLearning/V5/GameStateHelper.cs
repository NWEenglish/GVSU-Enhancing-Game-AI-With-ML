using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.MachineLearning.Models;

namespace Assets.Scripts.MachineLearning.V5
{
    public static class GameStateHelper
    {
        public static int Version => 5;

        public static List<int> DetermineStateDifferences(string currentState, string proposedState, TeamType team = TeamType.Neutral)
        {
            List<int> postsChanged = new List<int>();

            int startingPostIndex = proposedState.LastIndexOf("-") + 1;

            // Skip the first two since those are the percentiles
            for (int i = startingPostIndex; i < proposedState.Length; i++)
            {
                // Only grab posts rquired for the next state
                if (proposedState.ElementAtOrDefault(i) == TeamTypeHelper.GetTeamChar(team))
                {
                    int postNumber = i - startingPostIndex + 1;
                    postsChanged.Add(postNumber);
                }
            }

            return postsChanged;
        }

        public static List<string> GetOrderedStates(Algorithms algorithm, TeamType team, string currentStateID, int numberOfSmartBots)
        {
            List<LearnedGameState> nextStates = algorithm.GetNextStates(team, currentStateID);

            int numberOfPositiveNextStates = nextStates.Count(state => state.Value > 0);


            //// Either get the top 20% of states, or grab the top 5 best case states.
            //int numberOfStatesToGrab = numberOfPositiveNextStates > 0
            //    ? numberOfPositiveNextStates / 5
            //    : 5;

            // Order states by value, and grab enough for 1 per bot
            return nextStates
                .OrderByDescending(state => state.Value)
                //.Where(state => state.Value > 0) // Take all that lead to a positive result
                .Select(state => state.StateID)
                //.Take(numberOfStatesToGrab); // Only take 20% of the top states
                //.Take(1); // Future V6? - Might use take 1 instead of one per bot.. we add a weight based on value of state, but we don't have the info here
                .Take(numberOfSmartBots) // This idea was essentially bagged voting. Using weight would be a weighted bag voting. Maybe keep this - it's a "good-ish" concept. Maybe we just need to fix pathing
                .ToList();
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
