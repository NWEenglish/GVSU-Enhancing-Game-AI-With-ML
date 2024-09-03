using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Constants;
using Assets.Scripts.Enums;
using Assets.Scripts.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Entities
{
    public class BasicBot : BaseBot
    {
        private TargetingStyle TargetingStyle;

        private float? TimeAtTargetSec = null;

        private const float TimeToConsiderStalemate = 60f;

        public void InitValues(TeamType team)
        {
            BaseBotStart(team);
            //TargetingStyle = (TargetingStyle)Random.Range(0, Enum.GetValues(typeof(TargetingStyle)).Length);
            TargetingStyle = TargetingStyle.Random;
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
                if (IsInTargetArea())
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
            UpdateTargetDestination();
        }

        private Transform GetRandomEnemyCommandPost(CommandPostLogic postToSkip = null)
        {
            var posts = GetEnemyCommandPosts(postToSkip);
            return posts.ElementAtOrDefault(Random.Range(0, posts.Count));
        }

        private List<Transform> GetEnemyCommandPosts(CommandPostLogic postToSkip = null)
        {
            var spawn = GetAllySpawn();

            return PostLogicList
                .Where(post => post.ControllingTeam != this.Team && post.GetComponent<CommandPostLogic>() != postToSkip)
                .Select(post => post.gameObject.transform)
                .OrderBy(trans => spawn.transform.GetDistanceTo(trans))
                .ToList();
        }

        private Transform GetPostForDefensiveTarget(CommandPostLogic postToSkip = null)
        {
            Transform retTarget = null;

            GameObject allySpawn = GetAllySpawn();
            GameObject enemySpawn = GetEnemySpawn();

            // Closest enemy post
            var closestEnemyPost = PostLogicList
                .Where(post => post.ControllingTeam != this.Team && post.GetComponent<CommandPostLogic>() != postToSkip)
                .Select(post => post.gameObject.transform)
                .OrderBy(trans => allySpawn.transform.GetDistanceTo(trans))
                .FirstOrDefault();

            // If no enemy posts, use their spawn as a reference
            if (closestEnemyPost == null)
            {
                closestEnemyPost = enemySpawn.transform;
            }

            // Furthest captured post before closest enemy post
            var furthestChainedCapturedPost = PostLogicList
                .Where(post => post.ControllingTeam == this.Team
                    && allySpawn.transform.GetDistanceTo(post.transform) < allySpawn.transform.GetDistanceTo(closestEnemyPost)
                    && post.GetComponent<CommandPostLogic>() != postToSkip)
                .Select(post => post.gameObject.transform)
                .OrderByDescending(trans => allySpawn.transform.GetDistanceTo(trans))
                .FirstOrDefault();

            // If no captured post, target closest enemy, else furthest captured
            retTarget = furthestChainedCapturedPost != null
                ? furthestChainedCapturedPost
                : closestEnemyPost;

            return retTarget;
        }

        private GameObject GetAllySpawn()
        {
            var spawnName = this.Team == TeamType.BlueTeam
                ? Objects.BlueTeam
                : Objects.RedTeam;

            GameObject spawn = GameObject.Find(spawnName);
            return spawn;
        }

        private GameObject GetEnemySpawn()
        {
            var enemySpawnName = this.Team == TeamType.RedTeam
                ? Objects.BlueTeam
                : Objects.RedTeam;

            GameObject spawn = GameObject.Find(enemySpawnName);
            return spawn;
        }
    }
}
