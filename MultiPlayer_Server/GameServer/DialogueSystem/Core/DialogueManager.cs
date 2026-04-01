using GameServer.Database;
using GameServer.DialogueSystem.Tools;
using GameServer.Model;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DialogueSystem
{
    public class DialogueManager
    {
        public Character chr;
        public DialogueData dialogueData;
        public DialogueManager(Character chr)
        {
            this.chr = chr;
        }
        /// <summary>
        /// 初始化对话信息
        /// </summary>
        public void Init()
        {
            var repo = DataBase.fsql.GetRepository<PlayerDialogueState>();

            //如果是第一次进入游戏
            if (repo.Where(t => t.Id == chr.playerId).Count()==0)
            {
                PlayerDialogueState dialogueState = new PlayerDialogueState()
                {
                    Id = chr.playerId,
                    CompletedGroupIds = "",
                    UnlockedChapterIds=""
                };
                repo.Insert(dialogueState);
            }
            //从数据库读取对话数据
            PlayerDialogueState currentDialogueState = repo.Where(t => t.Id == chr.playerId).First();
            dialogueData = new DialogueData()
            {
                ChapterIds = DialogueProgressHelper.StrToList(currentDialogueState.UnlockedChapterIds),
                GroupIds = DialogueProgressHelper.StrToList(currentDialogueState.CompletedGroupIds)
            };
            Log.Information("玩家Id:{0},玩家对话章节{1}",dialogueData.ChapterIds,dialogueData.ChapterIds);
        }
        /// <summary>
        /// 更新对话数据
        /// </summary>
        public void UpdateDialogueData(int Id,int groupId,int chapterId)
        {
           var repo=DataBase.fsql.GetRepository<PlayerDialogueState>();
           var update= repo.Where(t=>t.Id==Id).First();
            if (update == null)
            {
                Log.Information("数据不存在{1}", chr.playerId);
                return;
            }

            //往列表中添加新的对话组Id并存入数据库
            if (!dialogueData.GroupIds.Contains(groupId))
                dialogueData.GroupIds.Add(groupId);
            update.CompletedGroupIds = DialogueProgressHelper.ListToStr(dialogueData.GroupIds);

            //往列表中添加新的对话章节Id并存入数据库
            if (!dialogueData.ChapterIds.Contains(chapterId))
                dialogueData.ChapterIds.Add(chapterId);
            update.UnlockedChapterIds = DialogueProgressHelper.ListToStr(dialogueData.ChapterIds);

            repo.Update(update);
        }
    }
}
