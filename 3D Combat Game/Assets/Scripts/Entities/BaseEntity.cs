using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public abstract class BaseEntity : MonoBehaviour
    {
        public abstract TeamType Team { get; protected set; }
    }
}
