using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Entities;
using Assets.Scripts.Enums;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class CommandPostLogic : MonoBehaviour
    {
        public TeamType ControllingTeam { get; private set; }

        public int PercentControlledByBlue { get; private set; }
        public int PercentControlledByRed { get; private set; }

        private List<TeamType> AttackingTeams = new List<TeamType>();

        [SerializeField] private Material DefaultMaterial;
        private Material ObejctMaterial;

        private void Start()
        {
            ControllingTeam = TeamType.Neutral;
            ObejctMaterial = new Material(DefaultMaterial);

            gameObject.GetComponentInChildren<MeshRenderer>().material = ObejctMaterial;
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
                ? (float)PercentControlledByRed / 100
                : (float)PercentControlledByBlue / 100;

            float redPercent = (float)PercentControlledByRed / 100;
            float bluePercent = (float)PercentControlledByBlue / 100;

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
            if (PercentControlledByBlue > 100)
            {
                PercentControlledByBlue = 100;
            }
            else if (PercentControlledByBlue < 0)
            {
                PercentControlledByBlue = 0;
            }

            if (PercentControlledByRed > 100)
            {
                PercentControlledByRed = 100;
            }
            else if (PercentControlledByRed < 0)
            {
                PercentControlledByRed = 0;
            }

            // Determine Control
            if (PercentControlledByBlue == 100)
            {
                ControllingTeam = TeamType.BlueTeam;
            }
            else if (PercentControlledByRed == 100)
            {
                ControllingTeam = TeamType.RedTeam;
            }
            else if (PercentControlledByBlue == 0 && PercentControlledByRed == 0)
            {
                ControllingTeam = TeamType.Neutral;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out BaseEntity entity))
            {
                AttackingTeams.Add(entity.Team);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out BaseEntity entity))
            {
                AttackingTeams.Remove(entity.Team);
            }
        }
    }
}
