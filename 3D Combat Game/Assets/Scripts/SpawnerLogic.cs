using Assets.Scripts.Entities;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts
{
    public class SpawnerLogic : MonoBehaviour
    {
        [SerializeField]
        private TeamType Team;

        private void Start()
        {
            var spawnableObjects = GameObject.Find(Constants.SpawnableItems);
            var basicBot = spawnableObjects.GetComponentInChildren<BasicBot>(true);

            BasicBot newBot = Instantiate(basicBot, this.transform);
            newBot.InitValues(Team);
            newBot.gameObject.SetActive(true);
        }
    }
}
