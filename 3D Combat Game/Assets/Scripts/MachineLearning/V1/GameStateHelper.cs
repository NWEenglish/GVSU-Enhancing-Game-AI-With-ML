using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.MachineLearning.Models;

namespace Assets.Scripts.MachineLearning.V1
{
    public static class GameStateHelper
    {
        public static int GetIndexForPostStates(string stateID)
        {
            return 2;
        }

        public static IEnumerable<string> GetOrderedStates(Algorithms algorithm, TeamType team, string currentStateID, int numberOfSmartBots)
        {
            List<NormalizedGameState.GameState> nextStates = algorithm.GetNextStates(team, currentStateID);

            // Order states by order of magnitude, and grab enough for 1 per bot
            return nextStates
                .OrderByDescending(state => Math.Abs(state.Value))
                .Select(state => state.StateID)
                .Take(numberOfSmartBots);
        }

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

            return retValue; // Outputs like "*5RNBBB"
        }
    }
}
