using GameServer.Database;
using GameServer.Mgr;
using Proto;
using Summer;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using GameServer.InventorySystem;
using Google.Protobuf;
using GameServer.TaskSystem;
using GameServer.DialogueSystem;
using GameServer.StorageSystem;

namespace GameServer.Model
{
    //角色
    public class Character : Actor
    {
        //当前角色的客户端连接
        public Connection conn;
        //当前角色的数据库对象
        public DbCharacter Data;
        //当前玩家的数据库信息
        public DbPlayer dbPlayer;
        //背包
        public Inventory knapsack;
        //存储数据管理器
        public StorageManager storageManager;
        //任务管理器
        public TaskManager taskMgr;
        //对话管理
        public DialogueManager dialogueMgr;
        //玩家角色唯一ID
        public int characterId => Data.Id;
        //玩家Id
        public int playerId;
        public Character (DbCharacter DbChar)
            : base(EntityType.Character,DbChar.JobId,DbChar.Level,new Vector3Int(DbChar.X,DbChar.Y,DbChar.Z),Vector3Int.zero)
        {
            this.Name = DbChar.Name;
            this.Id = DbChar.Id;
            this.info.Name = DbChar.Name;
            this.info.Id = DbChar.Id;
            this.info.Tid = DbChar.JobId;       //单位类型
            this.info.Exp = DbChar.Exp;
            this.info.SpaceId = DbChar.SpaceId;
            this.info.Gold = DbChar.Gold;
            this.info.Hp = DbChar.Hp;
            this.info.Mp = DbChar.Mp;
            this.Data = DbChar;
            this.dbPlayer = DataBase.fsql.Select<DbPlayer>().First();
            //创建背包
            this.knapsack = new Inventory(this);
            //读取数据库背包信息
            knapsack.Init(Data.Knapsack);
            //创建任务管理器
            this.taskMgr = new TaskManager(this);
            //创建对话管理器
            this.dialogueMgr = new DialogueManager(this);
            //创建存储数据管理器
            this.storageManager = new StorageManager(this);
        }
        /// <summary>
        /// 等号运算符重载
        /// </summary>
        /// <param name="r"></param>
        public static implicit operator Character(DbCharacter DbChr)
        {
            return new Character(DbChr);
        }
        /// <summary>
        /// 使用物品
        /// </summary>
        /// <param name="slotIndex"></param>
        public void UseItem(int slotIndex)
        {
            //物品为空或类型错误
            if (!knapsack.TrySlotItem(slotIndex, out var item)) return;
            if (item.ItemType != ItemType.Consumable) return;
            item.amount--;
            //物品用完则重新设置插槽
            if (item.amount<=0)
            {
                knapsack.SetItem(slotIndex,null);
            }
            SendInventory(true);
            //设置物品效果
            if (item.Id==1001)
            {
                this.SetHP(Hp+50);
            }
            else if (item.Id == 1002)
            {
                this.SetMP(Mp + 50);
            }
        }
        /// <summary>
        /// 发送背包信息
        /// </summary>
        /// <param name="v"></param>
        public void SendInventory(bool knapsack=false,bool storage=false,bool equip=false)
        {
            InventoryResponse resp = new InventoryResponse();
            resp.EntityId = entityId;
            if (knapsack) resp.KnapsackInfo = this.knapsack.InventoryInfo;
            conn.Send(resp);
        }
    }
}
