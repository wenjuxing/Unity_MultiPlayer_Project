using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Summer.Network;
using Proto;
using Serilog;
using GameServer.Model;
using GameServer.Mgr;
using Summer;
using GameServer.Database;
using GameServer.Core;
using GameServer.TaskSystem;

namespace GameServer.Service
{
    /// <summary>
    /// 玩家服务
    /// 注册，登录，创建角色，进入游戏
    /// </summary>
    public class UserService : Singleton<UserService>
    {
        public void Start()
        {
            MessageRouter.Instance.Subscribe<GameEnterRequest>(_GameEnterRequest);
            MessageRouter.Instance.Subscribe<UserLoginRequest>(_UserLoginRequest);
            MessageRouter.Instance.Subscribe<UserRegisterRequest>(_UserRegisterRequest);
            MessageRouter.Instance.Subscribe<CharacterCreateRequest>(_CharacterCreateRequest);
            MessageRouter.Instance.Subscribe<CharacterListRequest>(_CharacterListRequest);
            MessageRouter.Instance.Subscribe<CharacterDeleteRequest>(_CharacterDeleteRequest);
            MessageRouter.Instance.Subscribe<ReviveRequest>(_ReviveRequest);
            MessageRouter.Instance.Subscribe<PickupItemRequest>(_PickupItemRequest);
            MessageRouter.Instance.Subscribe<InventoryRequest>(_InventoryRequest);
            MessageRouter.Instance.Subscribe<ItemPlacementRequest>(_ItemPlacementRequest);
            MessageRouter.Instance.Subscribe<ItemDiscardRequest>(_ItemDiscardRequest);
            MessageRouter.Instance.Subscribe<ItemUseRequest>(_ItemUseRequest);
            MessageRouter.Instance.Subscribe<AcceptTaskRequest>(_AcceptTaskRequest);
            MessageRouter.Instance.Subscribe<TaskProUpdateRequest>(_TaskProUpdateRequest);
            MessageRouter.Instance.Subscribe<TaskDataRequest>(_TaskDataRequest);
            MessageRouter.Instance.Subscribe<AbandonTaskRequest>(_AbandonTaskRequest);
            MessageRouter.Instance.Subscribe<SubmitTaskRequest>(_SubmitTaskRequest);
            MessageRouter.Instance.Subscribe<DialogueRequest>(_DialogueRequest);
            MessageRouter.Instance.Subscribe<DialogueUpdateRequest>(_DialogueUpdateRequest);
            MessageRouter.Instance.Subscribe<GetCurrencyRequest>(_GetCurrencyRequest);
            MessageRouter.Instance.Subscribe<BuyGoodsRequest>(_BuyGoodsRequest);
            MessageRouter.Instance.Subscribe<AddItemRequest>(_AddItemRequest);
        }
      

        #region 购物信息
        /// <summary>
        /// 购物请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _BuyGoodsRequest(Connection conn, BuyGoodsRequest msg)
        {
            Log.Information("购物请求{0}", msg);

            Character chr = conn.Get<Session>().character;
            BuyGoodsResponse resp = new BuyGoodsResponse();
            chr.storageManager.GoodsConfigs.TryGetValue(msg.GoodsId, out ShopDefine goodItem);
            var dbPlayer = DataBase.fsql.Select<DbPlayer>().Where(t => t.Id == msg.PlayerId).First();
            //合法性检验
            if (chr == null)
            {
                resp.ErrorMsg = "角色不存在!";
                resp.Success = false;
            }
            else if (chr.playerId != msg.PlayerId)
            {
                resp.ErrorMsg = "玩家Id错误!";
                resp.Success = false;
            }
            else if (goodItem == null)
            {
                resp.ErrorMsg = "该商品不存在!";
                resp.Success = false;
            }
            else if (dbPlayer.Coin < goodItem.Price)
            {
                resp.ErrorMsg = "余额不足!";
                resp.Success = false;
            }
            else if (goodItem.IsLimitPurchase && chr.storageManager.GoodsItemsMap.ContainsKey(goodItem.id))
            {
                resp.ErrorMsg = "本商品限购一个!";
                resp.Success = false;
            }
            else
            {
                resp.Success = true;
                //发送剩余货币响应消息
                GetCurrencyResponse GetCoinresp = new GetCurrencyResponse();
                GetCoinresp.PlayerId = msg.PlayerId;
                GetCoinresp.CoinAmount = dbPlayer.Coin - goodItem.Price;
                conn.Send(GetCoinresp);

                //把购买的商品和余额存入数据库
                chr.storageManager.UpdateStorageInfo(msg.GoodsId);
                chr.storageManager.UpdateCurrencyInfo(msg.GoodsId);
            }
            //发送购买响应信息          
            resp.PlayerId = msg.PlayerId;
            resp.GoodsId = msg.GoodsId;
            resp.StorageInfo = chr.storageManager.storageInfo;
            conn.Send(resp);
        }
        #endregion

