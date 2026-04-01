using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Database
{
    /// <summary>
    /// 玩家信息
    /// </summary>
    [Table(Name ="player")]
    public class DbPlayer
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Coin { get; set; }
      
        [Column(DbType = "blob")]
        public byte[] StorageInfo { get; set; }

    }
    /// <summary>
    /// 玩家的角色
    /// </summary>
    [Table(Name = "character")]
    public class DbCharacter
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public int JobId { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; } = 100;
        public int Mp { get; set; } = 100;
        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        public int SpaceId { get; set; }
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Z { get; set; } = 0;
        public long Gold { get; set; } = 0;
        public int PlayerId { get; set; }
        [Column(DbType = "blob")]
        public byte[] Knapsack { get; set; }
    }
     
    [Table(Name = "task")] 
    public class PlayerTaskState
    {
        [Column(IsPrimary = true, Name = "player_id")] // 联合主键之一
        public int Id { get; set; }
     
        [Column(IsPrimary = true, Name = "task_id")] // 联合主键之二（角色+任务唯一）
        public int TaskId { get; set; }
      
        [Column(Name = "is_accepted")]
        public bool IsAccepted { get; set; } = false;

        [Column(Name = "is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column(Name = "is_submitted")]
        public bool IsSubmitted { get; set; } = false;

        [Column(Name = "is_abandoned")]
        public bool IsAbandoned { get; set; } = false;

        [Column(Name = "current_progress")]
        public int CurrentProgress { get; set; }
    }
    [Table(Name = "dialogue")]
    public class PlayerDialogueState
    {
        [Column(IsPrimary = true, Name = "player_id")] // 联合主键之一
        public int Id { get; set; }

        [Column(Name = "completed_chapter_ids", DbType = "varchar(200)")]
        public string UnlockedChapterIds { get; set; } = ""; // 初始化为空字符串

        [Column(Name = "completed_group_ids", DbType = "varchar(500)")] // MySQL 用 varchar 存储集合
        public string CompletedGroupIds { get; set; } = ""; // 初始化为空字符串，避免 null
    }
}
