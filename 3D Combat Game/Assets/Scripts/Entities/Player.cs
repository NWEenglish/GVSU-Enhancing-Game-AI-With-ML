using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.Extensions;
using Assets.Scripts.Gamemode.Conquest;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class Player : BaseEntity
    {
        [SerializeField]
        private TeamType _Team;

        private Dictionary<TeamType, TextMeshProUGUI> TeamPointsUI = new Dictionary<TeamType, TextMeshProUGUI>();

        private AudioSource AreaSecuredAudio;
        private AudioSource AreaLostAudio;

        private void Start()
        {
            BaseStart(_Team);

            var tmpUI = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().ToList();
            TeamPointsUI.Add(TeamType.RedTeam, tmpUI.FirstOrDefault(ui => ui.name == Constants.HUD_RedPoints));
            TeamPointsUI.Add(TeamType.BlueTeam, tmpUI.FirstOrDefault(ui => ui.name == Constants.HUD_BluePoints));

            var gameLogic = GameObject.Find(Constants.GeneralLogic);
            var audioSources = gameLogic.GetComponentsInChildren<AudioSource>();
            AreaSecuredAudio = audioSources.FirstOrDefault(src => src.clip.name == Constants.Audio_AreaSecured);
            AreaLostAudio = audioSources.FirstOrDefault(src => src.clip.name == Constants.Audio_AreaLost);
        }

        private void FixedUpdate()
        {
            UpdateUI();
        }

        public void NotifyOfCommandPostChange(TeamType previousTeam, TeamType currentTeam)
        {
            if (previousTeam == this.Team)
            {
                AreaLostAudio.TryPlay();
            }
            else if (previousTeam == TeamType.Neutral && currentTeam == this.Team)
            {
                AreaSecuredAudio.TryPlay();
            }
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
