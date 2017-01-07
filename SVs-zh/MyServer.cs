using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using OpcDA;
using ole32;
using System.IO;
using System.Windows.Forms;
using SVs;
using System.Runtime.InteropServices.ComTypes;
using System.Net;
using KebaOpcAddOns;


namespace OpcCommander
{
    // A delegate type for CallbackEvent
    public delegate void CallBackEventHandler(object sender, CallBackEvent e);

    public class MyServer:IOPCDataCallback
    {
        public MyServer(main par)
        {
            this.parent = par;
        }
        public static IntPtr alloc(object id)
        {
	        IntPtr ptrdef = Marshal.AllocCoTaskMem(Marshal.SizeOf(id));       
            Marshal.StructureToPtr(id, ptrdef, true);
            return ptrdef;
        }

        public static object unAlloc(IntPtr p,Type t)
        {
            object ret_struct = Marshal.PtrToStructure(p, t);
            return ret_struct;
        }

        public static void dealloc(IntPtr ptrdef, Type id)
        {
            Marshal.DestroyStructure(ptrdef, id);
            Marshal.FreeCoTaskMem(ptrdef);
        }

        public void Start()
        {
            //var test=Dns.GetHostName();
            //parent.PrintMessage("Server: Start", MessageStyl.info);
            try
            {
                // Re-intialize Multi-Query Interface:
                for (int i = 0; i < m_arrMultiQI.Length; i++)
                {
                    m_arrMultiQI[i].pItf = null;
                    m_arrMultiQI[i].hr = 0;
                } 
                //IID
                Guid IID_IOPCSERVER = new Guid("39c13a4d-011e-11d0-9675-0020afd8adb3");
                Guid IID_IOPCBROWSE = new Guid("39c13a4f-011e-11d0-9675-0020afd8adb3");
                Guid IID_IOPCItemProperties = new Guid("39c13a72-011e-11d0-9675-0020afd8adb3");
                  Guid IID_IOConfig=new Guid("B1FF6759-F04F-11d4-8315-009027578FBB");//alex
                //Guid IID_IOConfig=new Guid("b1ff6759-f04f-11d4-8315-009027578fbb");//alex
                //alloc IID
                m_arrMultiQI[MQI_IOPCSERVER].pIID = alloc(IID_IOPCSERVER);
                m_arrMultiQI[MQI_IOPCBROWSE].pIID = alloc(IID_IOPCBROWSE);
                m_arrMultiQI[MQI_IOPCItemProperties].pIID = alloc(IID_IOPCItemProperties);
                m_arrMultiQI[MQI_IKEBAOPCCONFIG].pIID = alloc(IID_IOConfig);//alex
                //Kemro.opc CLSI
                Guid t_clsid;
                ole32.funciton.CLSIDFromProgID("Kemro.opc.4.IF1", out t_clsid);
                //parent.PrintMessage("ProgID for Kemro.opc.4.IF1 is founded", MessageStyl.info);
                //create com objects
                //parent.PrintMessage("Create COM objects:", MessageStyl.info);
                ole32.funciton.CoCreateInstanceEx(t_clsid, null, ole32.CLSCTX.CLSCTX_LOCAL_SERVER, IntPtr.Zero, (uint)m_arrMultiQI.Length, m_arrMultiQI);
                //dealoc IID
                dealloc(m_arrMultiQI[MQI_IOPCSERVER].pIID, IID_IOPCSERVER.GetType());
                dealloc(m_arrMultiQI[MQI_IOPCBROWSE].pIID, IID_IOPCBROWSE.GetType());
                dealloc(m_arrMultiQI[MQI_IOPCItemProperties].pIID, IID_IOPCItemProperties.GetType());
                dealloc(m_arrMultiQI[MQI_IKEBAOPCCONFIG].pIID, IID_IOConfig.GetType());//==alex
                //IOPCServer interface pointer:
                m_pIServer = (IOPCServer)m_arrMultiQI[MQI_IOPCSERVER].pItf;
                if(m_pIServer != null)
                {
                    //SERVERSTATUS status;
                  //  m_pIServer.GetStatus(out status);
                    //parent.PrintMessage("IOPCServer: Creating instance is successful", MessageStyl.succes);
                  //  parent.PrintMessage("IOPCServer: status = " + status.eServerState.ToString(), MessageStyl.info);
                } 
                else
                    throw new Exception("IOPCServer: Creating instance failed");

                //IOPCBrowser interface pointer:
                m_pIBrowse = (IOPCBrowseServerAddressSpace)m_arrMultiQI[MQI_IOPCBROWSE].pItf;
                /*if(m_pIBrowse != null)
                    //parent.PrintMessage("IOPCBrowser: Creating instance is successful", MessageStyl.succes);
                else
                    throw new Exception("IOPCBrowser: Creating instance failed");

                //IOPCItemProperties interface pointer:
                m_pIProperties = (IOPCItemProperties)m_arrMultiQI[MQI_IOPCItemProperties].pItf;
                if (m_pIProperties != null)
                    //parent.PrintMessage("IOPCItemProperties: Creating instance is successful", MessageStyl.succes);
                else
                    throw new Exception("IOPCItemProperties: Creating instance failed");
                 */
                //IOPCCOnfig
                if (m_arrMultiQI[MQI_IKEBAOPCCONFIG].hr >= 0)
                {
                    m_pOpcConfig = (IKebaOpcConfig)m_arrMultiQI[MQI_IKEBAOPCCONFIG].pItf;
                    
                    if (m_pOpcConfig != null)
                    {
                        //parent.PrintMessage("IKebaOpcConfig: Creating instance is successful", MessageStyl.succes);

                    }
                    else
                        throw new Exception("IKebaOpcConfig: Creating instance failed");
                }
                //add Group
                long lBias = 0;
                float fDeadband = 0;
				int dwRevUpdateRate   = 0;
                Guid IID_IUknown = new Guid("00000000-0000-0000-C000-000000000046");
                int hr = m_pIServer.AddGroup("default", true, 100, 1, alloc(lBias), alloc(fDeadband),
                    1033, out hGroup, out dwRevUpdateRate, ref IID_IUknown, out pUnknown);
                if (hr < 0)
                    throw new Exception("Group: error hr =" + hr.ToString());
                
                //connetion points data call back
                m_pIConnPointCont = (IConnectionPointContainer)pUnknown;
                Type sinktype = typeof(IOPCDataCallback);
                Guid sinkguid = sinktype.GUID;

                m_pIConnPointCont.FindConnectionPoint(ref sinkguid, out callBackPoint);
                if(callBackPoint!=null)
                    callBackPoint.Advise(this, out cbCookies); 
                else
                    throw new Exception("IOPCDataCallback: The connection point cannot be found");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Stop()
        {
           
            if (m_pIServer != null)
            {
                m_pIServer.RemoveGroup(hGroup, false);
                Marshal.ReleaseComObject(this.m_pIServer);
                m_pIServer = null;
            }
            if (m_pIBrowse != null)
            {
                Marshal.ReleaseComObject(this.m_pIBrowse);
                m_pIBrowse = null;
            }

            if (m_pIProperties != null)
            {
                Marshal.ReleaseComObject(this.m_pIProperties);
                m_pIProperties = null;
            }
            if (callBackPoint != null)
            {
                callBackPoint.Unadvise(cbCookies);
                callBackPoint = null;
            }
            //parent.PrintMessage("Server: Stop", MessageStyl.info);
        }

        public void AddItems(string[] itemID, out int[] ItemHANDLES, out VarEnum[] arrayType)
        {
            int m_Res = 0;
            try
            {
                int dwCount=itemID.Length;
	            IntPtr pErrors;
                IntPtr pResults;
	            IOPCItemMgt m_pIItemMgt = (IOPCItemMgt) pUnknown;
                //ItemMgt: OK
                OPCITEMDEF pItem = new OPCITEMDEF();

                //set pItem
                int temp = 0;
                pItem.bActive = true;
                pItem.szAccessPath = null;
                pItem.hClient = temp;
                pItem.dwBlobSize = 0;				// no blob support
                pItem.pBlob = IntPtr.Zero;
                pItem.vtRequestedDataType = (int)VarEnum.VT_EMPTY;
                pItem.wReserved = 0;

	            //alloc memory
                IntPtr ptrDef = Marshal.AllocCoTaskMem(dwCount * Marshal.SizeOf(pItem));
		        int	runDef = (int) ptrDef;

                for(int i=0;i<dwCount;i++)
			    {
				    pItem.szItemID=itemID[i];
                    Marshal.StructureToPtr( pItem , (IntPtr) runDef, false );
                    runDef += Marshal.SizeOf(pItem);
			    }
                //add item
                m_Res=m_pIItemMgt.AddItems(dwCount,ptrDef, out pResults, out pErrors);

                //dealoc memory
                runDef = (int)ptrDef;
                for (int i = 0; i < dwCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr)runDef, pItem.GetType());
                    runDef += Marshal.SizeOf(pItem);
                }
                Marshal.FreeCoTaskMem(ptrDef);

                if(m_Res < 0 )
                    throw new Exception("Server: Add Items failed hr= " + m_Res.ToString());

                //read result
                int	runRes = (int) pResults;
		        int	runErr = (int) pErrors;
		        if( (runRes == 0) || (runErr == 0) )
                    throw new Exception("Server: Add Items failed hr= " + unchecked((int)0x80004004).ToString());

                //init output arry
                ItemHANDLES= new int[dwCount];
                arrayType = new VarEnum [dwCount];
                int error;
                for (int i = 0; i < dwCount; i++)
                {
                    error = Marshal.ReadInt32((IntPtr)runErr);
                    if (error < 0)
                    {
                        //parent.PrintMessage("Server: Add Item " + itemID[i] + " is failed hr =" + error.ToString(), MessageStyl.error);
                        ItemHANDLES[i] = 0;
                        arrayType[i] = VarEnum.VT_EMPTY;
                    }
                    else
                    {
                        ItemHANDLES[i] = Marshal.ReadInt32((IntPtr)runRes);
                        arrayType[i] = (VarEnum)(int)Marshal.ReadInt16((IntPtr)(runRes + 4));
                        //set server handle same as client handle
                        int[] tempHandle = new int[1];
                        tempHandle[0] = ItemHANDLES[i];
                        IntPtr setClientError;
                        m_pIItemMgt.SetClientHandles(1, tempHandle, tempHandle, out setClientError);
                        int s_err=Marshal.ReadInt32(setClientError);
                        //if (s_err < 0) parent.PrintMessage("Server: Add Item " + itemID[i] + " : Set the client handle is failed hr =" + s_err.ToString(), MessageStyl.error); 
                    }

                    runRes += Marshal.SizeOf(new OPCITEMRESULT());
                    runErr += 4;
                }

                Marshal.FreeCoTaskMem(pResults);
                Marshal.FreeCoTaskMem(pErrors);
            }
            catch (Exception e)
            {
                //parent.PrintMessage(e.Message, MessageStyl.error);
                throw e;
            }
        }

