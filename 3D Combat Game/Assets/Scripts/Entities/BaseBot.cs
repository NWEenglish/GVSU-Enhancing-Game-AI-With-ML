using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Entities
{
    public class BaseBot : BaseEntity
    {
        public Transform Target { get; protected set; } = null;
        protected NavMeshAgent Agent;

        [SerializeField] private Material DefaultMaterial;
        private Material ObejctMaterial;

        protected List<CommandPostLogic> PostLogicList = new List<CommandPostLogic>();

        protected void BaseBotStart(TeamType team)
        {
            BaseStart(team);

            Agent = GetComponent<NavMeshAgent>();

            ObejctMaterial = new Material(DefaultMaterial);
            ObejctMaterial.color = GetTeamColor();
            gameObject.GetComponentInChildren<MeshRenderer>().material = ObejctMaterial;
        }

        protected void BaseUpdate()
        {
            // Due to timing issues, we might not have this list populated on startup
            if (!PostLogicList.Any())
            {
                PostLogicList = ConquestGameLogic.CommandPosts;
            }
        }

        protected void UpdateTargetDestination()
        {
            var targetPost = Target.GetComponent<CommandPostLogic>();
            Agent.SetDestination(Target.position);
            Agent.stoppingDistance = Random.Range(5, targetPost.Radius - 5);
        }
    }
}
