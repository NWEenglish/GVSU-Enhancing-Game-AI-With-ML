using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class Player : BaseEntity
    {
        public override TeamType Team { get; protected set; }

        [SerializeField] private TeamType _Team;

        public Player()
        {
            Team = _Team;
        }

        private void Update()
        {
            Team = _Team;
        }
    }
}
