using System;
using System.Collections.Generic;

namespace Assets.Scripts.MachineLearning.Models
{
    [Serializable]
    public class NormalizedGameState : BaseGameState
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
