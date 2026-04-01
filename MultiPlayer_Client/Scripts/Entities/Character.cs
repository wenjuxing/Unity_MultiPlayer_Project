using Assets.InventorySystem;
using Assets.Scripts.U3d_scripts;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Entities
{
    public class Character : Actor
    {
        private Knapsack _knapsack;
        public Knapsack knapsack 
        {
            get
            {
                if (_knapsack == null)
                {
                    _knapsack = new Knapsack(this);
                }
                return _knapsack;
            }
        }
        public Character(NetActor info) : base(info)
        {

        }
        /// <summary>
        /// 角色死亡
        /// </summary>
        /// <param name="old_state"></param>
        /// <param name="new_state"></param>
        public override void OnStateChanged(UnitState old_state, UnitState new_state)
        {
            base.OnStateChanged(old_state, new_state);
            if (IsDeath&&GameApp.character==this)
            {
                (UIManager.Instance.ShowUI<TipType1Panel>(E_UIPanelLayer.Forefront) as TipType1Panel)
                    .Show("战斗失利", "请回城复活!", () =>
                     {
                         ReviveRequest res = new ReviveRequest();
                         res.EntityId = this.entityId;
                         NetClient.Send(res);
                         UIManager.Instance.HideUI("TipType1Panel");
                     });
            }
        }
    }
}
