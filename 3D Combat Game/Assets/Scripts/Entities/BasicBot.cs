﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        private Transform Target = null;
        private NavMeshAgent Agent;

        private List<CommandPostLogic> PostLogicList = new List<CommandPostLogic>();

        [SerializeField] private Material DefaultMaterial;
        private Material ObejctMaterial;

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
            UpdateTarget();
        }

        private void UpdateTarget()
        {
            // If no target, get a targt
            if (Target == null)
            {
                var potentialTargets = GetEnemyCommandPosts();
                if (potentialTargets.Any())
                {
                    SetEnemyCommandPostAsTarget();
                }
                else
                {
                    // Go somewhere random?
                }
            }
            else
            {
                var targetPost = Target.GetComponent<CommandPostLogic>();

                // If within bounds, wait for transion
                // Tryz @ https://discussions.unity.com/t/how-can-i-tell-when-a-navmeshagent-has-reached-its-destination/52403
                if (Agent.remainingDistance <= Agent.stoppingDistance)
                {
                    if (targetPost.ControllingTeam == this.Team)
                    {
                        SetEnemyCommandPostAsTarget();
                    }
                    else
                    {
                        // TODO - detect stalemate
                        // If at target for x amount of time, might be stalemate.. random chance to leave

                    }
                }
            }


            // If not at target, when to consider new target

            // If not at target, and not getting a new target, proceed
        }

        private void SetEnemyCommandPostAsTarget()
        {
            Target = TargetingStyle switch
            {
                TargetingStyle.ClosestFirst => GetEnemyCommandPosts().FirstOrDefault(),
                TargetingStyle.FurthestFirst => GetEnemyCommandPosts().LastOrDefault(),
                _ => GetRandomEnemyCommandPost()
            };

            var targetPost = Target.GetComponent<CommandPostLogic>();

            Agent.SetDestination(Target.position);
            Agent.stoppingDistance = Random.Range(0, targetPost.Radius - 5);
        }

        private Transform GetRandomEnemyCommandPost()
        {
            var posts = GetEnemyCommandPosts();
            return posts.ElementAtOrDefault(Random.Range(0, posts.Count));
        }

        private List<Transform> GetEnemyCommandPosts()
        {
            return PostLogicList
                .Where(post => post.ControllingTeam != this.Team)
                .Select(post => post.gameObject.transform)
                .OrderBy(trans => this.transform.GetDistanceTo(trans))
                .ToList();
        }

        private bool ShouldUseRandom()
        {
            return Random.Range(0, 100) > 90;
        }
    }
}