        public void DeleteItems(string[] id, int[] ItemHANDLES)
        {
            int m_Res = 0;
            try
            {
                int dwCount = ItemHANDLES.Length;
                IntPtr pErrors;
                IOPCItemMgt m_pIItemMgt = (IOPCItemMgt)pUnknown;
               
                //add item
                m_Res=m_pIItemMgt.RemoveItems(dwCount, ItemHANDLES, out pErrors);

                if (m_Res <0)
                    throw new Exception("Server: Delete Items failed hr= "+ m_Res.ToString());

                int runErr = (int)pErrors;
                if (runErr == 0)
                    throw new Exception("Server: Delete Items failed hr= " + unchecked((int)0x80004004).ToString());

                int error;
                for (int i = 0; i < dwCount; i++)
                {
                    error = Marshal.ReadInt32((IntPtr)runErr);
                    if (error < 0)
                        //parent.PrintMessage("Server: Delete Item " + id[i] + " is failed hr =" + error.ToString(), MessageStyl.error);
                    runErr += 4;
                }
                Marshal.FreeCoTaskMem(pErrors);
            }
            catch (Exception e)
            {
                //parent.PrintMessage(e.Message, MessageStyl.error);
                throw e;
            }
        }

        public void ReadItems(string[] id, int[] ItemHANDLES,VarEnum[] arrayType, out MyVariant[] returnValue)
        {
	        int m_Res;
	        try
            {
	            IntPtr		ptrStat;
		        IntPtr		ptrErr;
                int dwCount = ItemHANDLES.Length;
	            IOPCSyncIO m_pISync= (IOPCSyncIO) pUnknown;

                m_Res = m_pISync.Read(OPCDATASOURCE.OPC_DS_DEVICE, dwCount, ItemHANDLES, out ptrStat, out ptrErr);
                if (m_Res < 0)
                    throw new Exception("Server: Read Items failed hr= " + m_Res.ToString());

                int runErr = (int)ptrErr;
                int runStat = (int)ptrStat;
                if ((runErr == 0) || (runStat == 0))
                    throw new Exception("Server: Read Items failed hr= " + unchecked((int)0x80004004).ToString());

                returnValue = new MyVariant[dwCount];
                for (int i = 0; i < dwCount; i++)
                {
                    int error = Marshal.ReadInt32((IntPtr)runErr);
                    runErr += 4;

                    if (error >= 0)
                    {
                       object o=Marshal.GetObjectForNativeVariant((IntPtr)(runStat + 16));
                       returnValue[i] = new MyVariant(o,arrayType[i]);
                    }
                    else
                    {
                        returnValue[i] = new MyVariant(null,VarEnum.VT_EMPTY);
                        //parent.PrintMessage("Server: Read Item " + id[i] + " is failed hr =" + error, MessageStyl.error);
                    }

                    runStat += 32;
                }

                Marshal.FreeCoTaskMem(ptrStat);
                Marshal.FreeCoTaskMem(ptrErr);
            }
            catch (Exception e)
            {
                //parent.PrintMessage(e.Message, MessageStyl.error);
                throw e;
            }
		
        }

