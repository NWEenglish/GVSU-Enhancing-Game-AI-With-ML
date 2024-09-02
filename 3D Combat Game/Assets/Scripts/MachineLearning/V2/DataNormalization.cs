using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.MachineLearning.Helpers;
using Assets.Scripts.MachineLearning.Models;
using UnityEngine;

namespace Assets.Scripts.MachineLearning.V2
{
    public class DataNormalization
    {
        private const int Version = 2;
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
                RawGameState rawGameState = JsonUtility.FromJson<RawGameState>(rawFileData);

                // Normalize each entry
                NormalizedGameState normalizedData = Normalize(rawGameState);

                // Save to new file
                wasSuccessful = normalizedData.ToSaveFile(MLConstants.NormalizedDataFilePath.Replace(MLConstants.VersionNumberPlacement, Version.ToString()));
            }
            catch (Exception ex)
            {
                // TODO
            }

            return wasSuccessful;
        }

        private NormalizedGameState Normalize(RawGameState rawGameState)
        {
            NormalizedGameState retNormalizedGameState = new NormalizedGameState()
            {
                Team = rawGameState.Team
            };

            retNormalizedGameState.States = rawGameState.States.Select(state => new NormalizedGameState.GameState()
            {
                StateID = GameStateHelper.GetGameState(Version, state.MaxScore, state.RedTeamScore, state.BlueTeamScore, new Dictionary<int, TeamType>()
                {
                    { 1, state.Post1 },
                    { 2, state.Post2 },
                    { 3, state.Post3 },
                    { 4, state.Post4 },
                    { 5, state.Post5 },
                }),
                Value = 0
            }).ToList();

            return retNormalizedGameState;
        }
    }
}
