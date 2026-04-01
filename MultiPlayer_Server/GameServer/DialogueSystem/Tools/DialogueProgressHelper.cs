using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.DialogueSystem.Tools
{
    /// <summary>
    /// 对话进度转化器
    /// </summary>
    public static class DialogueProgressHelper
    {
        /// <summary>
        /// 列表转字符串
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListToStr(List<int> list)
        {
            if (list == null || list.Count == 0) return "";
            //筛选出重复的数据
            var distinctList = list.Distinct().ToList();
            //在每个成员之间添加指定字符
            return string.Join(",",distinctList);
        }
        /// <summary>
        /// 字符串转列表
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        public static List<int> StrToList(string Str)
        {
            List<int> result = new List<int>();
            if (string.IsNullOrEmpty(Str))
            {
                return result;
            }

            //分割字符串
            string[] idStrs = Str.Split(',', (char)StringSplitOptions.RemoveEmptyEntries);
            //字符串转列表
            foreach (var c in idStrs)
            {
                if (int.TryParse(c, out int Id))
                    result.Add(Id);
                else
                    Log.Debug($"对话进度解析失败：{0} 不是有效的整数", idStrs);
            }
            return result;
        }
    }
}