        #region 玩家信息
        /// <summary>
        /// 货币信息请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _GetCurrencyRequest(Connection conn, GetCurrencyRequest msg)
        {
            Log.Information("货币信息请求:{0}", msg);

           var chr=conn.Get<Session>().character;
            //合法性检验
            if (chr == null) return;
            if (chr.playerId != msg.PlayerId) return;

            //从数据库中获取玩家的货币信息
          DbPlayer dbPlayer= DataBase.fsql.Select<DbPlayer>()
                .Where(db => db.Id == msg.PlayerId).First();

            //发送响应信息
            GetCurrencyResponse resp = new GetCurrencyResponse()
            {
                PlayerId = msg.PlayerId,
                CoinAmount = dbPlayer.Coin
            };
            conn.Send(resp);
        }
        #endregion

        #region 对话信息

        /// <summary>
        /// 对话信息请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _DialogueRequest(Connection conn, DialogueRequest msg)
        {
            Log.Information("对话信息请求:{0}", msg);

            var chr = conn.Get<Session>().character;
            DialogueResponse resp = new DialogueResponse();

            //合法性检验
            if (chr == null) 
            {
                resp.ErrorMsg = $"游戏角色不存在!";
            }
            if (chr.playerId != msg.Id)
            {
                resp.ErrorMsg = $"用户Id不存在，Id={msg.Id}";
            }
            if (!DataManager.Instance.dialogueMains.ContainsKey(msg.ChapterId))
            {
                resp.ErrorMsg = $"对话章节不存在，Id={msg.ChapterId}";
            }
            if (!DataManager.Instance.dialogueGroups.ContainsKey(msg.GroupId)) 
            {
                resp.ErrorMsg = $"对话组不存在，Id={msg.GroupId}";
            }
            //判断是否完成该对话
            if (chr.dialogueMgr.dialogueData.ChapterIds.Contains(msg.ChapterId)) 
            {
                resp.IsCompleted = true;
                resp.ErrorMsg = $"对话已完成！";
            }
            else resp.IsCompleted = false;

            //发送响应信息
            resp.Id = chr.playerId;
            resp.ChapterId = msg.ChapterId;
            resp.GroupId = msg.GroupId;
            conn.Send(resp);
        }
        /// <summary>
        /// 对话进度更新请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _DialogueUpdateRequest(Connection conn, DialogueUpdateRequest msg)
        {
            Log.Information("对话进度更新:{0}", msg);

            Character chr = conn.Get<Session>().character;
            DialogueUpdateResponse resp = new DialogueUpdateResponse();

            //合法性检验
            if (chr == null)
            {
                resp.ErrorMsg = $"游戏角色不存在!";
                resp.Success = false;
            }
            if (chr.playerId != msg.Id)
            {
                resp.ErrorMsg = $"用户Id不存在，Id={msg.Id}";
                resp.Success = false;
            }
            if (!DataManager.Instance.dialogueMains.ContainsKey(msg.ChapterId))
            {
                resp.ErrorMsg = $"对话章节不存在，Id={msg.ChapterId}";
                resp.Success = false;
            }
            if (!DataManager.Instance.dialogueGroups.ContainsKey(msg.GroupId))
            {
                resp.ErrorMsg = $"对话组不存在，Id={msg.GroupId}";
                resp.Success = false;
            }
            //判断是否完成该对话
            if (chr.dialogueMgr.dialogueData.ChapterIds.Contains(msg.ChapterId)
                &&chr.dialogueMgr.dialogueData.GroupIds.Contains(msg.GroupId))
            {
                resp.Success = false;
                resp.ErrorMsg = $"对话已完成！";
            }
            else 
            {
                resp.Success = true;
                chr.dialogueMgr.UpdateDialogueData(msg.Id, msg.GroupId, msg.ChapterId);
            }
            resp.Id = msg.Id;
            conn.Send(resp);
        }
        #endregion

