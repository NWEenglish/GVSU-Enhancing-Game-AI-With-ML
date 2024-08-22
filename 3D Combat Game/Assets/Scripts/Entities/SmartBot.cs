using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class SmartBot : BaseBot
    {
        private MasterSmartBotLogic MasterLogic;

        public void InitValues(TeamType team)
        {
            BaseBotStart(team);
        }

        private void Start()
        {
            MasterLogic = GameObject.FindObjectOfType<MasterSmartBotLogic>();
            MasterLogic.SubscribeTo(this);
        }
    }
}
