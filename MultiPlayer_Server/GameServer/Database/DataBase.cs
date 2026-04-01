using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Database
{
   public class DataBase
    {
        /// <summary>
        /// 可配置参数
        /// </summary>
        static string host = "127.0.0.1";
        static int port = 3306;
        static string user = "root";
        static string password = "123456";
        static string dbName = "mmogame";

        static string connectionString =
             $"Data Source={host};Port={port};User ID={user};Password={password};" +
             $"Initial Catalog={dbName};Charset=utf8;SslMode=none;Max pool size=10";

        public static IFreeSql fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.MySql, connectionString)
            .UseAutoSyncStructure(true) //自动同步实体结构到数据库
            .Build(); //请务必定义成 Singleton 单例模式


        /// <summary>
        /// 手动建表工具类
        /// </summary>
        public static void SyncAllTables()
        {
            try
            {
                // 逐个同步需要的实体类（新增实体类后，这里需要补充）
                fsql.CodeFirst.SyncStructure<PlayerDialogueState>();
                // 如果有其他实体类（如角色表、道具表），也在这里添加：
                // fsql.CodeFirst.SyncStructure<Player>();
                // fsql.CodeFirst.SyncStructure<Item>();
                 fsql.CodeFirst.SyncStructure<DbPlayer>();

                Console.WriteLine("所有表结构同步成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"表结构同步失败：{ex.Message}");
                throw; // 建表失败可终止服务器启动，避免后续异常
            }
        }
    }
}
