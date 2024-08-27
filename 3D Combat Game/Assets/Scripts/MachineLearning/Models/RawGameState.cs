using System;
using System.Collections.Generic;

namespace Assets.Scripts.MachineLearning.Models
{
    [Serializable]
    public class RawGameState : BaseGameState
    {
        public List<GameState> States = new List<GameState>();

        [Serializable]
        public class GameState
        {
            public string State;
            public int Reward;

            public GameState(string stete)
            {
                State = stete;
                Reward = 0;
            }
        }
    }
}
