using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Enums;
using Assets.Scripts.Extensions;
using Assets.Scripts.Gamemode.Conquest;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class MasterSmartBotLogic : MonoBehaviour
    {
        private ConquestGameLogic GameLogic;
        private List<CommandPostLogic> PostLogicList = new List<CommandPostLogic>();
        private Dictionary<CommandPostLogic, TeamType> LastKnownControl = new Dictionary<CommandPostLogic, TeamType>();

        private List<SmartBot> SmartBots = new List<SmartBot>();

        private int NumberOfLogs = 0;
        private float LastLogTime = 0f;
        private float TimeBetweenLogs = 20f;
        private bool HasSavedRecords = false;

        private GameState StateOfGame = new GameState();

        private string OutputDirectory = @"D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\Raw Data";

        private void Start()
        {
            GameLogic = GameObject.FindObjectOfType<ConquestGameLogic>();
        }

        private void Update()
        {
            // Due to timing issues, we might not have this list populated on startup
            if (!PostLogicList.Any())
            {
                PostLogicList = GameLogic.CommandPosts;
            }

            if (!HasSavedRecords)
            {
                if (NumberOfLogs == 0 || GameLogic.IsGameOver || Time.time >= LastLogTime + TimeBetweenLogs)
                {
                    UpdateState(LogEventType.TimerTriggered);
                }
                else if (HasPostControlChanged())
                {
                    UpdateState(LogEventType.CommandPostChange);
                }

                if (GameLogic.IsGameOver)
                {
                    // Save records
                    string json = JsonUtility.ToJson(StateOfGame);

                    var writer = File.CreateText($"{OutputDirectory}\\{DateTime.Now.ToFileTime()}.txt");
                    writer.Write(json);
                    writer.Close();

                    HasSavedRecords = true;
                }
            }
        }

        private bool HasPostControlChanged()
        {
            bool retHasChanged = false;

            // First pass will be empty
            if (LastKnownControl.Any())
            {
                retHasChanged = PostLogicList.Any(post => LastKnownControl[post] != post.ControllingTeam);
            }

            LastKnownControl.Clear();
            PostLogicList.ForEach(post => LastKnownControl.Add(post, post.ControllingTeam));

            return retHasChanged;
        }

        private void UpdateState(LogEventType logEvent)
        {
            if (logEvent == LogEventType.TimerTriggered)
            {
                LastLogTime = Time.time;
            }

            NumberOfLogs++;
            SmartBots.ForEach(bot => RequestBotState(bot, logEvent));
        }

        public void SubscribeTo(SmartBot smartBot)
        {
            SmartBots.Add(smartBot);
        }

        private void RequestBotState(SmartBot smartBot, LogEventType logEvent)
        {
            TeamType EnemyTeam = smartBot.Team == TeamType.RedTeam
                ? TeamType.BlueTeam
                : TeamType.RedTeam;

            var currentState = new GameState.BotState()
            {
                LogGroup = NumberOfLogs,
                BotsTeam = smartBot.Team,
                LogEvent = logEvent,
                BotsTeamsScore = GameLogic.TeamPoints.GetValueOrDefault(smartBot.Team),
                EnemyTeamScore = GameLogic.TeamPoints.GetValueOrDefault(EnemyTeam),
            };

            var allFriendlyBots = SmartBots
                .Where(bot => bot.Team == smartBot.Team)
                .ToList();

            foreach (var post in PostLogicList)
            {
                var postState = new GameState.CommandPostState()
                {
                    PostNumber = int.Parse(Regex.Match(post.name, @"(\d+)").Value),
                    ControllingTeam = post.ControllingTeam,
                    DistanceToBot = post.transform.GetDistanceTo(smartBot.transform),
                    AverageDistanceFromBotsTeam = allFriendlyBots
                        .Select(bot => post.transform.GetDistanceTo(bot.transform))
                        .Average()
                };

                currentState.CommandPostRelativeState.Add(postState);
            }

            StateOfGame.BotStates.Add(currentState);
        }
    }

    [Serializable]
    public class GameState
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
}
