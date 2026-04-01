using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using Summer;

namespace GameServer.Model
{
    //在MMO世界进行同步的实体
    public class Entity
    {
        private Vector3Int position;    //位置
        private Vector3Int direction;   //方向
        private NetEntity netObj;         //网络对象 
        private int speed;              //速度
        private long _lastUpdate;       //最后一次更新的时间戳
        public EntityState state;       //状态 
        public int entityId { get { return netObj.Id; } }
        public int Speed {
            get { return speed; }
            set {
                speed = value;
                netObj.Speed = value;
                _lastUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            } }
        /// <summary>
        /// 上次更新位置的时间和现在的时间差
        /// </summary>
        public float PositionTime
        {
            get { return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastUpdate) * 0.001f; }
        }
        public Vector3Int Position
        {
            get { return position; }
            set { position = value;
                netObj.Position = value;
            }
        }
        public Vector3Int Direction
        {
            get { return direction; }
            set { direction = value;
                netObj.Direction = value;
            }
        }
        public NetEntity EntityData
        {
            get { return netObj; }
            set
            {
                Position = value.Position;
                Direction = value.Direction;
                Speed= value.Speed;
            }
        }
        public Entity(Vector3Int pos,Vector3Int dir)
        {
            netObj = new NetEntity();
            Position = pos;
            Direction = dir;
        }
        public virtual void Update()
        {

        }
    }
}