        #region 任务信息
        /// <summary>
        /// 任务更新请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _TaskProUpdateRequest(Connection conn, TaskProUpdateRequest msg)
        {
            Log.Information("任务进度更新请求:{0}", msg);

            //任务存在且未领取奖励？
            var chr = conn.Get<Session>().character;
            if (!chr.taskMgr.TaskItems.TryGetValue(msg.TaskId, out var taskItem)) return;
            if (taskItem.state == TaskState.Finished) return;

            //计算新的进度值
            chr.taskMgr.TaskItems[msg.TaskId].progressTargets.TargetValue += msg.CurrentProgressValues;
            var value = chr.taskMgr.TaskItems[msg.TaskId].progressTargets.TargetValue;

            DataManager.Instance.Init();
            var define= DataManager.Instance.Tasks[msg.TaskId];

            int realValue = Math.Min(value, define.ProgressTargets.TargetValue);

            //发送响应数据
            TaskProUpdateResponse resp = new TaskProUpdateResponse()
            {
                TaskId = msg.TaskId,
                CurrentProgressValues = realValue,
                Success = true,
            };
            //任务是否完成？
            if (realValue == define.ProgressTargets.TargetValue)
            {
                resp.IsCompleted = true;
                chr.taskMgr.TaskItems[msg.TaskId].state = TaskState.Completed;
            }
            else resp.IsCompleted = false;
            //更新任务进度到数据库
             chr.taskMgr.UpdateTaskData
                 (msg.TaskId, true, resp.IsCompleted, false, realValue);

            conn.Send(resp);
        }
        /// <summary>
        /// 任务拒绝请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _AbandonTaskRequest(Connection conn, AbandonTaskRequest msg)
        {
            Log.Information("拒绝任务请求:{0}", msg);

            //满足任务存在和任务可放弃？
            var chr = conn.Get<Session>().character;
            if (!chr.taskMgr.TaskItems.TryGetValue(msg.TaskId, out var taskItem)) return;
            if (!taskItem.canAbandon) return;

            //字典移除数据 数据库标记为放弃状态
            chr.taskMgr.TaskItems.TryRemove(msg.TaskId, out var task);
            chr.taskMgr.AbandonTaskData(msg.TaskId);

            //响应消息
            AbandonTaskResponse resp = new AbandonTaskResponse()
            {
                TaskId = msg.TaskId,
                Success = true
            };
            conn.Send(resp);
        }
        /// <summary>
        /// 任务信息请求
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void _TaskDataRequest(Connection conn, TaskDataRequest msg)
        {
            var chr = conn.Get<Session>().character;
            if (chr == null) return;

            TaskDataResponse resp = new TaskDataResponse();
            foreach (var task in chr.taskMgr.TaskItems)
            {
                resp.TaskInfo.Add(task.Value.NetTaskInfo);
            }
            conn.Send(resp);
        }
        /// <summary>
        /// 接取任务请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _AcceptTaskRequest(Connection conn, AcceptTaskRequest msg)
        {
            Log.Information("任务接取请求:{0}", msg);
            AcceptTaskResponse resp = new AcceptTaskResponse()
            {
                TaskId = msg.TaskId,
                TaskState = Proto.TaskState.InProgress
            };
            var chr = conn.Get<Session>().character;
            if (chr.taskMgr.TaskItems.ContainsKey(msg.TaskId))
            {
                resp.Success = true;
                 chr.taskMgr.UpdateTaskData(msg.TaskId, true, false, false, 0);
            }
            else
            {
                resp.Success = false;
                resp.ErrorMsg = "任务不存在于字典中";
            }
            conn.Send(resp);
        }
        /// <summary>
        /// 领取奖励请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _SubmitTaskRequest(Connection conn, SubmitTaskRequest msg)
        {
            var chr = conn.Get<Session>().character;
            if (chr == null) return;
            if (!chr.taskMgr.TaskItems.TryGetValue(msg.TaskId, out TaskItem taskItem)) return;

            //响应信息
            SubmitTaskResponse resp = new SubmitTaskResponse()
            {
                TaskId = msg.TaskId,
                Rewards = new TaskReward()
                {
                    RewardType = (Proto.RewardType)taskItem.taskRewards.RewardType,
                    RewardId = taskItem.taskRewards.RewardId,
                    Count = taskItem.taskRewards.Count
                }
            };
            //检验进度通过
            if (taskItem.state == TaskState.Completed)
            {
                chr.taskMgr.UpdateTaskData(msg.TaskId, true, true, true);

                //检验背包空间
                if(chr.knapsack.FindEmptyIndex()>0
                    ||chr.knapsack.FindSameItemAndNotFull(taskItem.taskRewards.RewardId) != null)
                {
                    //添加奖励到背包
                    chr.knapsack.AddItem(taskItem.taskRewards.RewardId, taskItem.taskRewards.Count);
                    resp.Success = true;
                }
                else
                {
                    resp.Success = false;
                    resp.ErrorMsg = "背包空间不足！";
                }
                
            }
            //检验进度失败
            else
            {
                resp.Success = false;
                resp.ErrorMsg = "任务未完成！";
            }
            conn.Send(resp);
        }
        #endregion

