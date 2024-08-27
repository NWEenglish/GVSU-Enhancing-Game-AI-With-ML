using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Enums;
using Assets.Scripts.Gamemode.Conquest;
using UnityEngine;

namespace Assets.Scripts.Entities
{

    //  CLASS PENDING DELETION --- Use MasterMLOrchestrator for ML process

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

        //private RawGameState StateOfGame = new RawGameState();

        private void Start()
        {
            GameLogic = GameObject.FindObjectOfType<ConquestGameLogic>();
        }

        private void Update()
        {
            // Due to timing issues, we might not have this list populated on startup
            //if (!PostLogicList.Any())
            //{
            //    PostLogicList = GameLogic.CommandPosts;
            //    return;
            //}

            //if (!HasSavedRecords)
            //{
            //    if (NumberOfLogs == 0 || Time.time >= LastLogTime + TimeBetweenLogs)
            //    {
            //        UpdateState(LogEventType.TimerTriggered);
            //    }

            //    if (GameLogic.IsGameOver)
            //    {
            //        UpdateState(LogEventType.GameOver);

            //        List<TeamType> teams = SmartBots
            //            .Select(bot => bot.Team)
            //            .Distinct()
            //            .ToList();

            //        foreach (var team in teams)
            //        {
            //            BackPropagate(team);
            //        }

            //        // Save records
            //        HasSavedRecords = StateOfGame.ToSaveFile(MLConstants.RawDataFilePath);
            //    }
            //}
        }

        private void BackPropagate(TeamType team)
        {
            //TeamType? winningTeam = GameLogic.GetWinningTeam();
            //int maxLogGroup = StateOfGame.BotStates.Max(state => state.LogGroup);

            //List<RawGameState.BotState> teamStates = StateOfGame.BotStates
            //    .Where(state => state.BotsTeam == team)
            //    .ToList();

            //// Set reward on last state
            //teamStates.Where(state => state.LogEvent == LogEventType.GameOver)
            //    .ToList()
            //    .ForEach(state => state.Reward = GetGameOverReward(winningTeam, team));

            //for (int level = maxLogGroup - 1; level > 0; level--)
            //{
            //    var states = teamStates.Where(state => state.LogGroup == level);
            //}
        }

        private int GetGameOverReward(TeamType? winningTeam, TeamType botTeam)
        {
            int reward;

            if (winningTeam == null)
            {
                reward = -20;
            }
            else if (winningTeam == botTeam)
            {
                reward = 1000;
            }
            else
            {
                reward = -1000;
            }

            return reward;
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

        public void NotifyOfCapture(SmartBot smartBot)
        {
            NumberOfLogs++;
            SmartBots.ForEach(bot => RequestBotState(bot, bot == smartBot ? LogEventType.CommandPostChange : LogEventType.TimerTriggered));
        }

        private void RequestBotState(SmartBot smartBot, LogEventType logEvent)
        {
            TeamType EnemyTeam = GetEnemyTeam(smartBot.Team);
            int stateReward = DetermineStateReward(LogEventType.CommandPostChange, GameLogic.TeamPoints[smartBot.Team], GameLogic.TeamPoints[EnemyTeam]);
            smartBot.ReceiveReward(stateReward);

            //var currentState = new RawGameState.BotState()
            //{
            //    LogGroup = NumberOfLogs,
            //    BotsTeam = smartBot.Team,
            //    LogEvent = logEvent,
            //    BotsTeamsScore = GameLogic.TeamPoints.GetValueOrDefault(smartBot.Team),
            //    EnemyTeamScore = GameLogic.TeamPoints.GetValueOrDefault(EnemyTeam),
            //    Reward = smartBot.CurrentReward,
            //    TargetPost = GetPostNumber(smartBot?.Target?.name)
            //};

            var allFriendlyBots = SmartBots
                .Where(bot => bot.Team == smartBot.Team)
                .ToList();

            foreach (var post in PostLogicList)
            {
                //var postState = new RawGameState.CommandPostState()
                //{
                //    PostNumber = GetPostNumber(post?.name),
                //    ControllingTeam = post.ControllingTeam,
                //    DistanceToBot = post.transform.GetDistanceTo(smartBot.transform),
                //    AverageDistanceFromBotsTeam = allFriendlyBots
                //        .Select(bot => post.transform.GetDistanceTo(bot.transform))
                //        .Average()
                //};

                //currentState.CommandPostRelativeState.Add(postState);
            }

            //StateOfGame.BotStates.Add(currentState);
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

        private TeamType GetEnemyTeam(TeamType currentTeam)
        {
            return currentTeam == TeamType.RedTeam
                ? TeamType.BlueTeam
                : TeamType.RedTeam;
        }

        private int DetermineStateReward(LogEventType eventType, int teamScore, int enemyScore)
        {
            int retRewardValue = -1; // Always subtracting - we want the game over quick

            if (eventType == LogEventType.CommandPostChange)
            {
                retRewardValue += 100; // It's a good thing to capture posts
            }
            else
            {
                retRewardValue += teamScore - enemyScore; // Positive when team is winning, negative if not
            }

            return retRewardValue;
        }
    }
}
