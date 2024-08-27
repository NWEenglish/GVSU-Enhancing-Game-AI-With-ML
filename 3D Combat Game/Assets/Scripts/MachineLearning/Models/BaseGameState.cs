using System;
using System.IO;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.MachineLearning.Models
{
    [Serializable]
    public class BaseGameState
    {
        public TeamType Team;

        public bool ToSaveFile(string path, string name = null)
        {
            bool wasSuccessful = false;

            try
            {
                string json = JsonUtility.ToJson(this);

                var writer = File.CreateText($"{path}\\{(int)this.Team}-{DateTime.Now.ToFileTime()}.txt");
                writer.Write(json);
                writer.Close();

                wasSuccessful = true;
            }
            catch (Exception ex)
            {
                // TODO
            }

            return wasSuccessful;
        }
    }
}