        #region 背包信息
        /// <summary>
        /// 使用物品请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _ItemUseRequest(Connection conn, ItemUseRequest msg)
        {
            if (!(Game.GetUnit(msg.EntityId) is Character chr)) return;
            chr.UseItem(msg.SlotIndex);
        }
        /// <summary>
        /// 丢弃物品请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _ItemDiscardRequest(Connection conn, ItemDiscardRequest msg)
        {
            Log.Information("丢弃请求!");
            if (!(Game.GetUnit(msg.EntityId) is Character chr)) return;
            chr.knapsack.Discard(msg.SlotIndex,msg.Count);
            chr.SendInventory(true);
        }
        /// <summary>
        /// 放置物品请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _ItemPlacementRequest(Connection conn, ItemPlacementRequest msg)
        {
            if (!(Game.GetUnit(msg.EntityId) is Character chr)) return;
            //交换物品
            chr.knapsack.Exchange(msg.OriginIndex, msg.TargetIndex);
            //发送背包数据
            chr.SendInventory(true);
        }
        /// <summary>
        /// 背包信息请求
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void _InventoryRequest(Connection conn, InventoryRequest msg)
        {
            if (!(Game.GetUnit(msg.EntityId) is Character chr)) return;
            //发送背包数据
            chr.SendInventory(msg.QueryKnapsack, msg.QueryWarehouse, msg.QueryEquipment);
        }
        /// <summary>
        /// 拾取物品请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _PickupItemRequest(Connection conn, PickupItemRequest msg)
        {
            var chr = conn.Get<Session>()?.character;
            if (chr == null) return;

            var itemEntity =
            Game.RangeUnit(chr.Space.Id, chr.Position, 3000)
                 .OfType<ItemEntity>()
                 .OrderBy(entity => Vector3Int.Distance(entity.Position, chr.Position))
                .FirstOrDefault();

            if (itemEntity == null) return;

            //如果添加失败则结束
            if (!chr.knapsack.AddItem(itemEntity.Item.Id, itemEntity.Item.amount)) return;
            //物品模型移出场景
            chr.Space.EntityLeave(itemEntity);
            EntityManager.Instance.RemoveEntity(itemEntity.Space.Id, itemEntity);
            Log.Information("玩家拾取物品Chr[{0}],背包[{1}]", chr.characterId, chr.knapsack.InventoryInfo);
            //发送背包数据和拾取信息
            PickupItemResponse resp = new PickupItemResponse();
            resp.ItemInfo = new ItemInfo()
            {
                ItemId = itemEntity.Item.Id,
                Amount= itemEntity.Item.amount
            };
            resp.EntityId = chr.entityId;
            resp.KnapsackInfo = chr.knapsack.InventoryInfo;
            conn.Send(resp);
        }
        /// <summary>
        /// 添加物品请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _AddItemRequest(Connection conn, AddItemRequest msg)
        {
            Character chr = conn.Get<Session>().character;

            //合法性检验
            if (chr.playerId != msg.PlayerId) return;
            if (!DataManager.Instance.Items.ContainsKey(msg.ItemId)) return;
            //如果添加失败则返回
            if (!chr.knapsack.AddItem(msg.ItemId, msg.Amount)) return;
            //发送添加响应消息
            chr.SendInventory();
        }
        #endregion

