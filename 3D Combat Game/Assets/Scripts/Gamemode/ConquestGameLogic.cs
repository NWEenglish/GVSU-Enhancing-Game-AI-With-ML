using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Gamemode.Conquest
{
    public class ConquestGameLogic : MonoBehaviour
    {
        public Dictionary<TeamType, int> TeamPoints { get; private set; } = new Dictionary<TeamType, int>();
        public List<CommandPostLogic> CommandPosts { get; private set; } = new List<CommandPostLogic>();

        private const int PointsPerTick = 1;
        private const int TickLengthMs = 2500;
        private const int MaxPointsPerTeam = 500;

        private DateTime LastTimePointsDistributed = DateTime.MinValue;

        private void Start()
        {
            foreach (TeamType team in TeamTypeHelper.GetPlayableTeams())
            {
                TeamPoints.Add(team, 0);
            }

            CommandPosts = Resources.FindObjectsOfTypeAll<CommandPostLogic>().ToList();
        }

        private void FixedUpdate()
        {
            UpdatePoints();
        }

        private void UpdatePoints()
        {
            if (LastTimePointsDistributed.AddMilliseconds(TickLengthMs) <= DateTime.Now)
            {
                CommandPosts
                    .Where(post => TeamTypeHelper.GetPlayableTeams().Contains(post.ControllingTeam))
                    .ToList()
                    .ForEach(post => TeamPoints[post.ControllingTeam] = CalculateUpdatedPoints(TeamPoints[post.ControllingTeam]));

                LastTimePointsDistributed = DateTime.Now;
            }
        }

        private int CalculateUpdatedPoints(int currentPoints)
        {
            int retPoints = currentPoints + PointsPerTick;
            retPoints = retPoints > MaxPointsPerTeam
                ? MaxPointsPerTeam
                : retPoints;

            return retPoints;
        }
    }
}