        public void WriteItems(string[] id, int[] ItemHANDLES, MyVariant[] pValue)
        {
            int m_Res;
            try
            {
                IntPtr ptrErr;
                int dwCount = ItemHANDLES.Length;
                IOPCSyncIO m_pISync = (IOPCSyncIO)pUnknown;

                //alloc object array
                object[] values=new object[pValue.Length];
                for(int i=0; i< pValue.Length; i++)
                    values[i]=pValue[i].GetVar();

                m_Res = m_pISync.Write(dwCount,ItemHANDLES, values,out ptrErr);
                if (m_Res < 0)
                    throw new Exception("Server: Write Items failed hr= " + m_Res.ToString());

                int runErr = (int)ptrErr;
                if ((runErr == 0))
                    throw new Exception("Server: Write Items failed hr= " + unchecked((int)0x80004004).ToString());

                for (int i = 0; i < dwCount; i++)
                {
                    int error = Marshal.ReadInt32((IntPtr)runErr);
                    runErr += 4;

                    //if (error < 0) parent.PrintMessage("Server: Write Item " + id[i] + " is failed hr =" + error, MessageStyl.error);
                }
                Marshal.FreeCoTaskMem(ptrErr);
            }
            catch (Exception e)
            {
                //parent.PrintMessage(e.Message, MessageStyl.error);
                throw e;
            }

        }

