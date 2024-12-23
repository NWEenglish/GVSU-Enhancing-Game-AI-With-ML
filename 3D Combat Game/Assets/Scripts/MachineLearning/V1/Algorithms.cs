﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Enums;
using Assets.Scripts.MachineLearning.Models;
using UnityEngine;

namespace Assets.Scripts.MachineLearning.V1
{
    public class Algorithms
    {
        public const int Version = 1;

        private const double LearningRate = 0.5;
        private const double RiskFactor = 0.95;
        private const double DiscountFactor = 1;

        private NormalizedGameState LoadedLearnedDate = null;

        private void LoadLearnedKnowledge(TeamType team)
        {
            // Check for saved data that's been learned for this team
            string mostRecentFile = Directory.GetFiles(MLConstants.LearnedDataFilePath.Replace(MLConstants.VersionNumberPlacement, Version.ToString()))
                .Where(fileName => GetTeamFromFileName(fileName) == team)
                .OrderByDescending(fileName => GetGenFromFileName(fileName))
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(mostRecentFile))
            {
                LoadedLearnedDate = ReadInGameState(mostRecentFile);
            }
        }

        public List<NormalizedGameState.GameState> GetNextStates(TeamType team, string stateID)
        {
            if (LoadedLearnedDate == null)
            {
                LoadLearnedKnowledge(team);
            }

            int currentRedScore = GetTeamScorePercentile(stateID, TeamType.RedTeam);
            int currentBlueScore = GetTeamScorePercentile(stateID, TeamType.BlueTeam);

            List<NormalizedGameState.GameState> retNextStates = GetApplicableNextStates(LoadedLearnedDate, currentRedScore, currentBlueScore);
            return retNextStates;
        }

        public void StartSaveProcess(TeamType teamToProcess)
        {
            // Check for new normalized data; continue if any present
            List<string> normFiles = Directory.GetFiles(MLConstants.NormalizedDataFilePath.Replace(MLConstants.VersionNumberPlacement, Version.ToString())).ToList();

            if (normFiles.Any())
            {
                // Read in new data
                List<NormalizedGameState> NormGameStates = new List<NormalizedGameState>();

                foreach (string normFile in normFiles)
                {
                    NormalizedGameState normGameState = ReadInGameState(normFile);
                    if (normGameState.Team == teamToProcess)
                    {
                        NormGameStates.Add(normGameState);
                    }
                }

                // Check for saved data that's been learned for this team
                List<string> teamLearnedFiles = Directory.GetFiles(MLConstants.LearnedDataFilePath.Replace(MLConstants.VersionNumberPlacement, Version.ToString()))
                    .Where(fileName => GetTeamFromFileName(fileName) == teamToProcess)
                    .ToList();

                string mostRecentFile = string.Empty;
                NormalizedGameState currentKnowledge = null;
                int currentGeneration = 0;

                if (teamLearnedFiles.Any())
                {
                    mostRecentFile = teamLearnedFiles
                        .OrderByDescending(fileName => GetGenFromFileName(fileName))
                        .First();

                    currentGeneration = GetGenFromFileName(mostRecentFile) + 1;
                    currentKnowledge = ReadInGameState(mostRecentFile);
                }

                // If nothing was read in, instantiate it
                if (currentKnowledge == null)
                {
                    currentKnowledge = new NormalizedGameState()
                    {
                        Team = teamToProcess
                    };
                }

                // Back-propagate and insert new states
                foreach (NormalizedGameState gameState in NormGameStates)
                {
                    currentKnowledge = BackPropagate(currentKnowledge, gameState);
                }

                // Save
                bool saveSuccess = SaveKnowledge(currentKnowledge, currentGeneration);

                // If save success, move normalized data to archive, and batch archive older knowledge files
                if (saveSuccess)
                {
                    foreach (string normFile in normFiles)
                    {
                        string fileName = Path.GetFileName(normFile);
                        File.Move(normFile, $"{MLConstants.NormalizedArchivedFilePath.Replace(MLConstants.VersionNumberPlacement, Version.ToString())}/{fileName}");
                    }

                    if (currentGeneration % 10 == 0)
                    {
                        List<string> oldLearnedFiles = teamLearnedFiles
                            .Where(fileName => GetGenFromFileName(fileName) <= currentGeneration - 10)
                            .ToList();

                        foreach (string file in oldLearnedFiles)
                        {
                            string fileName = Path.GetFileName(file);
                            string filePath = MLConstants.LearnedArchivedFilePath.Replace(MLConstants.VersionNumberPlacement, Version.ToString());

                            File.Move(file, $"{filePath}/{fileName}");
                        }
                    }
                }
            }
        }

        public bool ShouldUseRandomValue()
        {
            var value = Random.Range(0f, 1f);
            return value > RiskFactor;
        }

