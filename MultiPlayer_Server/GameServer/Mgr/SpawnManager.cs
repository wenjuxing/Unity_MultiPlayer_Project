using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
   public class SpawnManager
    {
        public List<Spawner> Rules = new List<Spawner>();
        public Space space { get;  set; }

        public void Init(Space space)
        {
            this.space = space;
            //根据当前场景加载对应的刷怪规则
          var rules= DataManager.Instance.Spawns.Values
                .Where(r => r.SpaceId == space.Id);
            //
            foreach (var r in rules)
            {
                //暂时取消
                //Rules.Add(new Spawner(r, space));
            }
        }
        public void Update()
        {
            Rules.ForEach(r => r.Update());
        }
    }
}