        public void GetItemProperties(string id, out string[] descriptions,out MyVariant[] values)
        {
            //out
            descriptions = null;
            values = null;
            //local variable
            int count;
            IntPtr propertyIDs;
            IntPtr des;
            IntPtr dataTypes;
            IntPtr data;
            IntPtr Error;
            int ret=m_pIProperties.QueryAvailableProperties(id, out count, out propertyIDs,
                out des, out dataTypes);

            if (ret >= 0 && ((int)propertyIDs) != 0 && ((int)dataTypes) != 0
                && ((int)des) != 0)
            {
                if (count > 0)
                {
                    int[] propIDs = IntArrayFromIntPtr(propertyIDs, count);
                    VarEnum[] types = VarEnumArrayFromIntPtr(dataTypes, count);

                    ret = m_pIProperties.GetItemProperties(id, count, propIDs, out data, out Error);
                    if (ret >= 0 && ((int)data) != 0 && ((int)Error) != 0)
                    {
                        int[] err = IntArrayFromIntPtr(Error, count);
                        descriptions = StringArrayFromIntPtr(des, count);
                        values = new MyVariant[count];
                        int i=0;
                        int actData=(int)data;
                        foreach (int item in err)
                        {
                            if (item == 0)
                            {
                                object temp = Marshal.GetObjectForNativeVariant((IntPtr)(actData));
                                values[i] = new MyVariant(temp, types[i]);
                            }
                            else
                            {
                                values[i] = new MyVariant(null, VarEnum.VT_EMPTY);
                                //parent.PrintMessage("Server: Read Item Properties " + id + "(" + descriptions[i] + ")" + " is failed hr =" + item, MessageStyl.error);
                            }
                            i++;
                            actData += 16;
                        }
                        Marshal.FreeCoTaskMem(data);
                    }
                    else throw new Exception("Function GetItemProperties failed. Error code = " + ret.ToString());
                }
            }
            else throw new Exception("Function QueryAvailableProperties failed. Error code = " + ret.ToString());
        }

