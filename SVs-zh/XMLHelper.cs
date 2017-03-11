using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SVs
{
        public static class XMLHelper
        {
            //读取xml文件的方法，返回值为字符串数组
            public static string[] ReadParameterFromXML(string FileName)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(FileName);    //加载Xml文件  
                XmlElement rootElem = doc.DocumentElement;   //获取根节点  
                XmlNodeList varNodes = rootElem.GetElementsByTagName("var"); //获取person子节点集合  
                string[] strVar = new string[doc.FirstChild.ChildNodes.Count];
                int a = 0;
                foreach (XmlNode node in varNodes)
                {
                    string strName = ((XmlElement)node).GetAttribute("name");   //获取name属性值  
                    strVar[a] = strName;
                    //MessageBox.Show(strVar[a]);
                    a++;
                }
                return (strVar);
            }

        }
    
}
