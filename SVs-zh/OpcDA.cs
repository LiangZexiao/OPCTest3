using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace OpcDA
{
    public enum OPCDATASOURCE
    {
        OPC_DS_CACHE = 1,
        OPC_DS_DEVICE = 2
    }

    public enum OpcServerState
    {
        Failed = 2,
        NoConfig = 3,
        Running = 1,
        Suspended = 4,
        Test = 5
    }

    public enum OPCNAMESPACETYPE
    {
        OPC_NS_FLAT = 2,
        OPC_NS_HIERARCHIAL = 1
    }

    public enum OPCBROWSEDIRECTION
    {
        OPC_BROWSE_DOWN = 2,
        OPC_BROWSE_TO = 3,
        OPC_BROWSE_UP = 1
    }

    public enum OPCBROWSETYPE
    {
        OPC_BRANCH = 1,
        OPC_FLAT = 3,
        OPC_LEAF = 2
    }

    [Flags]
    public enum OPCACCESSRIGHTS
    {
        OPC_UNKNOWN,
        OPC_READABLE,
        OPC_WRITEABLE,
        OPC_READWRITEABLE
    }

    public class OPCItemState
    {
        public object DataValue;
        public int Error;
        public int HandleClient;
        public short Quality;
        public long TimeStamp;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
    public class SERVERSTATUS
    {
        public long ftStartTime;
        public long ftCurrentTime;
        public long ftLastUpdateTime;
        [MarshalAs(UnmanagedType.U4)]
        public OpcServerState eServerState;
        public int dwGroupCount;
        public int dwBandWidth;
        public short wMajorVersion;
        public short wMinorVersion;
        public short wBuildNumber;
        public short wReserved;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szVendorInfo;
    }

    [ComImport, ComVisible(true), Guid("39c13a4d-011e-11d0-9675-0020afd8adb3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

    public interface IOPCServer
    {
        [PreserveSig]
        int AddGroup([In, MarshalAs(UnmanagedType.LPWStr)] string szName, [In, MarshalAs(UnmanagedType.Bool)] bool bActive, [In] int dwRequestedUpdateRate, [In] int hClientGroup, [In] IntPtr pTimeBias, [In] IntPtr pPercentDeadband, [In] int dwLCID, out int phServerGroup, out int pRevisedUpdateRate, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk);
        [PreserveSig]
        int GetErrorString([In] int dwError, [In] int dwLocale, [MarshalAs(UnmanagedType.LPWStr)] out string ppString);
        void GetGroupByName([In, MarshalAs(UnmanagedType.LPWStr)] string szName, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk);
        [PreserveSig]
        int GetStatus([MarshalAs(UnmanagedType.LPStruct)] out SERVERSTATUS ppServerStatus);
        [PreserveSig]
        int RemoveGroup([In] int hServerGroup, [In, MarshalAs(UnmanagedType.Bool)] bool bForce);
        [PreserveSig]
        int CreateGroupEnumerator([In] int dwScope, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("39c13a4f-011e-11d0-9675-0020afd8adb3"), ComVisible(true)]
    public interface IOPCBrowseServerAddressSpace
    {
        [PreserveSig]
        int QueryOrganization([MarshalAs(UnmanagedType.U4)] out OPCNAMESPACETYPE pNameSpaceType);
        [PreserveSig]
        int ChangeBrowsePosition([In, MarshalAs(UnmanagedType.U4)] OPCBROWSEDIRECTION dwBrowseDirection, [In, MarshalAs(UnmanagedType.LPWStr)] string szName);
        [PreserveSig]
        int BrowseOPCItemIDs([In, MarshalAs(UnmanagedType.U4)] OPCBROWSETYPE dwBrowseFilterType, [In, MarshalAs(UnmanagedType.LPWStr)] string szFilterCriteria, [In, MarshalAs(UnmanagedType.U2)] short vtDataTypeFilter, [In, MarshalAs(UnmanagedType.U4)] OPCACCESSRIGHTS dwAccessRightsFilter, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk);
        [PreserveSig]
        int GetItemID([In, MarshalAs(UnmanagedType.LPWStr)] string szItemDataID, [MarshalAs(UnmanagedType.LPWStr)] out string szItemID);
        [PreserveSig]
        int BrowseAccessPaths([In, MarshalAs(UnmanagedType.LPWStr)] string szItemID, [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk);
    }

    [ComImport, Guid("39c13a72-011e-11d0-9675-0020afd8adb3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
    public interface IOPCItemProperties
    {
        [PreserveSig]
        int QueryAvailableProperties([In, MarshalAs(UnmanagedType.LPWStr)] string szItemID, out int dwCount, out IntPtr ppPropertyIDs, out IntPtr ppDescriptions, out IntPtr ppvtDataTypes);
        [PreserveSig]
        int GetItemProperties([In, MarshalAs(UnmanagedType.LPWStr)] string szItemID, [In] int dwCount, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] pdwPropertyIDs, out IntPtr ppvData, out IntPtr ppErrors);
        [PreserveSig]
        int LookupItemIDs([In, MarshalAs(UnmanagedType.LPWStr)] string szItemID, [In] int dwCount, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] pdwPropertyIDs, out IntPtr ppszNewItemIDs, out IntPtr ppErrors);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Unicode)]
    internal class OPCITEMDEF
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szAccessPath;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string szItemID;

        [MarshalAs(UnmanagedType.Bool)]
        public bool bActive;

        public int hClient;
        public int dwBlobSize;
        public IntPtr pBlob;

        public short vtRequestedDataType;

        public short wReserved;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class OPCITEMRESULT
    {
        public int hServer = 0;
        public short vtCanonicalDataType = 0;
        public short wReserved = 0;

        [MarshalAs(UnmanagedType.U4)]
        public OPCACCESSRIGHTS dwAccessRights = 0;

        public int dwBlobSize = 0;
        public int pBlob = 0;
    };

    [ComVisible(true), ComImport,
    Guid("39c13a54-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOPCItemMgt
    {
        [PreserveSig]
        int AddItems(
            [In]											int dwCount,
            [In]											IntPtr pItemArray,
            [Out]										out IntPtr ppAddResults,
            [Out]										out	IntPtr ppErrors);

        [PreserveSig]
        int ValidateItems(
            [In]											int dwCount,
            [In]											IntPtr pItemArray,
            [In, MarshalAs(UnmanagedType.Bool)]			bool bBlobUpdate,
            [Out]										out	IntPtr ppValidationResults,
            [Out]										out	IntPtr ppErrors);

        [PreserveSig]
        int RemoveItems(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int SetActiveState(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In, MarshalAs(UnmanagedType.Bool)]						bool bActive,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int SetClientHandles(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phClient,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int SetDatatypes(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In]														IntPtr pRequestedDatatypes,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int CreateEnumerator(
            [In]										ref Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);

    }	// interface IOPCItemMgt

    [ComVisible(true), ComImport,
    Guid("39c13a52-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOPCSyncIO
    {
        [PreserveSig]
        int Read(
            [In, MarshalAs(UnmanagedType.U4)]							OPCDATASOURCE dwSource,
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]	int[] phServer,
            [Out]													out IntPtr ppItemValues,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int Write(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	object[] pItemValues,
            [Out]													out	IntPtr ppErrors);

    }	// interface IOPCSyncIO


    [ComImport, Guid("39c13a70-011e-11d0-9675-0020afd8adb3"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

    internal interface IOPCDataCallback
    {
        void OnDataChange([In] int dwTransid, [In] int hGroup, [In] int hrMasterquality, [In] int hrMastererror, [In] int dwCount, [In] IntPtr phClientItems, [In] IntPtr pvValues, [In] IntPtr pwQualities, [In] IntPtr pftTimeStamps, [In] IntPtr ppErrors);
        void OnReadComplete([In] int dwTransid, [In] int hGroup, [In] int hrMasterquality, [In] int hrMastererror, [In] int dwCount, [In] IntPtr phClientItems, [In] IntPtr pvValues, [In] IntPtr pwQualities, [In] IntPtr pftTimeStamps, [In] IntPtr ppErrors);
        void OnWriteComplete([In] int dwTransid, [In] int hGroup, [In] int hrMastererr, [In] int dwCount, [In] IntPtr pClienthandles, [In] IntPtr ppErrors);
        void OnCancelComplete([In] int dwTransid, [In] int hGroup);
    } //IOPCDataCallback

}
