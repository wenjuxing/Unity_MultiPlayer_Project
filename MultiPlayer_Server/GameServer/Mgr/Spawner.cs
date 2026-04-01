using GameServer.Model;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
   public class Spawner
    {
        public SpawnDefine Define;
        public Space space;
        public Vector3Int Pos { get; private set; }
        public Vector3Int Dir { get; private set; }

        public Monster mon;
        private bool reviving;
        private float reviveTime;
        public Spawner(SpawnDefine Define, Space space)
        {
            this.Define = Define;
            this.space = space;
            Pos = ParsePoint(Define.Pos);
            Dir = ParsePoint(Define.Dir);
            Log.Information("场景:{0},位置:{1},单位类型:{2}",space.Name, Pos, Define.TID);
            this.Spawn();
        }
        /// <summary>
        /// 把字符串转换为三维坐标
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Vector3Int ParsePoint(string text)
        {
            string pattern = @"\[(\d+),(\d+),(\d+)\]";
            Match match = Regex.Match(text, pattern);
            if (match.Success)
            {
                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);
                int z = int.Parse(match.Groups[3].Value);
                return new Vector3Int(x, y, z);
            }
            return Vector3Int.zero;
        }
        private void Spawn()
        {
           this.mon=this.space.monsterManager.Create(Define.TID,Define.Level,Pos,Dir);
        }
        /// <summary>
        /// 复活？
        /// </summary>
        public void Update()
        {
            //是否满足复活条件
            if (mon!=null&&mon.IsDeath&&!reviving)
            {
                //计算复活时间
                reviveTime = Time.time + Define.Period;
                reviving = true;
            }
            //复活？
            if (reviving&&reviveTime<=Time.time)
            {
                reviving = false;
                this.mon?.Revive();
            }
        }
    }
}
