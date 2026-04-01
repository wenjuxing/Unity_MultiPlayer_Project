using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Core
{
    public class MathC
    {
        /// <summary>
        /// 判断两个Float类型的值是否相等
        /// Float的值有浮动使用等号判断不准确
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Equals(float a,float b)
        {
           return Math.Abs(a - b) < 10e-6;//0.00001f
        }
    }
}
