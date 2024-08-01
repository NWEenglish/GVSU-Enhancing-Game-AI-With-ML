using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.Gamemode.Conquest;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class Player : BaseEntity
    {
        public override TeamType Team { get; protected set; }

        [SerializeField]
        private TeamType _Team;

        private Dictionary<TeamType, TextMeshProUGUI> TeamPointsUI = new Dictionary<TeamType, TextMeshProUGUI>();

        private void Start()
        {
            BaseStart();

            Team = _Team;

            var tmpUI = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().ToList();
            TeamPointsUI.Add(TeamType.RedTeam, tmpUI.FirstOrDefault(ui => ui.name == Constants.HUD_RedPoints));
            TeamPointsUI.Add(TeamType.BlueTeam, tmpUI.FirstOrDefault(ui => ui.name == Constants.HUD_BluePoints));
        }

        private void FixedUpdate()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (var teamPoints in TeamPointsUI)
            {
                TeamPointsUI[teamPoints.Key].text = ConquestGameLogic.TeamPoints[teamPoints.Key].ToString();
                TeamPointsUI[teamPoints.Key].color = teamPoints.Key == TeamType.BlueTeam
                    ? Color.blue
                    : Color.red;
            }
        }
    }
}
