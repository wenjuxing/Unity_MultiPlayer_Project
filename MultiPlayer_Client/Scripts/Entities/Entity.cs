using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using Summer;
using UnityEngine;
namespace GameClient
{
    //在MMO世界进行同步的实体
    public class Entity
    {
        public EntityState State;    //状态
        private Vector3 _position;    //位置
        private Vector3 _direction;   //方向
        private NetEntity _netObj;         //网络对象 
        private int _speed;              //速度
        private long _lastUpdate;       //最后一次更新的时间戳
        public int entityId => _netObj.Id; 
        public int Speed {
            get { return _speed; }
            set {
                _speed = value;
                _netObj.Speed = value;
                _lastUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            } }
        /// <summary>
        /// 上次更新位置的时间和现在的时间差
        /// </summary>
        public float PositionTime
        {
            get { return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastUpdate) * 0.001f; }
        }
        public Vector3 Position
        {
            get { return _position; }
            set { _position = value;
                _netObj.Position = V3.ToVec3(value);
            }
        }
        public Vector3 Direction
        {
            get { return _direction; }
            set { _direction = value;
                _netObj.Direction = V3.ToVec3(value);
            }
        }
        public NetEntity EntityData
        {
            get { return _netObj; }
            set
            {
                Position = V3.ToVector3(value.Position);
                Direction = V3.ToVector3(value.Direction) ;
                Speed= value.Speed;
            }
        }
        public Entity(NetEntity entity)
        {
            _netObj = new NetEntity();
            _netObj.Id = entity.Id;
            this.EntityData = entity;
        }
 
    }
}
