﻿using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Constants;
using Assets.Scripts.Enums;
using Assets.Scripts.Extensions;
using Assets.Scripts.Gamemode;
using Assets.Scripts.Gamemode.Conquest;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class Player : BaseEntity
    {
        private Dictionary<TeamType, TextMeshProUGUI> TeamPointsUI = new Dictionary<TeamType, TextMeshProUGUI>();

        private AudioSource AreaSecuredAudio;
        private AudioSource AreaLostAudio;
        private AudioSource GameWonAudio;
        private AudioSource GameLostAudio;

        private void Start()
        {
            GameSettings gameSettings = GameObject.Find(Objects.GameSettings).GetComponent<GameSettings>();
            BaseStart(gameSettings.PlayersTeam.Value);

            var tmpUI = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().ToList();
            TeamPointsUI.Add(TeamType.RedTeam, tmpUI.FirstOrDefault(ui => ui.name == HUD.RedPoints));
            TeamPointsUI.Add(TeamType.BlueTeam, tmpUI.FirstOrDefault(ui => ui.name == HUD.BluePoints));

            var gameLogic = GameObject.Find(Objects.GeneralLogic);
            var audioSources = gameLogic.GetComponentsInChildren<AudioSource>();
            AreaSecuredAudio = audioSources.FirstOrDefault(src => src.clip.name == Audio.AreaSecured);
            AreaLostAudio = audioSources.FirstOrDefault(src => src.clip.name == Audio.AreaLost);
            GameWonAudio = audioSources.FirstOrDefault(src => src.clip.name == Audio.GameWon);
            GameLostAudio = audioSources.FirstOrDefault(src => src.clip.name == Audio.GameLost);
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

        public void NotifyOfGameResult(bool playerWon)
        {
            var audioSource = playerWon
                ? GameWonAudio
                : GameLostAudio;

            audioSource.TryPlay();
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
