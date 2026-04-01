using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DialogueSystem
{
    public class DialogueData
    {
        public List<int> ChapterIds { get; set; } = new List<int>();
        public List<int> GroupIds { get; set; } = new List<int>();
        public DialogueData()
        {

        }
        public DialogueData(List<int> ChapterIds,List<int> GroupIds)
        {
            this.ChapterIds = ChapterIds;
            this.GroupIds = GroupIds;
        }
    }
}