        public static string[] StringArrayFromIntPtr(IntPtr p, int count)
        {
            if (count > 0)
            {
                string[] ret = new string[count];
                int size = 4;
                IntPtr temp;
                int j = (int)p;
                for (int i = 0; i < count; i++)
                {   
                    temp = (IntPtr)Marshal.ReadInt32((IntPtr)j);
                    ret[i] = Marshal.PtrToStringUni(temp);
                    j += size;
                }
                Marshal.FreeCoTaskMem(p);
                return ret;
            }
            else return null;
        }

        public static VarEnum[] VarEnumArrayFromIntPtr(IntPtr p, int count)
        {
            if (count > 0)
            {
                VarEnum[] ret = new VarEnum[count];
                int size = 2;// sizeof(VarEnum);
                int j = (int)p;
                for (int i = 0; i < count; i++)
                {
                    ret[i] = (VarEnum)(int)Marshal.ReadInt16((IntPtr)(j));
                    j += size;
                }
                Marshal.FreeCoTaskMem(p);
                return ret;
            }
            else return null;
        }

        public static int[] IntArrayFromIntPtr(IntPtr p, int count)
        {
            if (count > 0)
            {
                int[] ret = new int[count];
                int size = 4;// sizeof(VarEnum);
                int j = (int)p;
                for (int i = 0; i < count; i++)
                {
                    ret[i] = Marshal.ReadInt32((IntPtr)(j));
                    j += size;
                }
                Marshal.FreeCoTaskMem(p);
                return ret;
            }
            else return null;
        }