        /// <summary>
        /// 复活请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _ReviveRequest(Connection conn, ReviveRequest msg)
        {
            //传入的连接等于角色的连接？
            var chr = conn.Get<Session>().character;
            if (chr is Character&&chr.IsDeath&&chr.conn==conn)
            {
                //设置复活点
                var space = SpaceManager.Instance.GetSpace(1);
                chr.Revive();
                chr.TelePortSpace(space,Vector3Int.zero);
            }
        }

        /// <summary>
        /// 角色删除请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _CharacterDeleteRequest(Connection conn, CharacterDeleteRequest msg)
        {
            //删除数据中对应玩家的角色id
            int id = conn.Get<Session>().dbPlayer.Id;
            DataBase.fsql.Delete<DbCharacter>()
                .Where(t => t.PlayerId == id)
                .Where(t => t.Id == msg.CharacterId)
                .ExecuteAffrows();
            //发送删除成功响应
            CharacterDeleteResponse resp = new CharacterDeleteResponse();
            resp.Success = true;
            resp.Message = "删除成功";
            conn.Send(resp); 
        }

        /// <summary>
        /// 角色列表请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _CharacterListRequest(Connection conn, CharacterListRequest msg)
        {
            Log.Information("角色列表"+msg);
           var list=DataBase.fsql.Select<DbCharacter>()
                .Where(t=>t.PlayerId==conn.Get<Session>().dbPlayer.Id).ToList();
            CharacterListResponse resplist = new CharacterListResponse();
            foreach (var item in list)
            {
                resplist.ActorList.Add(new NetActor() {
                    Id=item.Id,
                    Name=item.Name,
                    Tid=item.JobId,
                    Level=item.Level,
                    Exp=item.Exp,
                    SpaceId=item.SpaceId,
                    Gold=item.Gold
                });
            }
            conn.Send(resplist);
        }

