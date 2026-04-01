using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proto;
using Summer.Network;
using System;
using UnityEngine.SceneManagement;
using Assets.Scripts.U3d_scripts;
using GameClient.Entities;
using GameClient;
using GameServer.Fight;

public class NetStart : MonoBehaviour
{
    [Header("切换场景保存对象")]
    public List<GameObject> KeepAlive;

    [Header("服务器信息")]
    public string host = "127.0.0.1";
    public int port = 32510;
    
    [SerializeField] Text ycText;

    private GameObject hero; //当前的角色
    private HeartBeatRequest beatRequest = new HeartBeatRequest();//心跳包
    private DateTime requestTime=DateTime.MinValue;//请求时间
    private TimeSpan spanTime;//心跳时间差

    void Start()
    {
        //忽略图层的碰撞检测
        Physics.IgnoreLayerCollision(6,6,true);
        //创建UIManager并加入持久化对象列表
        UIManager.Instance.CloseAllUI();
        KeepAlive.Add(GameObject.Find("UIManager"));
        //切换场景不会被销毁
        foreach (var go in KeepAlive)
        {
            DontDestroyOnLoad(go);
        }

        //显示登录界面
        UIManager.Instance.ShowUI<LoginPanel>();

        NetClient.ConnectToServer(host, port);

        MessageRouter.Instance.Subscribe<GameEnterResponse>(_GameEnterResponse);
        MessageRouter.Instance.Subscribe<SpaceEnterResponse>(_SpaceEnterResponse);
        MessageRouter.Instance.Subscribe<SpaceCharactersEnterResponse>(_SpaceCharactersEnterResponse);
        MessageRouter.Instance.Subscribe<SpaceEntitySyncResponse>(_SpaceEntitySyncResponse);
        MessageRouter.Instance.Subscribe<HeartBeatResponse>(_HeartBeatResponse);
        MessageRouter.Instance.Subscribe<SpaceCharacterLeaveResponse>(_SpaceCharacterLeaveResponse);
        MessageRouter.Instance.Subscribe<SpellResponse>(_SpellResponse);
        MessageRouter.Instance.Subscribe<DamageResponse>(_DamageResponse);
        MessageRouter.Instance.Subscribe<PropertyUpdateResponse>(_PropertyUpdateResponse);
        MessageRouter.Instance.Subscribe<ChatResponse>(_ChatResponse);
        MessageRouter.Instance.Subscribe<InventoryResponse>(_InventoryResponse);
        MessageRouter.Instance.Subscribe<PickupItemResponse>(_PickupItemResponse);
        MessageRouter.Instance.Subscribe<GetCurrencyResponse>(_GetCurrencyResponse);
        //发送心跳包
        StartCoroutine(SendHeartMessage());

        SceneManager.LoadSceneAsync("LoginScene");

        //初始化JSON数据
        DataManager.Instance.Init();
        //初始化对话数据
        DialogueDataModel.InitData();

        //注册事件
        Kaiyun.Event.RegisterIn("EnterGame", this, "EnterGame");
        Kaiyun.Event.RegisterIn("ItemPlacement",this, "ItemPlacement");
        Kaiyun.Event.RegisterIn("ItemDiscard", this, "ItemDiscard");
        Kaiyun.Event.RegisterIn("UseItem", this, "UseItem");
        Kaiyun.Event.RegisterIn("AddItemRequest",this, "AddItemRequest");
    }
    /// <summary>
    /// 货币信息响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _GetCurrencyResponse(Connection conn, GetCurrencyResponse msg)
    {
        Debug.Log("货币信息响应"+msg);

        //合法性检验
        if (GameApp.playerId==msg.PlayerId)
        {
            GameApp.currency = msg.CoinAmount;
            //Unity的API只能在主线程中执行
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                UIManager.Instance.Find<ShopPanel>()?.UpdateCurrencyText();
            });          
        }
    }
    /// <summary>
    /// 拾取物品响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _PickupItemResponse(Connection conn, PickupItemResponse msg)
    {
        Debug.Log($"拾取物品:{msg}");
        //角色是否存在
        var chr = GameApp.character;
        if (chr.entityId != msg.EntityId) return;

        //加载背包信息
        if (msg.KnapsackInfo != null)
        {
            chr.knapsack.ReLoad(msg.KnapsackInfo);
            Kaiyun.Event.FireOut("OnKnapsackReloaded");
        }

        // 通知任务系统有物品被拾取
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
        TaskEventCenter.TriggerCollectItem(GameApp.character.entityId, 
            new CollectItemEventArgs(msg.ItemInfo.ItemId, msg.ItemInfo.Amount));
        });
    }
    /// <summary>
    /// 背包信息响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _InventoryResponse(Connection conn, InventoryResponse msg)
    {
        //角色是否进入游戏？
        var chr = GameApp.character;
        if (chr == null || chr.entityId != msg.EntityId) return;
        //加载背包信息
        if (msg.KnapsackInfo != null)
        {
            chr.knapsack.ReLoad(msg.KnapsackInfo);
            Kaiyun.Event.FireOut("OnKnapsackReloaded");
        }
    }

    /// <summary>
    /// 进入地图响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _SpaceEnterResponse(Connection conn, SpaceEnterResponse msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            //主角为空或者当前地图不等于传入的地图Id
            if (GameApp.character == null || GameApp.character.Info.SpaceId != msg.Character.SpaceId)
            {
                //先清理旧的场景的角色和怪物
                EntityManager.Instance.Clear();
                //需要加载新的场景
                //GameApp.LoadScene(msg.Character.SpaceId);
                GameApp.AsyncLoadScene(msg.Character.SpaceId,(AsyncOperation)=> 
                {
                    //把其他单位加入游戏
                    foreach (var item in msg.List)
                    {
                        EntityManager.Instance.OnEntityEnter(item);
                    }
                    //把自己加入场景
                    EntityManager.Instance.OnEntityEnter(msg.Character);
                    //加入GameApp
                    GameApp.character = EntityManager.Instance.GetEntity<Character>(msg.Character.Entity.Id);
                    //玩家货币信息申请
                    GetCurrencyRequest res = new GetCurrencyRequest()
                    {
                        PlayerId = GameApp.playerId
                    };
                    NetClient.Send(res);
                });
            }
        });
    }

    /// <summary>
    /// 接受聊天消息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _ChatResponse(Connection conn, ChatResponse msg)
    { 
        var chr = Game.GetUnit(msg.SenderId)as Character;
        var text = $"[玩家]{chr.Info.Name}:{msg.TextValue}";
        UnityMainThreadDispatcher.Instance().Enqueue(() => 
        {
            UIManager.Instance.Find<SimpleChatPanel>().CreateText(text);
        });
    }

    /// <summary>
    /// 属性更新响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _PropertyUpdateResponse(Connection conn, PropertyUpdateResponse msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            foreach (PropertyUpdate item in msg.List)
            {
                var actor = Game.GetUnit(item.EntityId);
                switch (item.Property )
                {
                    case PropertyUpdate.Types.Prop.Hp:
                        actor.OnHpChanged(item.OldValue.FloatValue,item.NewValue.FloatValue);
                        break;
                    case PropertyUpdate.Types.Prop.Mp:
                        actor.OnMpChanged(item.OldValue.FloatValue, item.NewValue.FloatValue);
                        break;
                    case PropertyUpdate.Types.Prop.State:
                        actor.OnStateChanged(item.OldValue.StateValue, item.NewValue.StateValue);
                        break;
                }
            }
        });
    }
    /// <summary>
    /// 受到伤害响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _DamageResponse(Connection conn, DamageResponse msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            foreach (Damage item in msg.List)
            {
                var attacker = Game.GetUnit(item.AttackerId);
                var target = Game.GetUnit(item.TargetId);
                target.recvDamage(item);
            }
        });
    }

    /// <summary>
    /// 施法响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _SpellResponse(Connection conn, SpellResponse msg)
    {
        foreach (CastInfo item in msg.CastList)
        {
            Debug.Log($"施法信息:{msg}");
            //获取技能施法者
            var caster = Game.GetUnit(item.CasterId);
            try 
            {
                //获取技能
                var skill = caster.SkillMgr.GetSkill(item.SkillId);
                //检查技能类型
                if (skill.IsUnitTarget)
                {
                    //使用技能
                    var target = Game.GetUnit(item.TargetId);
                    skill.Use(new SCEntity(target));
                }
                if (skill.IsNoneTarget)
                {
                    //使用技能
                    skill.Use(new SCEntity(caster));
                }
            }
            catch(Exception e)
            {
                Debug.LogError("施法异常"+e.Message);
            }
            
        }
    }

    private void Update()
    {
        Kaiyun.Event.Tick();
        SelectTarget();
        //拾取物品
        if (Input.GetKeyDown(KeyCode.C))
        {
            PickUp();
        }
    }
    /// <summary>
    /// 拾取物品
    /// </summary>
    private void PickUp()
    {
        PickupItemRequest resp = new PickupItemRequest();
        NetClient.Send(resp);
    }

    /// <summary>
    /// 每帧固定次数执行
    /// </summary>
    private void FixedUpdate()
    {
        EntityManager.Instance.OnUpdate(Time.fixedDeltaTime);
    }
    private void OnDestroy()
    {
        Kaiyun.Event.UnregisterIn("EnterGame", this, "EnterGame");
    }
    

    /// <summary>
    /// 角色离开地图
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _SpaceCharacterLeaveResponse(Connection sender, SpaceCharacterLeaveResponse msg)
    {
        //把Entity从Entity管理器中移除
        EntityManager.Instance.RemoveEntity(msg.EntityId);
    }

    /// <summary>
    /// 接受服务器的心跳响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _HeartBeatResponse(Connection conn, HeartBeatResponse msg)
    {
        //Debug.Log($"收到服务器{conn}");
        spanTime = DateTime.Now-requestTime;
        
        //心跳响应函数是由子线程触发的，unity的对象必须在主线程中使用
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            int ms = Math.Max(1,((int)spanTime.TotalMilliseconds));
            ycText.text = $"网络延迟:{ms}ms" ;
        });
    }

    /// <summary>
    /// 每隔一段时间发送一次心跳包
    /// </summary>
    /// <returns></returns>
    IEnumerator SendHeartMessage()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            NetClient.Send(beatRequest);
            requestTime = DateTime.Now;
        }
    }
    /// <summary>
    /// 响应角色同步信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _SpaceEntitySyncResponse(Connection sender, SpaceEntitySyncResponse msg)
    {
        EntityManager.Instance.OnEntitySync(msg.EntitySync);
    }

    /// <summary>
    /// 加入游戏的响应结果（Entity肯定是自己）
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="msg"></param>
    private void _GameEnterResponse(Connection conn, GameEnterResponse msg)
    {
        Debug.Log("加入游戏的响应结果:" + msg.Success);
        if (msg.Success)
        {   
            Debug.Log("角色信息:" + msg);
            var info = msg.Actor;
            info.Entity = msg.Entity;
            //加载游戏场景
            GameApp.LoadScene(info.SpaceId);
            //把Entity加入到Entity管理器
            EntityManager.Instance.OnEntityEnter(info);
            //从Entity管理器中获取Entity
            GameApp.character = EntityManager.Instance.GetEntity<Character>(msg.Entity.Id);          
        }
    }

    /// <summary>
    /// 当有角色进入地图时候的通知（肯定不是自己）
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="msg"></param>
    private void _SpaceCharactersEnterResponse(Connection conn, SpaceCharactersEnterResponse msg)
    {
        Debug.Log($"收到{msg.ActorList}");
        foreach (var info in msg.ActorList)
        {
            //把Entity加入到Entity管理器
            EntityManager.Instance.OnEntityEnter(info);
        }
    }
    /// <summary>
    /// 加入游戏
    /// </summary>
    public void EnterGame(int roleId)
    {
        if (hero != null) return;
        GameEnterRequest request = new GameEnterRequest();
        request.CharacterId = roleId;
        NetClient.Send(request);
        //显示战斗UI
        UIManager.Instance.ShowUI<PlayPanel>();
        //UIManager.Instance.ShowUI<FightPanel>(E_UIPanelLayer.Rearmost);
        UIManager.Instance.ShowUI<SimpleChatPanel>(E_UIPanelLayer.Rearmost);
    }
    /// <summary>
    /// 关闭后台网络连接
    /// </summary>
    private void OnApplicationQuit()
    {
        NetClient.Close();
    }
    /// <summary>
    /// 选择施法对象
    /// </summary>
    /// <returns></returns>
    private void SelectTarget()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            LayerMask actorLayer = LayerMask.GetMask("Actor");
            if (Physics.Raycast(ray,out raycastHit,Mathf.Infinity,actorLayer))
            {
                GameObject clickedObject = raycastHit.collider.gameObject;
                Debug.Log("选择目标"+clickedObject);
                //通过游戏对象获取entityId
                int entityId = clickedObject.GetComponent<GameEntity>().entityId;
                //通过Entity管理器获取Actor赋值给GameApp.Target
                GameApp.Target= EntityManager.Instance.GetEntity<Actor>(entityId);
            }
        }
        
    }
    /// <summary>
    /// 放置物品请求
    /// </summary>
    public void ItemPlacement(int originIndex,int targetIndex)
    {
        //放置请求信息
        ItemPlacementRequest res = new ItemPlacementRequest()
        {
            EntityId = GameApp.character.entityId,
            OriginIndex=originIndex,
            TargetIndex=targetIndex
        };
        NetClient.Send(res);
    }
    /// <summary>
    /// 使用物品请求
    /// </summary>
    public void UseItem(int slotIndex)
    {
        //使用物品请求消息
        ItemUseRequest res = new ItemUseRequest()
        {
            EntityId = GameApp.character.entityId,
            SlotIndex = slotIndex
        };
        NetClient.Send(res);
    }
    /// <summary>
    /// 丢弃物品请求
    /// </summary>
    public void ItemDiscard(int slotIndex,int count)
    {
        //丢弃物品请求消息
        ItemDiscardRequest res = new ItemDiscardRequest()
        {
            EntityId=GameApp.character.entityId,
            SlotIndex=slotIndex,
            Count= count
        };
        NetClient.Send(res);
    }
    /// <summary>
    /// 添加物品请求
    /// </summary>
    public void AddItemRequest(int itemId,int amount)
    {
        AddItemRequest res = new AddItemRequest()
        {
            ItemId=itemId,
            Amount=amount,
            PlayerId=GameApp.playerId
        };
        NetClient.Send(res);
    }
}
