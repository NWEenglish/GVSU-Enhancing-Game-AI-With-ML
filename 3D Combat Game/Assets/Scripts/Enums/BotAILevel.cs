using System;

namespace Assets.Scripts.Enums
{
    public enum BotAILevel
    {
        BasicAI,
        SmartAI
    }

    public static class BotAILevelHelper
    {
        public static BotAILevel GetBotAILevel(string name)
        {
            BotAILevel retAILevel;

            if (name == "Basic AI")
            {
                retAILevel = BotAILevel.BasicAI;
            }
            else if (name == "Smart AI")
            {
                retAILevel = BotAILevel.SmartAI;
            }
            else
            {
                throw new ArgumentException(nameof(name));
            }

            return retAILevel;
        }
    }
}
