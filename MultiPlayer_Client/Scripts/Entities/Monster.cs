using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace GameClient.Entities
{
    public class Monster : Actor
    {
        public Monster(NetActor info) : base(info)
        {

        }
        public override void OnStateChanged(UnitState old_state, UnitState new_state)
        {
            base.OnStateChanged(old_state, new_state);
            Game.StartCoroutine(_HideElement());
        }
        IEnumerator _HideElement()
        {
            yield return new WaitForSeconds(3f);
            //如果死透则隐藏
            if (IsDeath)
            {
                renderObj.SetActive(false);
            }
        }
    }
}
