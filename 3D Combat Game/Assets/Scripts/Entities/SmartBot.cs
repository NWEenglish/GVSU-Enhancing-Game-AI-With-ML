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

        private TeamType TargetPostTeam;

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
            if (!PostLogicList.Any())
            {
                return;
            }
            else if (Target == null)
            {
                // Replace with logic
                Target = GetRandomTarget();
                UpdateTargetDestination();
            }
            else
            {
                var targetPost = Target.GetComponent<CommandPostLogic>();

                if (!IsInTargetArea())
                {
                    TargetPostTeam = targetPost.ControllingTeam;
                }
                else if (IsInTargetArea())
                {
                    // The gist is, we need to award for captures only when they reach a post and contribute to it being captured by their team
                    if (TargetPostTeam != targetPost.ControllingTeam && targetPost.ControllingTeam == this.Team)
                    {
                        MasterLogic.NotifyOfCapture(this);

                        // Ask for target
                    }
                    else if (!targetPost.IsChanging())
                    {
                        // Ask for target
                    }
                }
            }

            // On a timer, asking for a new target every so often (since the game could've drastically changed)
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
