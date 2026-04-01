using GameClient;
using GameClient.Battle;
using GameClient.Entities;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.U3d_scripts
{
   public class GameApp
    {
        //角色全局
        public static Character character;
        //目标全局
        public static Actor Target;
        //玩家Id
        public static int playerId;
        //玩家货币
        public static int currency;
        public static Vec3 TargetPoint;
        //现今技能
        public static Skill CurrSkill;
        //是否正在输入
        public static bool IsInputting => UIManager.Instance.Find<SimpleChatPanel>().inputField.isFocused;
        public static void LoadScene(int spaceId)
        {
            //加载地图
            var spaceDefine = DataManager.Instance.Spaces[spaceId];
            SceneManager.LoadScene(spaceDefine.Resource);
        }
        public static void AsyncLoadScene(int spaceId,Action<AsyncOperation> onCompletedEvent)
        {
            //加载地图
            var spaceDefine = DataManager.Instance.Spaces[spaceId];
            AsyncOperation operation =SceneManager.LoadSceneAsync(spaceDefine.Resource);
            operation .completed += onCompletedEvent;
        }
        /// <summary>
        /// 自动选择目标
        /// </summary>
        public static void SelectTarget()
        {
            Debug.Log("选择目标!");
            Target = Game.RangeUnit(character.Position, 12000)
                 .OrderBy(e => Vector3.Distance(character.Position, e.Position))
                 .FirstOrDefault(e => e.entityId != character.entityId&&!e.IsDeath);
        }
        /// <summary>
        /// 向服务器发送释放技能请求
        /// </summary>
        /// <param name="skill"></param>
        public static void Spell(Skill skill)
        {
            if (Target==null&&skill.IsUnitTarget)
            {
                SelectTarget();
                if (Target == null)
                {
                    Debug.Log("请选择施法对象！！！");
                    return;
                }
            }
            //请求释放技能
            SpellRequest req = new SpellRequest() { Info = new CastInfo() };
            req.Info.CasterId = character.entityId;
            req.Info.SkillId = skill.Define.ID;
            if (skill.IsUnitTarget)
            {
                req.Info.TargetId = Target.entityId;
            }
            if (skill.IsPointTarget)
            {
                req.Info.Targetloc = V3.ToVec3(Target.Position);
            }
            NetClient.Send(req);
        }
    }
}
