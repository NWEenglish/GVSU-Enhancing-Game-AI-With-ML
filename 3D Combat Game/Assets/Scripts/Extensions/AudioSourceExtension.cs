using System;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class AudioSourceExtension
    {
        public static bool TryPlay(this AudioSource audioSource)
        {
            bool wasSuccessful = false;

            try
            {
                audioSource.Play();
                wasSuccessful = true;
            }
            catch (Exception ex)
            {
                // TODO - Add logging
            }

            return wasSuccessful;
        }
    }
}
