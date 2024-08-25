using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.MachineLearning.Models
{
    public class BaseGameState
    {
        public bool ToSaveFile(string path, string name = null)
        {
            bool wasSuccessful = false;

            try
            {
                string json = JsonUtility.ToJson(this);

                var writer = File.CreateText($"{path}\\{DateTime.Now.ToFileTime()}.txt");
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
