using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SVs
{
    //Cy
    //用来读取INI文件里面有关设备信息
     public class INIClass
    {
            public string FileName; //INI文件名 
            public string inipath;
            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);
            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
            /// <summary> 
            /// 构造方法 
            /// </summary> 
            /// <param name="INIPath">文件路径</param> 
            public INIClass(string INIPath)
            {
                inipath = INIPath;
            }
            /// <summary> 
            /// 写入INI文件 
            /// </summary> 
            /// <param name="Section">项目名称(如 [TypeName] )</param> 
            /// <param name="Key">键</param> 
            /// <param name="Value">值</param> 
            //public void IniWriteValue(string Section, string Key, string Value)
            //{
            //    WritePrivateProfileString(Section, Key, Value, this.inipath);
            //}
            public byte[] IniReadValues(string section, string key)
            {
                byte[] temp = new byte[255];
                int i = GetPrivateProfileString(section, key, "", temp, 255, this.inipath);
                return temp;
            }

            /// <summary> 
            /// 读出INI文件 
            /// </summary> 
            /// <param name="Section">项目名称(如 [TypeName] )</param> 
            /// <param name="Key">键</param> 
            public string IniReadValue(string Section, string Key)
            {
                StringBuilder temp = new StringBuilder(500);
                int i = GetPrivateProfileString(Section, Key, "", temp, 500, this.inipath);
                return temp.ToString();
            }
            /// <summary> 
            /// 验证文件是否存在 
            /// </summary> 
            /// <returns>布尔值</returns> 
            public bool ExistINIFile()
            {
                return File.Exists(inipath);
            }

            [DllImport("kernel32")]
            private static extern uint GetPrivateProfileString(
                string lpAppName, // points to section name
                string lpKeyName, // points to key name
                string lpDefault, // points to default string
                byte[] lpReturnedString, // points to destination buffer
                uint nSize, // size of destination buffer
                string lpFileName  // points to initialization filename
            );
            /// <summary>
            /// 读取section
            /// </summary>
            /// <param name="Strings"></param>
            /// <returns></returns>
            public List<string> ReadSections(string iniFilename)
            {
                List<string> result = new List<string>();
                byte[] buf = new byte[65536];
                uint len = GetPrivateProfileString(null, null, null, buf, (uint)buf.Length, iniFilename);
                int k = 0;
                for (int i = 0; i < len; i++)
                    if (buf[i] == 0)
                    {
                        result.Add(Encoding.Default.GetString(buf, k, i - k));
                        k = i + 1;
                    }
                return result;
            }
            public void ReadSection(string Section, StringCollection Idents)
            {
                Byte[] Buffer = new Byte[16384];
                //Idents.Clear(); 

                int bufLen = GetPrivateProfileString(Section, null, null, Buffer, Buffer.GetUpperBound(0),
                              FileName);
                //对Section进行解析 
                GetStringsFromBuffer(Buffer, bufLen, Idents);
            }

            private void GetStringsFromBuffer(Byte[] Buffer, int bufLen, StringCollection Strings)
            {
                Strings.Clear();
                if (bufLen != 0)
                {
                    int start = 0;
                    for (int i = 0; i < bufLen; i++)
                    {
                        if ((Buffer[i] == 0) && ((i - start) > 0))
                        {
                            String s = Encoding.GetEncoding(0).GetString(Buffer, start, i - start);
                            Strings.Add(s);
                            start = i + 1;
                        }
                    }
                }
            }

        }
}
