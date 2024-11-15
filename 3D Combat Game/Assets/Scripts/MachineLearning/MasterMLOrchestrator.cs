using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Entities;
using Assets.Scripts.Enums;
using Assets.Scripts.Gamemode.Conquest;
using Assets.Scripts.MachineLearning.Models;
using Assets.Scripts.MachineLearning.V5;
using UnityEngine;

namespace Assets.Scripts.MachineLearning
{
    public class MasterMLOrchestrator : MonoBehaviour
    {
        private Algorithms MLAlgorithm = new Algorithms();
        private ConquestGameLogic GameLogic;
        private List<CommandPostLogic> PostLogicList = new List<CommandPostLogic>();
        private List<SmartBot> SmartBots = new List<SmartBot>();
        private RawGameState GameState;
        private bool HaveSaved = false;
        private string LastGameState = string.Empty;
        private float LastStateSave = 0f;

        private const float MinTimeBetweeenSaves = 5f;

        [SerializeField]
        private TeamType _team;
        public TeamType Team => _team;

        private void Start()
        {
            GameLogic = GameObject.FindObjectOfType<ConquestGameLogic>();
            GameState = new RawGameState()
            {
                Team = Team,
                Version = GameStateHelper.Version
            };
        }

        private void Update()
        {
            string currentState = GetGameStateID();

            // Due to timing issues, we might not have this list populated on startup
            if (!PostLogicList.Any())
            {
                PostLogicList = GameLogic.CommandPosts;
                return;
            }
            if (!SmartBots.Any())
            {
                return;
            }
            else if (HaveSaved)
            {
                return;
            }
            else if (GameLogic.IsGameOver)
            {
                GameState.States.Add(GetGameState());
                GameState.ToSaveFile(MLConstants.RawDataFilePath);
                HaveSaved = true;
            }
            else if (LastGameState != currentState || Time.time >= LastStateSave + MinTimeBetweeenSaves)
            {
                bool isNewGame = string.IsNullOrEmpty(LastGameState);
                bool shouldEvaluateNextStates = isNewGame;

                if (!isNewGame)
                {
                    // If we're in a new percentile state, update targets
                    Dictionary<TeamType, int> lastGameStateScores = GameStateHelper.GetTeamScorePercentile(LastGameState);
                    Dictionary<TeamType, int> currGameStateScores = GameStateHelper.GetTeamScorePercentile(currentState);
                    shouldEvaluateNextStates = lastGameStateScores.Any(kvp => currGameStateScores[kvp.Key] != kvp.Value);
                }

                if (shouldEvaluateNextStates)
                {
                    // Get updated list of next moves
                    EvaluateNextStates(currentState);
                }

                LastGameState = currentState;
                LastStateSave = Time.time;
                GameState.States.Add(GetGameState());
            }
        }

        private void EvaluateNextStates(string currentState)
        {
            var orderedStates = GameStateHelper.GetOrderedStates(MLAlgorithm, Team, currentState, SmartBots.Count());

            // Assign bots based on magnitude and current targets
            Dictionary<SmartBot, int> currentBotTargets = new Dictionary<SmartBot, int>();

            // Get each bots current target
            foreach (var bot in SmartBots)
            {
                int postNumber = bot?.Target?.gameObject?.GetComponent<CommandPostLogic>()?.GetPostNumber() ?? 0;
                currentBotTargets.Add(bot, postNumber);
            }

            List<int> postChanges = new List<int>();
            foreach (var state in orderedStates)
            {
                postChanges.AddRange(GameStateHelper.DetermineStateDifferences(currentState, state, Team));
            }

            Dictionary<int, int> postCounts = new Dictionary<int, int>();
            List<int> unqiueChangedPosts = postChanges.Distinct().ToList();
            foreach (var post in unqiueChangedPosts)
            {
                postCounts.Add(post, postChanges.Count(p => p == post));
            }

            int totalPostChanges = postCounts.Values.Sum();
            Dictionary<int, int> postResourceNeed = new Dictionary<int, int>();
            foreach (var post in unqiueChangedPosts)
            {
                int botsRequired = (int)Math.Floor(SmartBots.Count() * (double)postCounts[post] / totalPostChanges);
                postResourceNeed.Add(post, botsRequired);
            }

            var sortedPostNeed = postResourceNeed.OrderByDescending(kvp => kvp.Value);

            // First pass is to unassign extra bots
            foreach (var post in sortedPostNeed)
            {
                IEnumerable<KeyValuePair<SmartBot, int>> botsAssignedToPost = currentBotTargets.Where(kvp => kvp.Value == post.Key);
                int alreadyAssigned = botsAssignedToPost.Count();
                int netChange = Math.Abs(post.Value - alreadyAssigned);

                if (netChange < 0)
                {
                    var botsBeingRemoved = botsAssignedToPost
                        .OrderByDescending(kvp => kvp.Key.DistanceToTarget())
                        .Take(netChange)
                        .ToList();

                    foreach (var bot in botsBeingRemoved)
                    {
                        currentBotTargets[bot.Key] = 0;
                    }
                }
            }

            // Second pass is to assign extra bots
            foreach (var post in sortedPostNeed)
            {
                IEnumerable<KeyValuePair<SmartBot, int>> botsAssignedToPost = currentBotTargets.Where(kvp => kvp.Value == post.Key);
                int alreadyAssigned = botsAssignedToPost.Count();
                int netChange = Math.Abs(post.Value - alreadyAssigned);

                if (netChange > 0)
                {
                    var unassignedBots = currentBotTargets
                        .Where(kvp => kvp.Value == 0)
                        .OrderBy(kvp => kvp.Key.DistanceToTarget())
                        .Select(kvp => kvp.Key)
                        .Take(netChange)
                        .ToList();

                    foreach (var uBot in unassignedBots)
                    {
                        currentBotTargets[uBot] = post.Key;
                    }
                }
            }

            // Update bots
            foreach (var bot in SmartBots)
            {
                CommandPostLogic newTarget = PostLogicList.FirstOrDefault(post => post.GetPostNumber() == currentBotTargets[bot]);

                // Let the bot pick something random
                if (MLAlgorithm.ShouldUseRandomValue())
                {
                    newTarget = null;
                }

                bot.UpdateTarget(newTarget);
            }
        }

        private RawGameState.GameState GetGameState()
        {
            Dictionary<int, TeamType> postTeams = new Dictionary<int, TeamType>();
            foreach (var post in PostLogicList)
            {
                int postNumber = post.GetPostNumber();
                postTeams.Add(postNumber, post.ControllingTeam);
            }

            return new RawGameState.GameState(GameLogic.MaxPointsPerTeam, GameLogic.TeamPoints[TeamType.RedTeam], GameLogic.TeamPoints[TeamType.BlueTeam], postTeams);
        }

        private string GetGameStateID()
        {

            int maxPoints = GameLogic.MaxPointsPerTeam;
            int redTeamPoints = GameLogic.TeamPoints[TeamType.RedTeam];
            int blueTeamPoints = GameLogic.TeamPoints[TeamType.BlueTeam];

            Dictionary<int, TeamType> postTeams = new Dictionary<int, TeamType>();
            foreach (var post in PostLogicList)
            {
                int postNumber = post.GetPostNumber();
                postTeams.Add(postNumber, post.ControllingTeam);
            }

            var retGameStateID = GameStateHelper.GetGameState(maxPoints, redTeamPoints, blueTeamPoints, postTeams);
            return retGameStateID;
        }

        public void SubscribeTo(SmartBot smartBot)
        {
            SmartBots.Add(smartBot);
        }
    }
}
