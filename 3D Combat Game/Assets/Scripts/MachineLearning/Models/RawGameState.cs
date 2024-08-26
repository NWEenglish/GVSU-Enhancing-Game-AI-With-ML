using System;
using System.Collections.Generic;
using Assets.Scripts.Enums;

namespace Assets.Scripts.MachineLearning.Models
{
    [Serializable]
    public class RawGameState : BaseGameState
    {
        public List<BotState> BotStates = new List<BotState>();

        [Serializable]
        public class BotState
        {
            public int LogGroup;
            public TeamType BotsTeam;
            public LogEventType LogEvent;
            public int BotsTeamsScore;
            public int EnemyTeamScore;
            public int TargetPost;
            public int Reward;
            public List<CommandPostState> CommandPostRelativeState = new List<CommandPostState>();
        }

        [Serializable]
        public class CommandPostState
        {
            public int PostNumber;
            public TeamType ControllingTeam;
            public float DistanceToBot;
            public float AverageDistanceFromBotsTeam;
        }
    }

    [Serializable]
    public class RawGameState_Master : BaseGameState
    {
        public List<GameState> States = new List<GameState>();

        [Serializable]
        public class GameState
        {
            public string State; // EENCC => E = Enemy, N = Neutral, C = Controlled
            public int Reward;
            public int BotsTeamsScore;
            public int EnemyTeamScore;
        }
    }
}
