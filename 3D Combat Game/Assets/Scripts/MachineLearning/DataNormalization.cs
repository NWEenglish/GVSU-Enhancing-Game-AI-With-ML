using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.MachineLearning.Models;
using UnityEngine;

namespace Assets.Scripts.MachineLearning
{
    public class DataNormalization
    {
        public void StartProcess()
        {
            // Check for new files for processing
            List<string> rawFiles = Directory.GetFiles(MLConstants.RawDataFilePath).ToList();

            if (rawFiles.Any())
            {
                foreach (string file in rawFiles)
                {
                    // Begin normalization of data
                    bool fileSuccessful = NormalizeData(file);

                    // Move files to archive
                    if (fileSuccessful)
                    {
                        string fileName = Path.GetFileName(file);
                        File.Move(file, $"{MLConstants.RawDataArchiveFilePath}/{fileName}");
                    }
                }

                // Kick off next step

            }
        }

        private bool NormalizeData(string filePath)
        {
            bool wasSuccessful = false;

            try
            {
                // Read in data
                string rawFileData = File.ReadAllText(filePath);
                RawGameState rawGameState = JsonUtility.FromJson<RawGameState>(rawFileData);

                // Normalize each entry
                var normalizedData = Normalize(rawGameState);

                // Save to new file

            }
            catch (Exception ex)
            {
                // TODO
            }

            return wasSuccessful;
        }

        private NormalizedGameState Normalize(RawGameState rawGameState)
        {
            var retNormalizedGameState = new NormalizedGameState()
            {
                BotStates = rawGameState.BotStates.Select(state => Normalize(rawGameState, state)).ToList()
            };

            return retNormalizedGameState;
        }

        private NormalizedGameState.BotState Normalize(RawGameState rawGameState, RawGameState.BotState rawBotState)
        {
            float maxScore = GetMaxScore(rawGameState);

            var retNormalizedBotState = new NormalizedGameState.BotState()
            {
                LogGroup = rawBotState.LogGroup,
                //EventType = 0f,
                BotsTeamsScore = NormalizeScore(maxScore, rawBotState.BotsTeamsScore),
                EnemyTeamScore = NormalizeScore(maxScore, rawBotState.EnemyTeamScore),
                TargetPost = rawBotState.TargetPost,
                CommandPostRelativeState = rawBotState.CommandPostRelativeState.Select(state => Normalize(rawBotState.BotsTeam, state)).ToList()
            };

            return retNormalizedBotState;
        }

        private NormalizedGameState.CommandPostState Normalize(TeamType botTeam, RawGameState.CommandPostState rawPostState)
        {
            var retNormalizedPostState = new NormalizedGameState.CommandPostState()
            {
                PostNumber = rawPostState.PostNumber,
                ControllingTeam = NormalizeTeam(botTeam, rawPostState.ControllingTeam),
                DistanceToBot = NormalizeDistance(rawPostState.DistanceToBot),
                AverageDistanceFromBotsTeam = NormalizeDistance(rawPostState.DistanceToBot),
            };

            return retNormalizedPostState;
        }

        //private float NormalizeEvent(LogEventType eventType, TeamType botTeam, TeamType currPostTeam)
        //{
        //    float retNormVal = 0f;

        //    if (eventType == LogEventType.TimerTriggered)
        //    {
        //        retNormVal = 0.5f;
        //    }
        //    else
        //    {
        //        // Post changed sides... need to figure out if this is good/bad

        //    }

        //    return retNormVal;
        //}

        private float NormalizeScore(float maxScore, float score)
        {
            // Using min-max normalization
            float minScore = 0f;
            float retNormVal = (score - minScore) / (maxScore - minScore);

            return retNormVal;
        }

        private float GetMaxScore(RawGameState gameState)
        {
            int maxEnemyScore = gameState.BotStates.Select(state => state.EnemyTeamScore).Max();
            int maxTeamScore = gameState.BotStates.Select(state => state.BotsTeamsScore).Max();

            float retMaxVal = maxEnemyScore > maxTeamScore
                ? maxEnemyScore
                : maxTeamScore;

            return retMaxVal;
        }

        private float NormalizeTeam(TeamType botTeam, TeamType postTeam)
        {
            float retNormVal = 0f;

            if (postTeam == TeamType.Neutral)
            {
                retNormVal = 0.5f;
            }
            else if (botTeam == postTeam)
            {
                retNormVal = 1.0f;
            }

            return retNormVal;
        }

        private float NormalizeDistance(float distance)
        {
            float retNormVal;

            // Attacking/Defending
            if (distance <= 15)
            {
                retNormVal = 1;
            }
            // Otherside of Map
            else if (distance >= 400)
            {
                retNormVal = 0;
            }
            // Calculate with min-max normalization
            else
            {
                int max = 400;
                int min = 15;
                retNormVal = (distance - min) / (max - min);
            }

            return retNormVal;
        }
    }
}
