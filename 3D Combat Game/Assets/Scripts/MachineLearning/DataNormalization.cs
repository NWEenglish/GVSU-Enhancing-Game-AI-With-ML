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
        private const int DecimalPlaces = 1;

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
                //RawGameState rawGameState = JsonUtility.FromJson<RawGameState>(rawFileData);
                RawGameState_Master rawGameState = JsonUtility.FromJson<RawGameState_Master>(rawFileData);

                // Normalize each entry
                NormalizedGameState_Master normalizedData = Normalize(rawGameState);

                // Save to new file
                wasSuccessful = normalizedData.ToSaveFile(MLConstants.NormalizedDataFilePath);
            }
            catch (Exception ex)
            {
                // TODO
            }

            return wasSuccessful;
        }

        private NormalizedGameState_Master Normalize(RawGameState_Master rawGameState)
        {
            decimal maxScore = rawGameState.States.Max(state => state.BotsTeamsScore);

            NormalizedGameState_Master retNormalizedGameState = new NormalizedGameState_Master();

            retNormalizedGameState.States = rawGameState.States.Select(state => new NormalizedGameState_Master.GameState()
            {
                State = state.State,
                Reward = state.Reward,
                BotsTeamsScore = NormalizeScore(maxScore, state.BotsTeamsScore).ToString(),
                EnemyTeamScore = NormalizeScore(maxScore, state.EnemyTeamScore).ToString(),
            }).ToList();

            return retNormalizedGameState;
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
            decimal maxScore = GetMaxScore(rawGameState);

            var retNormalizedBotState = new NormalizedGameState.BotState()
            {
                LogGroup = rawBotState.LogGroup,
                EventType = NormalizeEvent(rawBotState.LogEvent),
                BotsTeamsScore = NormalizeScore(maxScore, rawBotState.BotsTeamsScore),
                EnemyTeamScore = NormalizeScore(maxScore, rawBotState.EnemyTeamScore),
                TargetPost = rawBotState.TargetPost,
                Reward = rawBotState.Reward,
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

        private decimal NormalizeEvent(LogEventType logEvent)
        {
            return logEvent switch
            {
                LogEventType.GameOver => 1.0m,
                LogEventType.CommandPostChange => 0.5m,
                _ => 0m
            };
        }

        private decimal NormalizeScore(decimal maxScore, int score)
        {
            // Using min-max normalization
            decimal minScore = 0m;
            decimal normalizedValue = (score - minScore) / (maxScore - minScore);
            decimal retNormVal = Math.Round(normalizedValue, DecimalPlaces, MidpointRounding.AwayFromZero);

            return retNormVal;
        }

        private decimal GetMaxScore(RawGameState gameState)
        {
            int maxEnemyScore = gameState.BotStates.Select(state => state.EnemyTeamScore).Max();
            int maxTeamScore = gameState.BotStates.Select(state => state.BotsTeamsScore).Max();

            decimal retMaxVal = maxEnemyScore > maxTeamScore
                ? maxEnemyScore
                : maxTeamScore;

            return retMaxVal;
        }

        private decimal NormalizeTeam(TeamType botTeam, TeamType postTeam)
        {
            decimal retNormVal = 0m;

            if (postTeam == TeamType.Neutral)
            {
                retNormVal = 0.5m;
            }
            else if (botTeam == postTeam)
            {
                retNormVal = 1.0m;
            }

            return retNormVal;
        }

        private decimal NormalizeDistance(float distance)
        {
            decimal retNormVal;

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
                double normalizedValue = (distance - min) / (max - min);

                retNormVal = (decimal)Math.Round(normalizedValue, DecimalPlaces, MidpointRounding.AwayFromZero);
            }

            return retNormVal;
        }
    }
}
