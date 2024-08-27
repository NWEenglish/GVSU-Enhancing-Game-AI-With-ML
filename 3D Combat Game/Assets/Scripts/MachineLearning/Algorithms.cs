using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Enums;
using Assets.Scripts.MachineLearning.Models;
using UnityEngine;

namespace Assets.Scripts.MachineLearning
{
    public class Algorithms
    {
        public void StartProcess(TeamType teamToProcess)
        {
            // Check for new normalized data; continue if any present
            List<string> normFiles = Directory.GetFiles(MLConstants.NormalizedDataFilePath).ToList();

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
                List<string> teamLearnedFiles = Directory.GetFiles(MLConstants.LearnedDataFilePath)
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
                        File.Move(normFile, $"{MLConstants.RawDataArchiveFilePath}/{fileName}");
                    }

                    if (currentGeneration % 10 == 0)
                    {
                        List<string> oldLearnedFiles = teamLearnedFiles
                            .Where(fileName => GetGenFromFileName(fileName) <= currentGeneration - 10)
                            .ToList();

                        foreach (string file in oldLearnedFiles)
                        {
                            string fileName = Path.GetFileName(file);
                            File.Move(file, $"{MLConstants.LearnedArchiveaFilePath}/{fileName}");
                        }
                    }
                }
            }
        }

        private NormalizedGameState BackPropagate(NormalizedGameState currentKnowledge, NormalizedGameState gameState)
        {
            // Go in reverse order
            for (int index = gameState.States.Count - 1; index >= 0; index--)
            {
                // Game over
                if (index == gameState.States.Count - 1)
                {
                    TeamType? winner = GetWinner(gameState.States[index].State);
                    int points;

                    // Won
                    if (winner == gameState.Team)
                    {
                        points = 1000;
                    }
                    // Lost
                    else if (winner != gameState.Team)
                    {
                        points = -1000;
                    }
                    // Tie
                    else
                    {
                        points = -20;
                    }

                    gameState.States[index].Reward = points;
                }
            }

            return currentKnowledge;
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
            string fileName = $"{(int)currentKnowledge.Team}-{currentGeneration}.txt";
            bool wasSuccessful = currentKnowledge.ToSaveFile(MLConstants.LearnedDataFilePath, fileName);
            return wasSuccessful;
        }

        private int GetGenFromFileName(string fileName)
        {
            return int.Parse(Regex.Match(fileName, @"(\d*).txt").Value);
        }

        private TeamType GetTeamFromFileName(string fileName)
        {
            int team = int.Parse(Regex.Match(fileName, @"(\d*)-").Value);
            return (TeamType)team;
        }
    }
}
