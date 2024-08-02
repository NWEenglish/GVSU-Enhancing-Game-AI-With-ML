using Assets.Scripts.Enums;
using Assets.Scripts.Gamemode.Conquest;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class BaseEntity : MonoBehaviour
    {
        public TeamType Team { get; private set; }
        public ConquestGameLogic ConquestGameLogic { get; protected set; }

        protected void BaseStart(TeamType team)
        {
            ConquestGameLogic = GameObject.FindObjectOfType<ConquestGameLogic>();
            Team = team;
        }

        protected Color GetTeamColor()
        {
            return this.Team == TeamType.BlueTeam
                    ? Color.blue
                    : Color.red;
        }
    }
}
