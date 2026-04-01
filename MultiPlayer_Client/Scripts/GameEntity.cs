using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;
using GameClient.Entities;
using GameClient;

public class GameEntity : MonoBehaviour
{
    public int entityId;
    public Vector3 position;
    public Vector3 direction;
    public bool IsMine;
    private CharacterController characterController;
    private float fallSpeed=0f;
    private float fallSpeedMax = 30f;
    public float speed;
    public Actor actor;

    public string entityName = "闪电五连鞭";
    public EntityType entityType=EntityType.Character;
    public EntityState entityState;
    private void Start()
    {
        actor = Game.GetUnit(entityId);
        //发送同步请求
        StartCoroutine(SyncRequest());

        characterController = GetComponent<CharacterController>();
    }
    private void Update()
    {
        if (!IsMine)
        {
            var targetRotation = Quaternion.Euler(direction);
            this.transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            Move(Vector3.Lerp(transform.position, position, Time.deltaTime * 10f));
        }
        else
        {
            this.position = transform.position;
            this.direction = transform.rotation.eulerAngles;
        }

        //计算模拟重力
        if (!characterController.isGrounded)
        {
            if (fallSpeed<= fallSpeedMax)
            {
                fallSpeed += 9.8f * Time.deltaTime;
            }
            characterController.Move(new Vector3(0,-fallSpeed*Time.deltaTime,0));
        }
        else
        {
            characterController.Move(new Vector3(0, -0.01f, 0));
            fallSpeed = 0;
        }
    }
    public void SetData(NetEntity nEntity,bool InstantMove=false)
    {
        //把网络的Vec3转换为本地的Vector3
        this.entityId = nEntity.Id;
        this.position = ToVector3(nEntity.Position);
        this.direction = ToVector3(nEntity.Direction);
        this.speed = nEntity.Speed * 0.001f;
        //再把本地的Vec3赋值给transfrom
        if (InstantMove)
        {
            this.transform.rotation= Quaternion.Euler(direction);
            Move(position);
        }
    }
    public void Move(Vector3 target)
    {
        var ctr = GetComponent<CharacterController>();
        ctr.Move(target-ctr.transform.position);
    }
    //优化堆的内存,创建实例避免空指针异常
    SpaceEntitySyncRequest res = new SpaceEntitySyncRequest()
    {
        EntitySync = new NEntitySync()
        {
            Entity = new NetEntity()
            {
                Position = new Vec3(),
                Direction=new Vec3()
            }
        }
    };
    /// <summary>
    /// 显示玩家名字
    /// </summary>
    private void OnGUI()
    {
       // //对象不在相机范围内则返回
       // if (!IsInView(this.gameObject)) return;
       // //设置高度
       // float height = 1.8f;
       // var playerCamera = Camera.main;
       // //计算世界坐标
       // var pos = new Vector3(transform.position.x,transform.position.y+height,transform.position.z);
       // //把世界坐标转换为屏幕坐标
       //Vector2 uiPos=playerCamera.WorldToScreenPoint(pos);
       // //计算角色头顶的真实二维坐标
       // uiPos = new Vector2(uiPos.x, Screen.height - uiPos.y);
       // Vector2 nameSize=GUI.skin.label.CalcSize(new GUIContent(entityName));
       // if (entityType == EntityType.Character)
       // {
       //     GUI.color = Color.white;
       // }
       // if (entityType == EntityType.Monster)
       // {
       //     GUI.color = Color.red;
       // }
       // GUI.Label(new Rect(uiPos.x-(nameSize.x/2),uiPos.y-nameSize.y,nameSize.x,nameSize.y),entityName);
    }
    /// <summary>
    /// 点乘判断游戏对象是否在相机视野内
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool IsInView(GameObject target)
    {
        Vector3 worldPos = target.transform.position;
        Transform camTransform = Camera.main.transform;
        //如果超距则返回
        if (Vector3.Distance(camTransform.position, worldPos) > 50f) return false;
        Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPos);
        Vector3 dir = (worldPos - camTransform.position).normalized;
        //判断物体是否在相机前面
        float dot = Vector3.Dot(camTransform.forward, dir);

        if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 向服务器发送同步请求10/s
    /// </summary>
    /// <returns></returns>
    IEnumerator SyncRequest()
    {
        //记录上次发送的状态
        EntityState _lastState = EntityState.None;
        while (true)
        {
            if (IsMine&&transform.hasChanged&&!actor.IsDeath)
            {
                //把本地Vector3转换为网络Vector3
                res.EntitySync.Entity.Id = entityId;
                //角色状态发生改变向服务器发送新的状态
                if (_lastState!= entityState)
                {
                    res.EntitySync.State = entityState;
                    _lastState = entityState;
                }
                this.actor.Position = this.position * 1000;
                this.actor.Direction = this.direction * 1000;
                SetValueTo(this.actor.Position, res.EntitySync.Entity.Position);
                SetValueTo(this.actor.Direction, res.EntitySync.Entity.Direction);
                //发送同步消息
                NetClient.Send(res);
                transform.hasChanged = false;
                //每次发送完状态则清空在res中的状态
                res.EntitySync.State = EntityState.None;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    /// <summary>
    /// 把Vector3转换为Vec3
    /// </summary>
    /// <param name="v">vec3默认是float类型需要转换为int类型</param>
    /// <returns></returns>
    private Vec3 ToNVector3(Vector3 v)
    {
        v *= 1000;
       return new Vec3() {X=(int)v.x,Y=(int)v.y,Z=(int)v.z };
    }
    /// <summary>
    /// 把Vec3转换为Vector3类型
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private Vector3 ToVector3(Vec3 v)
    {
        return new Vector3() { x = v.X, y = v.Y, z = v.Z }*0.001f;
    }
    /// <summary>
    /// 给Vec3设值
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    private void SetValueTo(Vector3 a,Vec3 b)
    {
        b.X = (int)a.x;
        b.Y = (int)a.y;
        b.Z = (int)a.z;
    }
}