        public static string VarEnumToString(VarEnum type)
        {
            string ret = "";
            switch (type)
            {
                case VarEnum.VT_EMPTY:
                    ret = "VT_EMPTY";
                    break;
                case VarEnum.VT_BOOL:
                    ret = "BOOL";
                    break;
                case VarEnum.VT_UI1:
                    ret = "BYTE";
                    break;
                case VarEnum.VT_I1:
                    ret = "CHAR";
                    break;
                case VarEnum.VT_I2:
                    ret = "INT";
                    break;
                case VarEnum.VT_UI4:
                    ret = "INT";
                    break;
                case VarEnum.VT_I4:
                    ret = "INT";
                    break;
                case VarEnum.VT_DECIMAL:
                    ret = "DECIMAL";
                    break;
                case VarEnum.VT_R4:
                    ret = "FLOAT";
                    break;
                case VarEnum.VT_R8:
                    ret = "DOUBLE";
                    break;
                case VarEnum.VT_BSTR:
                    ret = "STRING";
                    break;
                case VarEnum.VT_VARIANT:
                    ret = "VARIANT";
                    break;
                case VarEnum.VT_UI1 | VarEnum.VT_ARRAY:
                    ret = "BYTE [ ]";
                    break;

                case VarEnum.VT_BSTR | VarEnum.VT_ARRAY:
                    ret = "STRING [ ]";
                    break;

                case VarEnum.VT_I4 | VarEnum.VT_ARRAY:
                    ret = "INT [ ]";
                    break;

                case VarEnum.VT_BOOL | VarEnum.VT_ARRAY:
                    ret = "BOOL [ ]";
                    break;

                case VarEnum.VT_VARIANT | VarEnum.VT_ARRAY:
                    ret = "VARIANT [ ]";
                    break;
                case VarEnum.VT_UNKNOWN:
                    ret = "Unknow";
                    break;
                default:
                    ret = "Unknow (" + type.ToString() + ")";
                    break;
            }
            return ret;
        }

        //call back
        void IOPCDataCallback.OnDataChange(int dwTransid, int hGroup, int hrMasterquality, int hrMastererror, int dwCount, IntPtr phClientItems, IntPtr pvValues, IntPtr pwQualities, IntPtr pftTimeStamps, IntPtr ppErrors)
        {
            if ((dwCount != 0) && (hGroup == 1))
            {
                int count = (int)dwCount;
                int runh = (int)phClientItems;
                int runv = (int)pvValues;
                int rune = (int)ppErrors;

                int error;
                int handle;
                for (int i = 0; i < count; i++)
                {
                    error = Marshal.ReadInt32((IntPtr)rune);
                    rune += 4;

                    handle = Marshal.ReadInt32((IntPtr)runh);
                    runh += 4;

                    if (error>=0)
                    {
                        object variant = Marshal.GetObjectForNativeVariant((IntPtr)runv);
                        CallBackEvent e = new CallBackEvent(handle, variant);
                        //this event will be proccessed by ..... main.CallBackEventListener
                        //CallBack(this, e);
                       
                    }
                    runv += 16;
                }
            }
        }
        void IOPCDataCallback.OnReadComplete(int dwTransid, int hGroup, int hrMasterquality, int hrMastererror, int dwCount, IntPtr phClientItems, IntPtr pvValues, IntPtr pwQualities, IntPtr pftTimeStamps, IntPtr ppErrors) { }
        void IOPCDataCallback.OnWriteComplete(int dwTransid, int hGroup, int hrMastererr, int dwCount, IntPtr pClienthandles, IntPtr ppErrors) { }
        void IOPCDataCallback.OnCancelComplete(int dwTransid, int hGroup){ }

        public IOPCBrowseServerAddressSpace GetBrowser() { return this.m_pIBrowse; }
        public event CallBackEventHandler CallBack;
        // m_arrMultiQI indices:
        const int MQI_IOPCSERVER = 0;
        const int MQI_IOPCBROWSE = 1;
        const int MQI_IOPCItemProperties = 2;
        const int MQI_IKEBAOPCCONFIG=3;
        MULTI_QI[] m_arrMultiQI = new MULTI_QI[4];
        private object pUnknown;
        private int hGroup;
        private IOPCServer m_pIServer = null;
        private IOPCBrowseServerAddressSpace m_pIBrowse = null;
        private IOPCItemProperties m_pIProperties = null;
        private IConnectionPointContainer m_pIConnPointCont = null;
        private IConnectionPoint callBackPoint = null;
        private IKebaOpcConfig m_pOpcConfig = null;
        private int cbCookies;
        //
        private main parent;
        //public
        public IKebaOpcConfig OpcConfig
        {
            get
            {
                return m_pOpcConfig;
            }
        }
    }
}
