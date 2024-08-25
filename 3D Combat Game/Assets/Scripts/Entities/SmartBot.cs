using System.Linq;
using Assets.Scripts.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Entities
{
    public class SmartBot : BaseBot
    {
        private MasterSmartBotLogic MasterLogic;

        private float LastUpdate = 0f;

        public void InitValues(TeamType team)
        {
            BaseBotStart(team);
        }

        private void Start()
        {
            MasterLogic = GameObject.FindObjectOfType<MasterSmartBotLogic>();
            MasterLogic.SubscribeTo(this);
        }

        private void Update()
        {
            BaseUpdate();
        }

        private void FixedUpdate()
        {
            // Due to timing issues, we might not have this list populated on startup
            if (!PostLogicList.Any() || Time.time < LastUpdate + 15f)
            {
                return;
            }

            // Replace with logic
            Target = GetRandomTarget();
            UpdateTargetDestination();
            LastUpdate = Time.time;
        }

        private Transform GetRandomTarget(CommandPostLogic postToSkip = null)
        {
            var posts = PostLogicList
                .Where(post => post.ControllingTeam != this.Team && post.GetComponent<CommandPostLogic>() != postToSkip)
                .Select(post => post.gameObject.transform)
                .ToList();

            return posts.ElementAtOrDefault(Random.Range(0, posts.Count));
        }
    }
}
