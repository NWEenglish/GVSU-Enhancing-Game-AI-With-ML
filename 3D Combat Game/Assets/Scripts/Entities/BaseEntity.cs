using Assets.Scripts.Enums;
using Assets.Scripts.Gamemode.Conquest;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public abstract class BaseEntity : MonoBehaviour
    {
        public abstract TeamType Team { get; protected set; }

        public ConquestGameLogic ConquestGameLogic { get; protected set; }

        protected void BaseStart()
        {
            ConquestGameLogic = GameObject.FindObjectOfType<ConquestGameLogic>();
        }
    }
}
