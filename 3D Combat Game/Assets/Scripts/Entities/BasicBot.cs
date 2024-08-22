using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Constants;
using Assets.Scripts.Enums;
using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Entities
{
    public class BasicBot : BaseEntity
    {
        [SerializeField]
        private TeamType Team;
        private TargetingStyle TargetingStyle;

        private float? TimeAtTargetSec = null;
        private Transform Target = null;
        private NavMeshAgent Agent;

        private List<CommandPostLogic> PostLogicList = new List<CommandPostLogic>();

        [SerializeField] private Material DefaultMaterial;
        private Material ObejctMaterial;

        private const float TimeToConsiderStalemate = 60f;

        public void InitValues(TeamType team)
        {
            Team = team;
            TargetingStyle = (TargetingStyle)Random.Range(0, Enum.GetValues(typeof(TargetingStyle)).Length);
        }

        private void Start()
        {
            BaseStart(Team);

            Agent = GetComponent<NavMeshAgent>();

            ObejctMaterial = new Material(DefaultMaterial);
            ObejctMaterial.color = GetTeamColor();
            gameObject.GetComponentInChildren<MeshRenderer>().material = ObejctMaterial;
        }

        private void Update()
        {
            // Due to timing issues, we might not have this list populated on startup
            if (!PostLogicList.Any())
            {
                PostLogicList = ConquestGameLogic.CommandPosts;
            }
        }

        private void FixedUpdate()
        {
            // Due to timing issues, we might not have this list populated on startup
            if (!PostLogicList.Any())
            {
                return;
            }

            UpdateTarget();
        }

        private void UpdateTarget()
        {
            // If no target, set a target
            if (Target == null)
            {
                SetCommandPostAsTarget();
            }
            // Defensive play style always needs to be evaluating its position
            else if (this.TargetingStyle == TargetingStyle.Defensive)
            {
                SetCommandPostAsTarget();
            }
            else
            {
                var targetPost = Target.GetComponent<CommandPostLogic>();

                // If within bounds, wait for transition
                // Tryz @ https://discussions.unity.com/t/how-can-i-tell-when-a-navmeshagent-has-reached-its-destination/52403
                if (Agent.remainingDistance <= Agent.stoppingDistance)
                {
                    // If target is now controlled, set a new target
                    if (targetPost.ControllingTeam == this.Team)
                    {
                        SetCommandPostAsTarget();
                    }
                    else
                    {
                        if (TimeAtTargetSec == null && Agent.hasPath)
                        {
                            TimeAtTargetSec = Time.time;
                        }
                        else if (TimeAtTargetSec + TimeToConsiderStalemate <= Time.time && !targetPost.IsChanging())
                        {
                            SetCommandPostAsTarget(targetPost);
                        }
                    }
                }
            }
        }

        private void SetCommandPostAsTarget(CommandPostLogic postToSkip = null)
        {
            Target = TargetingStyle switch
            {
                TargetingStyle.ClosestFirst => GetEnemyCommandPosts(postToSkip).FirstOrDefault(),
                TargetingStyle.FurthestFirst => GetEnemyCommandPosts(postToSkip).LastOrDefault(),
                TargetingStyle.Defensive => GetPostForDefensiveTarget(postToSkip),
                _ => GetRandomEnemyCommandPost(postToSkip)
            };

            // If no post was selected (probably because they were all captured), select a random one to go to
            if (Target == null)
            {
                Target = GetRandomEnemyCommandPost(postToSkip);
            }

            TimeAtTargetSec = null;
            var targetPost = Target.GetComponent<CommandPostLogic>();
            Agent.SetDestination(Target.position);
            Agent.stoppingDistance = Random.Range(5, targetPost.Radius - 5);
        }

        private Transform GetRandomEnemyCommandPost(CommandPostLogic postToSkip = null)
        {
            var posts = GetEnemyCommandPosts(postToSkip);
            return posts.ElementAtOrDefault(Random.Range(0, posts.Count));
        }

        private List<Transform> GetEnemyCommandPosts(CommandPostLogic postToSkip = null)
        {
            return PostLogicList
                .Where(post => post.ControllingTeam != this.Team && post.GetComponent<CommandPostLogic>() != postToSkip)
                .Select(post => post.gameObject.transform)
                .OrderBy(trans => this.transform.GetDistanceTo(trans))
                .ToList();
        }

        private Transform GetPostForDefensiveTarget(CommandPostLogic postToSkip = null)
        {
            var spawnName = this.Team == TeamType.BlueTeam
                ? Objects.BlueTeam
                : Objects.RedTeam;

            var spawn = GameObject.Find(spawnName);

            // Closest enemy post
            var closestEnemyPost = PostLogicList
                .Where(post => post.ControllingTeam != this.Team && post.GetComponent<CommandPostLogic>() != postToSkip)
                .Select(post => post.gameObject.transform)
                .OrderBy(trans => spawn.transform.GetDistanceTo(trans))
                .FirstOrDefault();

            // Furthest captured post before closest enemy post
            var furthestChainedCapturedPost = PostLogicList
                .Where(post => post.ControllingTeam == this.Team
                    && spawn.transform.GetDistanceTo(post.transform) < spawn.transform.GetDistanceTo(closestEnemyPost.transform)
                    && post.GetComponent<CommandPostLogic>() != postToSkip)
                .Select(post => post.gameObject.transform)
                .OrderByDescending(trans => spawn.transform.GetDistanceTo(trans))
                .FirstOrDefault();

            // If no captured post, target closest enemy, else furthest captured
            var target = furthestChainedCapturedPost != null
                ? furthestChainedCapturedPost
                : closestEnemyPost;

            return target;
        }
    }
}