        /// <summary>
        /// 创建角色请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _CharacterCreateRequest(Connection conn, CharacterCreateRequest msg)
        {
            Log.Information("创建角色：{0}",msg);
            //创建角色信息
            var player = conn.Get<Session>().dbPlayer;
            ChracterCreateResponse resp = new ChracterCreateResponse();
            //防止跳过登录直接创建角色
            if (player==null)
            {
                resp.Success = false;
                resp.Message = "没有登录无法创建角色";
                conn.Send(resp);
                return;
            }

            //获取创建角色的数量
           long roleCount=DataBase.fsql.Select<DbCharacter>()
                .Where(t => t.PlayerId.Equals(player.Id)).Count();
            if (roleCount >= 4)
            {
                resp.Success = false;
                resp.Message = "角色数量超额";
                conn.Send(resp);
                return;
            }

            //判断角色名是否为空
            if (string.IsNullOrEmpty(msg.Name)) return;

            //判断角色名字长度是否合法
            string name = msg.Name.Trim();
            if (name.Length > 7) 
            {
                resp.Success = false;
                resp.Message = "名字长度不得超过七位";
                conn.Send(resp);
                return;
            }

            //检查角色名称是否存在
            if (DataBase.fsql.Select<DbCharacter>().Where(t => t.Name.Equals(name)).Count() > 0)
            {
                resp.Success = false;
                resp.Message = "角色名字已存在";
                conn.Send(resp);
                return;
            }

            //创建数据
            DbCharacter db = new DbCharacter()
            {
                Name = msg.Name,
                JobId = msg.JobType,
                PlayerId = player.Id,
                Hp=100,Mp=100,SpaceId=2,Level=1,Exp=0,Gold=0,
                X= 114461,Y= 7083,Z= 89917
            };
            //把角色信息存入数据库
           int aff= DataBase.fsql.Insert(db).ExecuteAffrows();
            if (aff>0)
            {
                resp.Success = true;
                resp.Message = "角色创建成功!";
                conn.Send(resp);
            }
        }

        /// <summary>
        /// 注册请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _UserRegisterRequest(Connection conn, UserRegisterRequest msg)
        {
            var dbPlayer = DataBase.fsql.Select<DbPlayer>()
                .Where(p => p.Username == msg.Username)
                .Where(p => p.Password == msg.Password)
                .First();

            //把玩家Id存入角色中
            conn.Get<Session>().character.playerId = dbPlayer.Id;

            UserRegisterResponse rep = new UserRegisterResponse();
            if (dbPlayer != null)
            {
                rep.Success = false;
                rep.Message = "账号已存在";
            }
            else
            {
                if (msg.Username!=""&&msg.Password!="")
                {
                    //创建一条DbPlayer数据并存入数据库
                    var newPlayer = new DbPlayer() { Username = msg.Username, Password = msg.Password };
                    DataBase.fsql.Insert(newPlayer).ExecuteAffrows();
                    rep.Success = true;
                    rep.Message = "注册成功";
                }
                else
                {
                    rep.Success = false;
                    rep.Message = "输入为空！";
                }
               
            }
            conn.Send(rep);
        }
        /// <summary>
        /// 登录请求
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void _UserLoginRequest(Connection conn, UserLoginRequest msg)
        {
           var dbPlayer= DataBase.fsql.Select<DbPlayer>()
                .Where(p => p.Username == msg.Username)
                .Where(p => p.Password == msg.Password)
                .First();
            UserLoginResponse rep = new UserLoginResponse();
            if (dbPlayer!=null)
            {
                rep.Success = true;
                rep.Message = "登录成功";
                //把登录状态存入Session
                conn.Get<Session>().dbPlayer = dbPlayer;
                rep.PlayerId = conn.Get<Session>().dbPlayer.Id;            
            }
            else
            {
                rep.Success = false;
                rep.Message = "账号或者密码错误";
            }
            conn.Send(rep);
        }
        /// <summary>
        /// 进入游戏请求
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void _GameEnterRequest(Connection conn, GameEnterRequest msg)
        {
            Log.Information("有玩家进入游戏"+ conn);
            //获取玩家id
            var player = conn.Get<Session>().dbPlayer;
            //查询数据库角色
          var dbRole = DataBase.fsql.Select<DbCharacter>()
                .Where(t => t.PlayerId == player.Id)
                .Where(t => t.Id == msg.CharacterId)
                .First();
            //把数据库角色转化为游戏角色并存储在角色管理器中
            Character chr = CharacterManager.Instance.Create(dbRole);
            chr.conn = conn;
            //把角色存入Session当中
            chr.conn.Get<Session>().character = chr;
            //把玩家Id存入角色中
            chr.conn.Get<Session>().character.playerId = player.Id;
            //初始化对话信息
            chr.conn.Get<Session>().character.dialogueMgr.Init();
            //将新角色加入地图
            var space = SpaceService.Instance.GetSpace(dbRole.SpaceId);
            space.EntityEnter(chr);
        }

    }
}
