using Assets.Scripts.Enums;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Entities
{
    public class BaseBot : BaseEntity
    {
        protected Transform Target = null;
        protected NavMeshAgent Agent;

        [SerializeField] private Material DefaultMaterial;
        private Material ObejctMaterial;

        public void BaseBotStart(TeamType team)
        {
            BaseStart(team);

            Agent = GetComponent<NavMeshAgent>();

            ObejctMaterial = new Material(DefaultMaterial);
            ObejctMaterial.color = GetTeamColor();
            gameObject.GetComponentInChildren<MeshRenderer>().material = ObejctMaterial;
        }
    }
}