        private NormalizedGameState BackPropagate(NormalizedGameState currentKnowledge, NormalizedGameState gameState)
        {
            // Go in reverse order
            for (int index = gameState.States.Count - 1; index >= 0; index--)
            {
                NormalizedGameState.GameState currentState = gameState.States[index];
                NormalizedGameState.GameState matchingKnownState = currentKnowledge.States.FirstOrDefault(state => state.StateID == currentState.StateID);

                // Game over
                if (index == gameState.States.Count - 1)
                {
                    TeamType? gameWinner = GetWinner(currentState.StateID);
                    currentState.Value = GameOverPoints(gameWinner, currentKnowledge.Team);
                }
                // Bring knowledge back by learning with Q-Learning
                else
                {
                    // No rewards for entering a new state
                    double reward = 0;

                    // Check if the current state has been seen before
                    double currentValue = 0;
                    if (matchingKnownState != null)
                    {
                        currentValue = matchingKnownState.Value;
                    }

                    // Find the next best state
                    double maxNextValue = BestNextStateValue(currentKnowledge, currentState.StateID);

                    // Calculate this state's value
                    double newStateValue = GetNewStateValue(currentValue, reward, maxNextValue);
                    currentState.Value = newStateValue;
                }

                // Update or append to current knowledge
                if (matchingKnownState == null)
                {
                    currentKnowledge.States.Add(currentState);
                }
                else
                {
                    matchingKnownState.Value = currentState.Value;
                }
            }

            return currentKnowledge;
        }

        private double BestNextStateValue(NormalizedGameState currentKnowledge, string currentStateID)
        {
            double retBestNextStateValue = 0;

            int currentRedScore = GetTeamScorePercentile(currentStateID, TeamType.RedTeam);
            int currentBlueScore = GetTeamScorePercentile(currentStateID, TeamType.BlueTeam);

            List<NormalizedGameState.GameState> applicableGameStates = GetApplicableNextStates(currentKnowledge, currentRedScore, currentBlueScore);

            retBestNextStateValue = applicableGameStates
                .OrderByDescending(state => state.Value)
                .FirstOrDefault()?.Value ?? 0;

            return retBestNextStateValue;
        }

        private List<NormalizedGameState.GameState> GetApplicableNextStates(NormalizedGameState currentKnowledge, int currentRedScore, int currentBlueScore)
        {
            var retNextStates = new List<NormalizedGameState.GameState>();

            if (currentKnowledge != null)
            {
                foreach (NormalizedGameState.GameState gameState in currentKnowledge.States)
                {
                    int thisRedScore = GetTeamScorePercentile(gameState.StateID, TeamType.RedTeam);
                    int thisBlueScore = GetTeamScorePercentile(gameState.StateID, TeamType.BlueTeam);

                    // We can't go backwards in score, exclude these
                    if (thisRedScore < currentRedScore || thisBlueScore < currentBlueScore)
                    {
                        continue;
                    }

                    // If within 0-1 percentiles, include them
                    if (thisRedScore == currentRedScore || thisRedScore == currentRedScore + 1)
                    {
                        if (thisBlueScore == currentBlueScore || thisBlueScore == currentBlueScore + 1)
                        {
                            retNextStates.Add(gameState);
                        }
                    }
                }
            }

            return retNextStates;
        }

        private int GetTeamScorePercentile(string stateID, TeamType team)
        {
            int index = team == TeamType.RedTeam ? 0 : 1;
            string rawValue = stateID.ElementAt(index).ToString();
            int retScore = rawValue == "*" ? 10 : int.Parse(rawValue);

            return retScore;
        }

        private double GetNewStateValue(double currentValue, double reward, double maxNextValue)
        {
            // Q-Learning Algorithm
            double retNewValue = currentValue + (LearningRate * (reward + (DiscountFactor * maxNextValue)) - currentValue);
            return retNewValue;
        }

        private int GameOverPoints(TeamType? gameWinner, TeamType currentTeam)
        {
            int retPoints;

            // Won
            if (gameWinner == currentTeam)
            {
                retPoints = 1000;
            }
            // Tie
            else if (gameWinner == null)
            {
                retPoints = -20;
            }
            // Lost
            else
            {
                retPoints = -1000;
            }

            return retPoints;
        }

        private TeamType? GetWinner(string gameStateID)
        {
            // Invalid state or no winner
            if (string.IsNullOrEmpty(gameStateID) || !gameStateID.Contains('*'))
            {
                return null;
            }
            // Tie
            else if (gameStateID.Contains("**"))
            {
                return null;
            }
            else if (gameStateID.ElementAt(0) == '*')
            {
                return TeamType.RedTeam;
            }
            else
            {
                return TeamType.BlueTeam;
            }
        }

        private NormalizedGameState ReadInGameState(string filePath)
        {
            string normFileData = File.ReadAllText(filePath);
            NormalizedGameState normGameState = JsonUtility.FromJson<NormalizedGameState>(normFileData);

            return normGameState;
        }

        private bool SaveKnowledge(NormalizedGameState currentKnowledge, int currentGeneration)
        {
            string fileName = $"{(int)currentKnowledge.Team}-{currentGeneration}";
            bool wasSuccessful = currentKnowledge.ToSaveFile(MLConstants.LearnedDataFilePath.Replace(MLConstants.VersionNumberPlacement, Version.ToString()), fileName);
            return wasSuccessful;
        }

        private int GetGenFromFileName(string fileName)
        {
            string genStr = Regex.Match(fileName, @"(\d*)\.txt").Groups[1].Value;
            return int.Parse(genStr);
        }

        private TeamType GetTeamFromFileName(string fileName)
        {
            string teamStr = Regex.Match(fileName, @"(\d+)-").Groups[1].Value;
            int team = int.Parse(teamStr);
            return (TeamType)team;
        }
    }
}
