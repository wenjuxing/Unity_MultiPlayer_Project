using FreeSql;
using GameServer.Database;
using GameServer.Model;
using Google.Protobuf;
using Summer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
    /// <summary>
    /// 统一管理全部角色（创建 移除 获取）
    /// </summary>
    public class CharacterManager : Singleton<CharacterManager>
    {
        //游戏里全部的角色
        //使用线程安全字典，支持多线程
        private ConcurrentDictionary<int, Character> Characters = new ConcurrentDictionary<int, Character>();
        //获取数据库中角色表
        IBaseRepository<DbCharacter> repo = DataBase.fsql.GetRepository<DbCharacter>();
        public CharacterManager() 
        {
            //每隔一段时间保存角色信息到数据库中
            Scheduler.Instance.AddTask(Save,5);
        }
        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="DbChr"></param>
        /// <returns></returns>
        public Character Create(DbCharacter DbChr)
        {
            Character chr = new Character(DbChr);
            Characters[chr.Id] = chr;

            //把角色Entity和spaceId存入Entity管理器中
            EntityManager.Instance.AddEntity(DbChr.SpaceId,chr);
            return chr;
        }
        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="chrId"></param>
        public void Remove(int chrId)
        {
            Character chr;
            if (Characters.TryRemove(chrId, out chr))
            {
                EntityManager.Instance.RemoveEntity(chr.Data.SpaceId,chr);
            }
        }
        /// <summary>
        /// 获取角色
        /// </summary>
        /// <param name="chrId"></param>
        /// <returns></returns>
        public Character GetCharacter(int chrId)
        {
            if (Characters.TryGetValue(chrId, out Character chr)) return chr;
            else return null;
        }
        /// <summary>
        /// 清空所有角色
        /// </summary>
        public void Clear()
        {
            Characters.Clear();
        }
        /// <summary>
        /// 保存角色信息到数据库
        /// </summary>
        private void Save()
        {
            foreach (var chr in Characters.Values)
            {
                //把Character信息赋值给DbCharacter
                chr.Data.X = chr.Position.x;
                chr.Data.Y = chr.Position.y;
                chr.Data.Z = chr.Position.z;
                //把角色背包信息存入数据库
                chr.Data.Knapsack = chr.knapsack.InventoryInfo.ToByteArray();
                repo.UpdateAsync(chr.Data);            
            }
        }
    }
}
