using OpcCommander;
using SVs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SVs
{
    class OPCOprHelper
    {
        /*
         * 作者：梁泽逍
         * 时间：2017年3月7日
         * 目的：第一次重构代码，将主页面main的OPC操作分离到此类中
         * */

        //单参数读取
        static public void GetItem(MyServer server,string[] itemID,out string type,out string value)
        {
            int[] itemHANDLES = new int[] { 0 };
            VarEnum[] arrayType = new VarEnum[] { VarEnum.VT_EMPTY };

            MyVariant[] returnValue = null;
            server.AddItems(itemID, out itemHANDLES, out arrayType);
            type = MyServer.VarEnumToString(arrayType[0]);
            server.ReadItems(itemID, itemHANDLES, arrayType, out returnValue);
            if (returnValue != null)
            {
                value = returnValue[0].GetStringValue();
            }
            else
            {
                value = "null";
            }
        }

        //多参数读取
        static public void GetItems(MyServer server, string[] itemID, out string[] type, out string[] value)
        {
            int[] itemHANDLES = new int[] { 0 };
            VarEnum[] arrayType = new VarEnum[] { VarEnum.VT_EMPTY };

            MyVariant[] returnValue = null;
            server.AddItems(itemID, out itemHANDLES, out arrayType);

            server.ReadItems(itemID, itemHANDLES, arrayType, out returnValue);

            type = new string[returnValue.Length];
            value = new string[returnValue.Length];

            for (int i = 0; i < returnValue.Length; i++)
            {
                type[i] = MyServer.VarEnumToString(arrayType[i]);
                if (returnValue != null)
                {                    
                    value[i] = returnValue[i].GetStringValue();
                }
                else
                {
                    value[i] = "null";
                }
            }
           

 
        }
    }
}
