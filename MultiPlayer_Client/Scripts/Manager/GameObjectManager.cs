using Assets.Scripts.U3d_scripts;
using GameClient;
using GameClient.Entities;
using Proto;
using Summer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectManager : MonoBehaviour
{
    public static GameObjectManager Instance;
    //entityId和对应的游戏对象
    private static Dictionary<int, GameObject> dict = new Dictionary<int, GameObject>();
    private void Start()
    {
        Instance = this;
        Kaiyun.Event.RegisterOut("CharacterEnter", this, "CharacterEnter");
        Kaiyun.Event.RegisterOut("CharacterLeave", this, "CharacterLeave");
        Kaiyun.Event.RegisterOut("EntitySync", this, "EntitySync");
    }
    private void OnDestroy()
    {
        Kaiyun.Event.UnregisterOut("CharacterEnter", this, "CharacterEnter");
        Kaiyun.Event.UnregisterOut("CharacterLeave", this, "CharacterLeave");
        Kaiyun.Event.UnregisterOut("EntitySync", this, "EntitySync");
    }
    /// <summary>
    /// 创建物品
    /// </summary>
    /// <param name="chr"></param>
    private void ItemEnter(NetActor chr)
    {
        //设置出生点
        Vector3 InitPos = V3.Of(chr.Entity.Position) / 1000f;
        //设置Y轴坐标
        if (InitPos.y == 0)
        {
            InitPos = GameTools.CalculateGroundPosition(InitPos);
        }
        //加载预制体
        var itemDef = DataManager.Instance.Items[chr.ItemInfo.ItemId];
        Actor actor = Game.GetUnit(chr.Entity.Id);
        var prefab = Resources.Load<GameObject>(itemDef.Model);
        var go = Instantiate(prefab, InitPos, Quaternion.identity, this.transform);
        //加入Actor图层
        go.layer = 6;
        actor.renderObj = go; 
        //把游戏对象加入字典
        dict.Add(chr.Entity.Id, go);
    }
    /// <summary>
    /// 创建游戏角色
    /// </summary>
    /// <param name="chr"></param>
    public void CharacterEnter(NetActor chr)
    {
        //如果为物品
        if (chr.EntityType==EntityType.Item)
        {
            ItemEnter(chr);
            return;
        }
        if (!dict.ContainsKey(chr.Entity.Id))
        {
            //判断创建的角色是否为自己
            bool IsMine = (GameApp.character.entityId == chr.Entity.Id);

            //设置出生点
            Vector3 InitPos = V3.Of(chr.Entity.Position)/1000f;
            //设置Y轴坐标
            if (InitPos.y == 0)
            {
                InitPos = GameTools.CalculateGroundPosition(InitPos);
            }
            Actor actor = Game.GetUnit(chr.Entity.Id);
            //加载预制体
            UnitDefine define= DataManager.Instance.Units[chr.Tid];
            var prefab = Resources.Load<GameObject>(define.Resource);
            var go = Instantiate(prefab, InitPos,Quaternion.identity,this.transform);
            //加入Actor图层
            go.layer = 6;
            actor.renderObj = go;
            //获取游戏对象上的GameEntity并设置属性
            var gameEntity = go.GetComponent<GameEntity>();
            gameEntity.IsMine = IsMine;
            gameEntity.entityName = chr.Name;
            gameEntity.entityType = chr.EntityType;
            gameEntity.SetData(chr.Entity);
            //如果是我自己就添加角色控制器
            if (IsMine)
            {
                go.AddComponent<HeroController>();
            }
            //设置游戏对象名字
            if (chr.EntityType==EntityType.Character)
            {
                go.name = $"Character_{chr.EntityId}";
            }
            if (chr.EntityType == EntityType.Monster)
            {
                go.name = $"Monster_{chr.EntityId}";
            }
            //把游戏对象加入字典
            dict.Add(chr.Entity.Id,go);
        }
       
    } 
    /// <summary>
    /// 角色离开场景
    /// </summary>
    /// <param name="entityId"></param>
    public void CharacterLeave(int entityId)
    {
        if (dict.ContainsKey(entityId))
        {
            var obj = dict[entityId];
            if (obj!=null)
            {
                Destroy(obj);
            }
            dict.Remove(entityId);
        }
    }
    /// <summary>
    /// 角色网络数据同步
    /// </summary>
    /// <param name="entitySync"></param>
    public void EntitySync(NEntitySync entitySync)
    {
        if (!dict.TryGetValue(entitySync.Entity.Id, out GameObject go)) return;

        //设置Y轴坐标
        Vector3 Pos = V3.Of(entitySync.Entity.Position) / 1000f;
        if (Pos.y == 0)
        {
            Pos = GameTools.CalculateGroundPosition(Pos,20);
            entitySync.Entity.Position = V3.ToVec3(Pos * 1000);
        }
        var gameEntity = go.GetComponent<GameEntity>();
        gameEntity.SetData(entitySync.Entity);
        //如果数据异常则强制返回原来位置
        if (entitySync.Force)
        {
            Vector3 target = V3.Of(entitySync.Entity.Position)*0.001f;
            gameEntity.Move(target);
        }
    }
}
