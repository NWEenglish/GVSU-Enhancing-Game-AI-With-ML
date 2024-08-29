using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.MachineLearning;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Entities
{
    public class SmartBot : BaseBot
    {
        private MasterMLOrchestrator MasterLogic;
        private float LastUpdate = 0f;
        private TeamType TargetPostTeam;

        public void InitValues(TeamType team)
        {
            BaseBotStart(team);
        }

        private void Start()
        {
            MasterLogic = GameObject.FindObjectsOfType<MasterMLOrchestrator>().First(orch => orch.Team == this.Team);
            MasterLogic.SubscribeTo(this);
        }

        private void Update()
        {
            BaseUpdate();
        }

        public void UpdateTarget(CommandPostLogic newTargetPost = null)
        {
            if (newTargetPost == null)
            {
                Target = GetRandomTarget();
            }
            else
            {
                Target = PostLogicList.FirstOrDefault(post => post == newTargetPost).transform;
            }

            UpdateTargetDestination();
        }

        public float DistanceToTarget()
        {
            float retDistance = float.MaxValue;

            if (Target != null)
            {
                retDistance = Agent.remainingDistance;
            }

            return retDistance;
        }

        private Transform GetRandomTarget()
        {
            var posts = PostLogicList
                .Select(post => post.gameObject.transform)
                .ToList();

            return posts.ElementAtOrDefault(Random.Range(0, posts.Count));
        }
    }
}
