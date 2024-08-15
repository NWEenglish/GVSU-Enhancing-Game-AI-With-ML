using System;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.Constants;
using Assets.Scripts.Entities;
using Assets.Scripts.Enums;
using Assets.Scripts.Gamemode;
using UnityEngine;

namespace Assets.Scripts
{
    public class SpawnerLogic : MonoBehaviour
    {
        [SerializeField]
        private TeamType Team;

        [SerializeField]
        private bool CanPlayerSpawnHere = false;

        private void Start()
        {
            GameSettings gameSettings = GameObject.Find(Objects.GameSettings).GetComponent<GameSettings>();

            // Wait for assignment
            DateTime waitStart = DateTime.Now;
            while (gameSettings.PlayersTeam == null && waitStart.AddSeconds(30) > DateTime.Now) { }

            if (CanPlayerSpawnHere && Team == gameSettings.PlayersTeam)
            {
                GameObject player = GameObject.Find(Objects.Player);
                player.transform.position = gameObject.transform.position;
            }
            else
            {
                GameObject spawnableObjects = GameObject.Find(Objects.SpawnableItems);
                BotAILevel aiLevel = gameSettings.TeamAILevels.GetValueOrDefault(Team);

                if (aiLevel == BotAILevel.BasicAI)
                {
                    SpawnAsBasicBot(spawnableObjects);
                }
                else if (aiLevel == BotAILevel.SmartAI)
                {
                    SpawnAsSmartBot(spawnableObjects);
                }
                else
                {
                    throw new ArgumentNullException(nameof(aiLevel));
                }
            }
        }

        private void SpawnAsBasicBot(GameObject spawnableObjects)
        {
            var basicBot = spawnableObjects.GetComponentInChildren<BasicBot>(true);

            BasicBot newBot = Instantiate(basicBot, this.transform);
            newBot.InitValues(Team);
            newBot.gameObject.SetActive(true);
        }

        private void SpawnAsSmartBot(GameObject spawnableObjects)
        {

        }
    }
}
