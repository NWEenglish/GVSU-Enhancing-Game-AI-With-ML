using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Entities;
using Assets.Scripts.Enums;
using Assets.Scripts.Gamemode.Conquest;
using Assets.Scripts.MachineLearning.Helpers;
using Assets.Scripts.MachineLearning.Models;
using UnityEngine;

namespace Assets.Scripts.MachineLearning
{
    public class MasterMLOrchestrator : MonoBehaviour
    {
        private ConquestGameLogic GameLogic;
        private List<CommandPostLogic> PostLogicList = new List<CommandPostLogic>();
        private List<SmartBot> SmartBots = new List<SmartBot>();
        private RawGameState_Master GameState = new RawGameState_Master();
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
                GameState.States.Add(new RawGameState_Master.GameState(currentState));
                GameState.ToSaveFile(MLConstants.RawDataFilePath);
                HaveSaved = true;
            }
            else if (LastGameState != currentState || Time.time >= LastStateSave + MinTimeBetweeenSaves)
            {
                LastGameState = currentState;
                LastStateSave = Time.time;
                GameState.States.Add(new RawGameState_Master.GameState(currentState));
            }
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

        private TeamType GetEnemyTeam()
        {
            return Team == TeamType.RedTeam
                ? TeamType.BlueTeam
                : TeamType.RedTeam;
        }

        public void SubscribeTo(SmartBot smartBot)
        {
            SmartBots.Add(smartBot);
        }
    }
}
