using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql;
using GameServer.Database;
using GameServer.Model;
using Proto;
using Google.Protobuf;
using Serilog;

namespace GameServer.StorageSystem
{
    public class StorageManager
    {
        //仓储物品配置信息
        public Dictionary<int, ShopDefine> GoodsConfigs = new Dictionary<int, ShopDefine>();
        //线程安全字典
        public ConcurrentDictionary<int, GoodsItem> GoodsItemsMap = new ConcurrentDictionary<int, GoodsItem>();
        //获取数据库中玩家信息表
        IBaseRepository<DbPlayer> repo = DataBase.fsql.GetRepository<DbPlayer>();
        private Character chr;
        public StorageManager(Character chr)
        {
            this.chr = chr;
            this.Init(chr.dbPlayer.StorageInfo);
        }
        //存储的物品信息
        private StorageInfo _storageInfo;
        public StorageInfo storageInfo 
        {
            get 
            {
                if (_storageInfo==null)
                {
                    _storageInfo = new StorageInfo();
                }
                _storageInfo.GoodsList.Clear();

                foreach (var item in GoodsItemsMap.Values)
                {
                    _storageInfo.GoodsList.Add(item.goodsInfo);
                }
                return _storageInfo;
            }
        }
        //初始化数据
        private void Init(byte[] data)
        {
            //获取商品配置信息
            GoodsConfigs = DataManager.Instance.GoodsItem;
            if (data == null) return;
            //从数据库中获取存储信息
            StorageInfo storageInfo = StorageInfo.Parser.ParseFrom(data);
            //创建物品并存储字典
            foreach (var info in storageInfo.GoodsList)
            {
                GoodsItemsMap.TryAdd(info.GoodsId,new GoodsItem(info.GoodsId,info.Count));
            }
        }
        /// <summary>
        /// 更新玩家仓储信息
        /// </summary>
        public void UpdateStorageInfo(int goodsId)
        {
            //判断字典中是否有重复的商品如果有则堆叠数量
            if (GoodsItemsMap.TryGetValue(goodsId, out var item))
            {
                item.Count++;
                GoodsItemsMap[goodsId] = item;
            }
            else
            {
                GoodsItemsMap.TryAdd(goodsId,new GoodsItem(goodsId,1));
            }
            //把玩家仓储信息存储数据库
            chr.dbPlayer.StorageInfo = chr.storageManager.storageInfo.ToByteArray();
            repo.UpdateAsync(chr.dbPlayer);
        }
        /// <summary>
        /// 更新玩家货币信息
        /// </summary>
        public void UpdateCurrencyInfo(int goodsId)
        {
            int currency = chr.dbPlayer.Coin - GoodsConfigs[goodsId].Price;
            //把玩家仓储信息存储数据库
            chr.dbPlayer.Coin = currency;
            repo.UpdateAsync(chr.dbPlayer);
        }
    }
}
