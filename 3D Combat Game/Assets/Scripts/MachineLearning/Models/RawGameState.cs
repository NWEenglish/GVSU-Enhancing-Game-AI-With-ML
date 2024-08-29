using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;

namespace Assets.Scripts.MachineLearning.Models
{
    [Serializable]
    public class RawGameState : BaseGameState
    {
        public List<GameState> States = new List<GameState>();

        [Serializable]
        public class GameState
        {
            public int MaxScore;
            public int RedTeamScore;
            public int BlueTeamScore;
            public TeamType Post1;
            public TeamType Post2;
            public TeamType Post3;
            public TeamType Post4;
            public TeamType Post5;

            public GameState(int maxScore, int redTeamScore, int blueTeamScore, Dictionary<int, TeamType> PostTeams)
            {
                MaxScore = maxScore;
                RedTeamScore = redTeamScore;
                BlueTeamScore = blueTeamScore;
                Post1 = PostTeams.ElementAt(0).Value;
                Post2 = PostTeams.ElementAt(1).Value;
                Post3 = PostTeams.ElementAt(2).Value;
                Post4 = PostTeams.ElementAt(3).Value;
                Post5 = PostTeams.ElementAt(4).Value;
            }
        }
    }
}
