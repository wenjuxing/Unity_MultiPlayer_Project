using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
namespace GameServer.StorageSystem
{
    public class GoodsItem
    {
        public int GoodsId { get; set; }
        public int Count { get; set; }
        public int Position { get; set; }

        private GoodsInfo _goodsInfo;
        public GoodsInfo goodsInfo 
        {
            get 
            {
                if (_goodsInfo==null)
                {
                    _goodsInfo = new GoodsInfo();               
                }
                _goodsInfo.GoodsId = this.GoodsId;
                _goodsInfo.Count = this.Count;
                _goodsInfo.Position = this.Position;
                return _goodsInfo;
            }
        }
        public GoodsItem(int GoodsId,int Count)
        {
            this.GoodsId = GoodsId;
            this.Count = Count;
            //this.Position = Position;
        }

    }
}
