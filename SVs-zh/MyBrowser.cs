using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using OpcDA;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Xml;


namespace OpcCommander
{
    public class MyBrowser
    {
        public MyBrowser(MyServer s)
        {
            this.m_pIBrowse = s.GetBrowser();
            this.server = s;
        }
        //public
        public List<string> GetRoot()
        {
            setPosition("");
            List<string> root;
            List<string> temp; 
            listItems(OPCBROWSETYPE.OPC_BRANCH,out root, out temp);
            return root;
        }

        public List<string> GetAllChild(string patern)
        {
            List<string> temp;
            List<string> childs1;
            List<string> childs2;
            listItems(OPCBROWSETYPE.OPC_BRANCH, out childs1, out  temp);
            listItems(OPCBROWSETYPE.OPC_LEAF, out childs2, out  temp);
            foreach (string item in childs2)
            {
                childs1.Add(item);
            }
            return childs1;
        }

        public void GetChildWithChild(string patern, out List<string> items, out List<string> paths)
        {
            setPosition(patern);
            listItems(OPCBROWSETYPE.OPC_BRANCH,out items,out paths);
        }

        public void GetChildWithOutChild(string patern, out List<string> items, out List<string> paths)
        {
            setPosition(patern);
            listItems(OPCBROWSETYPE.OPC_LEAF, out items, out paths);
        }

        public int LogInToOpcUser(string root)
        {
            int logID=0;
            List<object> inValues = new List<object>();
            MyVariant[] outValues;
            //In.LocalLevel
            inValues.Add(0);
            //In.Name
            inValues.Add("opcuser");
            //In.Override
            inValues.Add(1);
            //In.Password
            inValues.Add("opcuser");//opcuser/ 
            //In.WriteAccess
            inValues.Add(1);
            //call
            bool run = true;
            int i=0;
            string error="";
            while (i < 3 && run == true)
            {
                callFunctionWithParameter(root + ".Funcs.User.LogIn()", inValues, out outValues);
                if (outValues != null)
                {
                    if (outValues.Length == 3)
                    {
                        error = outValues[0].GetStringValue();
                        if (error == "0")
                            run = false;
                        logID = (int)outValues[1].GetVar();
                    }
                    else
                        throw new Exception(root + ".Funcs.User.LogIn(): Count of output parameters must be equal 3.");
                }
                else
                    throw new Exception(root + ".Funcs.User.LogIn(): output parameters not found.");
                i++;
            }
            if(error != "0")
                throw new Exception(root + ".Funcs.User.LogIn(): Error = " + error);
            return logID;
        }

        public void LogOutFromOpcUser(string root, int id)
        {
            List<object> inValues = new List<object>();
            MyVariant[] outValues;
            //In.LoginId
            inValues.Add(id);
            //call
            bool run = true;
            int i = 0;
            string error = "";
            while (i < 3 && run == true)
            {
                callFunctionWithParameter(root + ".Funcs.User.LogOut()", inValues, out outValues);
                if (outValues != null)
                {
                    if (outValues.Length == 1)
                    {
                        error= outValues[0].GetStringValue();
                        if (error == "0")
                            run = false;
                    }
                    else
                        throw new Exception(root + ".Funcs.User.LogOut(): Count of output parameters must be equal 1.");
                }
                else
                    throw new Exception(root + ".Funcs.User.LogOut(): output parameters not found.");
                i++;
            }
            if (error != "0")
                throw new Exception(root + ".Funcs.User.LogOut(): Error = " + error);
        }

        public int Refresh(string root, int id)
        {
            int status = -100;
            List<object> inValues = new List<object>();
            MyVariant[] outValues;
            //In.LoginId
            inValues.Add(id);
            //call
            callFunctionWithParameter(root + ".Funcs.Status.GetSystemStatus()", inValues, out outValues);
            if (outValues != null)
                if (outValues.Length == 11)
                    status = (int)outValues[5].GetVar();
            return status;
        }
        //private
        private string getFullPath(string id)
        {
            string path = id;
            int ret = 0;
            ret = m_pIBrowse.GetItemID(id, out path);
            if (ret < 0)
                throw new Exception("Browser: Full Path is not found. Error= " + ret.ToString());
            return path;
        }

        private void setPosition(string f_name)
        {
            int m_Res;
            OPCNAMESPACETYPE cOpcNameSpace = OPCNAMESPACETYPE.OPC_NS_FLAT;
            m_Res = m_pIBrowse.QueryOrganization(out cOpcNameSpace);
            if (m_Res >= 0)
            {
                m_Res = m_pIBrowse.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_TO, f_name);
                if (m_Res < 0)
                    throw new Exception("Browser: Item "+f_name+" is not found.");

            }
            else
                throw new Exception("Browser: QueryOrganization error.");
        }

