using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Gamemode.Conquest
{
    public class ConquestGameLogic : MonoBehaviour
    {
        private const int PointsPerTick = 1;
        private const int TickLengthMs = 2500;

        private Dictionary<TeamType, int> TeamPoints = new Dictionary<TeamType, int>();
        private List<CommandPostLogic> CommandPosts = new List<CommandPostLogic>();
        private DateTime LastTimePointsDistributed = DateTime.MinValue;

        private void Start()
        {
            foreach (TeamType team in TeamTypeHelper.GetPlayableTeams())
            {
                TeamPoints.Add(team, 0);
            }

            Resources.FindObjectsOfTypeAll(typeof(CommandPostLogic)).ToList().ForEach(gameObj => CommandPosts.Add((CommandPostLogic)gameObj));
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
                    .ForEach(post => TeamPoints[post.ControllingTeam] += PointsPerTick);

                LastTimePointsDistributed = DateTime.Now;
            }
        }
    }
}
