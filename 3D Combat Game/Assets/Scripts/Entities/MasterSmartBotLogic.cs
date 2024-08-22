using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private List<SmartBot> SmartBots = new List<SmartBot>();

        private int NumberOfLogs = 0;
        private float LastLogTime = 0f;
        private float TimeBetweenLogs = 20f;
        private bool HasSavedRecords = false;

        private BotStates States = new BotStates();

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
                    UpdateState();
                }

                if (GameLogic.IsGameOver)
                {
                    // Save records
                    string json = JsonUtility.ToJson(States);

                    var writer = File.CreateText($"{OutputDirectory}\\{DateTime.Now.ToFileTime()}.txt");
                    writer.Write(json);
                    writer.Close();

                    HasSavedRecords = true;
                }
            }
        }

        private void UpdateState()
        {
            NumberOfLogs++;
            LastLogTime = Time.time;
            SmartBots.ForEach(bot => RequestBotState(bot));
        }

        public void SubscribeTo(SmartBot smartBot)
        {
            SmartBots.Add(smartBot);
        }

        private void RequestBotState(SmartBot bot)
        {
            TeamType EnemyTeam = bot.Team == TeamType.RedTeam
                ? TeamType.BlueTeam
                : TeamType.RedTeam;

            var currentState = new BotStates.BotState()
            {
                LogGroup = NumberOfLogs,
                BotsTeam = bot.Team,
                BotsTeamsScore = GameLogic.TeamPoints.GetValueOrDefault(bot.Team),
                EnemyTeamScore = GameLogic.TeamPoints.GetValueOrDefault(EnemyTeam),
            };

            PostLogicList.ForEach(post => currentState.DistanceToPosts.Add(post.name, post.transform.GetDistanceTo(bot.gameObject.transform)));
            States.AddState(currentState);
        }
    }

    [Serializable]
    public class BotStates
    {
        public List<BotState> States = new List<BotState>();

        public void AddState(BotState state)
        {
            States.Add(state);
        }

        [Serializable]
        public class BotState
        {
            public int LogGroup;
            public TeamType BotsTeam;
            public Dictionary<string, float> DistanceToPosts = new Dictionary<string, float>();
            public int BotsTeamsScore;
            public int EnemyTeamScore;
        }
    }


}