        private void listItems(OPCBROWSETYPE type,out List<string> items, out List<string> paths)
        {
            string szFilter="";
            items = new List<string>();
            paths = new List<string>();
            int m_Res;
            m_Res = 0;
            object pIEnumString;

            m_Res = m_pIBrowse.BrowseOPCItemIDs(type, szFilter, (int)VarEnum.VT_EMPTY, 0, out pIEnumString);
            if (m_Res >= 0 && pIEnumString != null)
            {
                if (pIEnumString is UCOMIEnumString)
                {
                    int celt = 1;
                    string[] rgelt = new string[1];
                    int p_celtFetched = 0;
                    UCOMIEnumString stringArray = (UCOMIEnumString)pIEnumString;

                    stringArray.Reset();
                    int ret = stringArray.Next(celt, rgelt, out p_celtFetched);

                 
                    while (p_celtFetched > 0)
                    {  
                        string path = getFullPath(rgelt[0]);
                        items.Add(rgelt[0]);
                        paths.Add(path);
                        celt = 1;
                        p_celtFetched = 0;
                        stringArray.Next(celt, rgelt, out p_celtFetched);
                    }
                }
                else
                    throw new Exception("Browser: IEnumString is not found.");
            }
            else
                throw new Exception("Browser: Enumeration Items error.");
        }

        //IP.Funcs.Alaram.GetAlarms()
        private void callFunctionWithParameter(string funciton, List<object> inValues, out MyVariant[] outValues)
        {
            //function param
            List<string> inPar;
            List<string> outPar;
            this.getFunctionParam(funciton, out inPar, out outPar);
            int[] opcHandle;
            VarEnum[] arrayType;

            if (inValues.Count != inPar.Count)
                    throw new Exception("Count of parameters must be equal "+inPar.Count.ToString()+".");

            //write in
            inPar.Add(funciton);
            //add items
            server.AddItems(inPar.ToArray(), out opcHandle, out arrayType);
            //create myVariant array
            List<MyVariant> values= new List<MyVariant>();
            for (int i = 0; i < arrayType.Length; i++)
            {
                if(i< arrayType.Length-1)
                    values.Add(new MyVariant(inValues[i], arrayType[i]));
                else
                    values.Add(new MyVariant(0, arrayType[i]));
            }
            //write
            server.WriteItems(inPar.ToArray(), opcHandle, values.ToArray());
            //delete item
            server.DeleteItems(inPar.ToArray(), opcHandle);

            //read out
            if (outPar.Count > 0)
            {
                //add items
                server.AddItems(outPar.ToArray(), out opcHandle, out arrayType);
                //read
                server.ReadItems(outPar.ToArray(), opcHandle, arrayType, out outValues);
                //delete item
                server.DeleteItems(outPar.ToArray(), opcHandle);
            }
            else
                outValues = null;
        }

        private void getFunctionParam(string function,out List<string> inPar, out List<string> outPar)
        {
            inPar = new List<string>();
            outPar = new List<string>();
            List<string> param;
            List<string> temp;
            setPosition(function);
            this.listItems(OPCBROWSETYPE.OPC_FLAT,out param,out temp);
            foreach (string item in param)
            {
                if (item.Contains("In."))
                    inPar.Add(function+ "." + item);
                else if (item.Contains("Out."))
                    outPar.Add(function + "." + item);
            }
        }

        private MyServer server;
        private IOPCBrowseServerAddressSpace m_pIBrowse;

        // asynchrone operation-----------------------------------------------------
        volatile bool abort = false;
        public void Abort() { abort = true; }
        private string found;
        private string selectedItem;
       /* private SVs.FindItem findItem;
        public void Init(string f, string s, SVs.FindItem p)
        {
            found = f;
            selectedItem = s;
            findItem = p;
            abort = false;
        }

        public void Find()
        {
            try
            {
                find(found, selectedItem, findItem);
            }
            catch (Exception) {}
            finally
            {
                findItem.FinallyBlock();
            }
        }

        private void find(string found, string start, SVs.FindItem parent)
        {
            CheckAbort();
            setPosition(start);
            List<string> path1;
            List<string> path2;
            List<string> childs1;
            List<string> childs2;
            listItems(OPCBROWSETYPE.OPC_BRANCH, out childs1, out  path1);
            listItems(OPCBROWSETYPE.OPC_LEAF, out childs2, out  path2);
            //find
            for(int i=0; i< childs1.Count; i++)
            {
                if (childs1[i].Contains(found) == true)
                    parent.MyList.Items.Add(path1[i]);
            }
            for (int i=0; i < childs2.Count; i++)
            {
                if (childs2[i].Contains(found) == true)
                    parent.MyList.Items.Add(path2[i]);
            }
            //iterace
            foreach (string item in path1)
            {
                find(found, item, parent);
            }
            
        }*/
        private void CheckAbort() { if (abort) Thread.CurrentThread.Abort(); }
        //end asynchrone
    }
}
