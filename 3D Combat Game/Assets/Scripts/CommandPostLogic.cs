using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Entities;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts
{
    public class CommandPostLogic : MonoBehaviour
    {
        public TeamType ControllingTeam { get; private set; }
        public float Radius { get; private set; }

        public int PercentControlledByBlue { get; private set; }
        public int PercentControlledByRed { get; private set; }

        private int TotalPercentToBeControlled => 500;

        private List<TeamType> AttackingTeams = new List<TeamType>();
        private Player Player;

        [SerializeField] private Material DefaultMaterial;
        private Material ObejctMaterial;

        private void Start()
        {
            ControllingTeam = TeamType.Neutral;
            Radius = GetComponent<SphereCollider>().radius;
            ObejctMaterial = new Material(DefaultMaterial);

            gameObject.GetComponentInChildren<MeshRenderer>().material = ObejctMaterial;
            Player = Resources.FindObjectsOfTypeAll<Player>().FirstOrDefault();
        }

        private void FixedUpdate()
        {
            int blueTeamAttackers = AttackingTeams.FindAll(team => team == TeamType.BlueTeam).Count();
            int redTeamAttackers = AttackingTeams.FindAll(team => team == TeamType.RedTeam).Count();

            TransitionControllingTeam(blueTeamAttackers, redTeamAttackers);
            UpdateColor();
        }

        private void UpdateColor()
        {
            float change = PercentControlledByRed > 0
                ? (float)PercentControlledByRed / TotalPercentToBeControlled
                : (float)PercentControlledByBlue / TotalPercentToBeControlled;

            float redPercent = (float)PercentControlledByRed / TotalPercentToBeControlled;
            float bluePercent = (float)PercentControlledByBlue / TotalPercentToBeControlled;

            float red = redPercent > 0
                ? redPercent
                : 1f - change;
            float blue = bluePercent > 0
                ? bluePercent
                : 1f - change;

            float green = 1f - change;
            float alpha = 1f;

            ObejctMaterial.color = new Color(red, green, blue, alpha);
        }

        private void TransitionControllingTeam(int blueTeam, int redTeam)
        {
            TeamType prevControllingTeam = ControllingTeam;
            int playerDiff = Math.Abs(blueTeam - redTeam);

            TeamType mostPlayers = TeamType.Neutral;
            if (blueTeam > redTeam)
            {
                mostPlayers = TeamType.BlueTeam;
            }
            else if (blueTeam < redTeam)
            {
                mostPlayers = TeamType.RedTeam;
            }

            // Blue is winning control
            if (mostPlayers == TeamType.BlueTeam)
            {
                // Red was previously winning
                if (PercentControlledByRed > 0)
                {
                    PercentControlledByRed -= playerDiff;
                }
                // Was Neutral or Blue is winning
                else
                {
                    PercentControlledByBlue += playerDiff;
                }
            }
            // Red is winning control
            else if (mostPlayers == TeamType.RedTeam)
            {
                // Blue was previously winning
                if (PercentControlledByBlue > 0)
                {
                    PercentControlledByBlue -= playerDiff;
                }
                // Was Neutral or Red is winning
                else
                {
                    PercentControlledByRed += playerDiff;
                }
            }
            // No one around to take it, reset percentages if it was already neutral
            else if (ControllingTeam == TeamType.Neutral && blueTeam == 0 && redTeam == 0)
            {
                PercentControlledByBlue = 0;
                PercentControlledByRed = 0;
            }

            // Clean up percentages
            if (PercentControlledByBlue > TotalPercentToBeControlled)
            {
                PercentControlledByBlue = TotalPercentToBeControlled;
            }
            else if (PercentControlledByBlue < 0)
            {
                PercentControlledByBlue = 0;
            }

            if (PercentControlledByRed > TotalPercentToBeControlled)
            {
                PercentControlledByRed = TotalPercentToBeControlled;
            }
            else if (PercentControlledByRed < 0)
            {
                PercentControlledByRed = 0;
            }

            // Determine Control
            if (PercentControlledByBlue == TotalPercentToBeControlled)
            {
                ControllingTeam = TeamType.BlueTeam;
            }
            else if (PercentControlledByRed == TotalPercentToBeControlled)
            {
                ControllingTeam = TeamType.RedTeam;
            }
            else if (PercentControlledByBlue == 0 && PercentControlledByRed == 0)
            {
                ControllingTeam = TeamType.Neutral;
            }

            if (prevControllingTeam != ControllingTeam && Player != null)
            {
                Player.NotifyOfCommandPostChange(prevControllingTeam, ControllingTeam);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out BaseEntity entity))
            {
                AttackingTeams.Add(entity.Team);
            }
        }

        private void OnTriggerExit(Collider other) // TODO: fix for when entity is destroyed
        {
            if (other.TryGetComponent(out BaseEntity entity))
            {
                AttackingTeams.Remove(entity.Team);
            }
        }
    }
}
