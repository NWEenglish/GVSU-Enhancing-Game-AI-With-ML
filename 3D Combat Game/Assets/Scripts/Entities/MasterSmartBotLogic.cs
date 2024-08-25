using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Enums;
using Assets.Scripts.Extensions;
using Assets.Scripts.Gamemode.Conquest;
using Assets.Scripts.MachineLearning;
using Assets.Scripts.MachineLearning.Models;
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
        private bool HasSavedRecords = false;
        private const float TimeBetweenLogs = 20f;

        private RawGameState StateOfGame = new RawGameState();

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
                //else if (HasPostControlChanged())
                //{
                //    UpdateState(LogEventType.CommandPostChange);
                //}

                if (GameLogic.IsGameOver)
                {
                    // Save records
                    HasSavedRecords = StateOfGame.ToSaveFile(MLConstants.RawDataFilePath);
                }
            }
        }

        //private bool HasPostControlChanged()
        //{
        //    bool retHasChanged = false;

        //    // First pass will be empty
        //    if (LastKnownControl.Any())
        //    {
        //        retHasChanged = PostLogicList.Any(post => LastKnownControl[post] != post.ControllingTeam);
        //    }

        //    LastKnownControl.Clear();
        //    PostLogicList.ForEach(post => LastKnownControl.Add(post, post.ControllingTeam));

        //    return retHasChanged;
        //}

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

        public void NotifyOfCapture(SmartBot smartBot)
        {
            NumberOfLogs++;
            SmartBots.ForEach(bot => RequestBotState(bot, bot == smartBot ? LogEventType.CommandPostChange : LogEventType.TimerTriggered));
        }

        private void RequestBotState(SmartBot smartBot, LogEventType logEvent)
        {
            TeamType EnemyTeam = smartBot.Team == TeamType.RedTeam
                ? TeamType.BlueTeam
                : TeamType.RedTeam;

            var currentState = new RawGameState.BotState()
            {
                LogGroup = NumberOfLogs,
                BotsTeam = smartBot.Team,
                LogEvent = logEvent,
                BotsTeamsScore = GameLogic.TeamPoints.GetValueOrDefault(smartBot.Team),
                EnemyTeamScore = GameLogic.TeamPoints.GetValueOrDefault(EnemyTeam),
                TargetPost = GetPostNumber(smartBot?.Target?.name)
            };

            var allFriendlyBots = SmartBots
                .Where(bot => bot.Team == smartBot.Team)
                .ToList();

            foreach (var post in PostLogicList)
            {
                var postState = new RawGameState.CommandPostState()
                {
                    PostNumber = GetPostNumber(post?.name),
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

        private int GetPostNumber(string postName)
        {
            int postNumber = 0;

            if (!string.IsNullOrEmpty(postName))
            {
                postNumber = int.Parse(Regex.Match(postName, @"(\d+)").Value);
            }

            return postNumber;
        }
    }
}
