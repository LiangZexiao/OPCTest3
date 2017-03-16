using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVs
{
    class StringOpr
    {
        //参数：本机IP地址
        //返回：本机网段地址
        public static string GetNetSegment(string LocalIP)
        {
            string[] arr = LocalIP.Split('.');
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                builder.Append(arr[i]);
                builder.Append('.');
            }
            return builder.ToString();
        }
    }
}
