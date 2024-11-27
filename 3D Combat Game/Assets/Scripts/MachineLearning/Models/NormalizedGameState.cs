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
            public string StateID;
            public double Value;
        }

        public List<LearnedGameState> ToLearnedGameStates()
        {
            List<LearnedGameState> retLearnedGameStates = new List<LearnedGameState>();
            States.ForEach(state => retLearnedGameStates.Add(new LearnedGameState(state.StateID, state.Value)));
            return retLearnedGameStates;
        }
    }
}
