using System;
using System.Collections.Generic;

namespace Assets.Scripts.MachineLearning.Models
{
    [Serializable]
    public class NormalizedGameState : BaseGameState
    {
        public List<BotState> BotStates = new List<BotState>();

        [Serializable]
        public class BotState
        {
            public int LogGroup;
            public decimal ControllingTeam;
            public decimal EventType;
            public decimal BotsTeamsScore;
            public decimal EnemyTeamScore;
            public int TargetPost;
            public int Reward;
            public List<CommandPostState> CommandPostRelativeState = new List<CommandPostState>();
        }

        [Serializable]
        public class CommandPostState
        {
            public int PostNumber;
            public decimal ControllingTeam;
            public decimal DistanceToBot;
            public decimal AverageDistanceFromBotsTeam;
        }
    }

    [Serializable]
    public class NormalizedGameState_Master : BaseGameState
    {
        public List<GameState> States = new List<GameState>();

        [Serializable]
        public class GameState
        {
            public string State;
            public int Reward;
        }
    }
}
