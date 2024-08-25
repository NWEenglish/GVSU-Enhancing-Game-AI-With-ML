using System;
using System.Collections.Generic;

namespace Assets.Scripts.MachineLearning.Models
{
    [Serializable]
    public class NormalizedGameState
    {
        public List<BotState> BotStates = new List<BotState>();

        [Serializable]
        public class BotState
        {
            public int LogGroup;
            public float ControllingTeam;
            public float EventType;
            public float BotsTeamsScore;
            public float EnemyTeamScore;
            public int TargetPost;
            public List<CommandPostState> CommandPostRelativeState = new List<CommandPostState>();
        }

        [Serializable]
        public class CommandPostState
        {
            public int PostNumber;
            public float ControllingTeam;
            public float DistanceToBot;
            public float AverageDistanceFromBotsTeam;
        }
    }
}
