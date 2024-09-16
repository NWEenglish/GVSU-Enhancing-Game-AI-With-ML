using System.Linq;
using System.Text.RegularExpressions;

namespace Assets.Scripts.MachineLearning.Models
{
    public class LearnedGameState
    {
        public int RedTeamPointsPercentile { get; set; }
        public int BlueTeamPointsPercentile { get; set; }
        public string PostStates { get; set; }
        public string StateID { get; set; }
        public double Value { get; set; }

        public LearnedGameState(string stateID, double stateValue)
        {
            string regex = @"(\d+)-(\d+)-(\w+)";
            var match = Regex.Match(stateID, regex);

            // Regex index for groups starts at 1 (0 is groups combined)
            RedTeamPointsPercentile = int.Parse(match.Groups.ElementAt(1).Value);
            BlueTeamPointsPercentile = int.Parse(match.Groups.ElementAt(2).Value);
            PostStates = match.Groups.ElementAt(3).Value;

            StateID = stateID;
            Value = stateValue;
        }
    }
}
