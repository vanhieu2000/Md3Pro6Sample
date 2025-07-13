using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Md3Pro6Sample
{   
    public enum MMLReturn : short
    {
        EM_INTERNAL = -1,
        EM_OK = 0,
        EM_NODE = 1,
        EM_HANDFULL = 2,
        EM_HANDLE = 3,
        EM_DATA = 4,
        EM_FLIB = 5,
        EM_OPTION = 6,
        EM_BUSY = 7,
        EM_NOREPLY = 8,
        EM_REJECT = 9,
        EM_PARA = 10,
        EM_MODE = 11,
        EM_WIN32 = 12,
        EM_WINSOCK = 13,
        EM_PROTECT = 14,
        EM_BUFFER = 15,
        EM_ALARM = 16,
        EM_RESET = 17,
        EM_FUNC = 18,
        EM_DISCONNECT = 19,
        EM_SEARCHED = 20,
    }

    public enum Pro6ToolDataItem
    {
        Magazine = 1,
        Pot = 2,
        PTN = 4,
        FTN = 5,
        ITN = 6,
        Order = 7,
        ThroughSpindleEnable = 9,
        TrroughSpindleRemovalTime = 10,
        AtcSpeed = 11,
        AlarmEfective = 14,
        TotalCuttor = 15,
        EmptyPot = 18,
        IrregulShape = 19,
        CommandedTCode = 20,				//T code executed when the pot is called
        ToolSize = 25,						//size of tool (0:Standard, 1:Middle 2;Large 3:Extra Large)
        TscRemovalType = 26,				//TSC Removal Type (0:Draw Back, 1:Air Discharge)
        CheckH = 28,						//Tool Length for Check(Atc Magazine Interference/3D Check) --TOOL_DATA_V05-- changed number 'MergedAlarmFlag = 28, CheckH = 29, CheckD = 30'
        CheckD = 29,						//Tool Radius/Diameter for Check(Atc Magazine Interference/3D Check)
        TscFrequency = 34,      	        //Through Spindle Coolant Frequency
        TscFlowCheckEnable = 35,
        AirSpindleDataNo = 50,
        SeatCheck = 57,

        StdLength = 502,
        StdDiameter = 503,
        StdLenMaxTolerance = 504,
        StdLenMinTolerance=505,
        StdDiaMaxTolerance=506,
        StdDiaMinTolerance=507,
        ProhibitFlag=508,
    
        MaxToolLength=511,
        MinToolLength=512,
        MaxToolRadius=513,
        MinToolRadius=514,
        

        //Professional 6's Cutter data item
        CuttorNo = 101,
        Kind = 102,
        HGometroy = 103,
        HWear = 104,
        DGeometry = 105,
        DWear = 106,
        ManageLifeTime = 107,
        LifeTime = 108,
        LifeWarning = 109,
        ActualLife = 110,
        ManageLifeDist = 111,
        LifeDistance = 112,
        LifeDistanceWarn = 113,
        LifeDistanceActual = 114,
        ManageLifeCount = 115,
        LifeCount = 116,
        LifeCountWarn = 117,
        LifeCountActual = 118,
        SLUpperRate = 119,
        SLLowerRate = 120,
        ACRate = 121,
        AlarmFlag = 122,
        WarningFlag = 123,
        BTSLength = 127,
        OperatorCall = 128,
        FirstUse = 129,
        SpindleSpeedLimit = 132,
        SurfaceSpeedLimit = 134,	//Spindle rotation speed limitation value by surface speed [0.01m/s]
        Teeth = 138,
        GeometryR = 139,
        WearR = 140,
        RadialMaxCLCutter = 141,
        AxialMaxCLCutter = 142,
        CutType=163,

        ZShift=601,
        RadiusShift=602,
        XShift=603,
        YShift=604,
        ContinuousMeasuringLength=605,
        ContinuousMeasuringRadius=606,
        MeasurementSpindleSpeed=607,
        MeasurementWarmingUp=608,
        CuttingTime=609,
        CuttingDist=610,
        BTSMotiontype =633
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public ushort year;
        public ushort month;
        public ushort dyaOfWeek;
        public ushort day;
        public ushort hour;
        public ushort minuite;
        public ushort second;
        public ushort milliseconds;
    }


    internal sealed class MMLib
    {
        private MMLib() { }

        public static byte DLL_TRUE = 1;
        public static byte DLL_FALSE = 0;


        # region Md3Pro6.dll Member


        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_GetLastError")]
        public static extern MMLReturn md3pro6_GetLastError(out int mainErr, out int subErr);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_alloc_handle")]
        public static extern MMLReturn md3pro6_alloc_handle(out uint handle, string nodeInfo, int sendTimout, int recvTimeout, int noop, byte logLevel);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_free_handle")]
        public static extern MMLReturn md3pro6_free_handle(uint handle);        

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_chk_system_mode")]
        public static extern MMLReturn md3pro6_chk_system_mode(uint handle, out byte  sysMode);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_system_cycle_start")]
        public static extern MMLReturn md3pro6_system_cycle_start(uint handle, byte opeCall);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_chk_system_mach_finish")]
        public static extern MMLReturn md3pro6_chk_system_mach_finish(uint handle, out byte finish, out uint finCondition);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_chk_mc_alarm")]
        public static extern MMLReturn md3pro6_chk_mc_alarm(uint handle, out uint totalAlarm, out uint totalWarn);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_max_alarm_history")]
        public static extern MMLReturn md3pro6_max_alarm_history(uint handle, out uint maxAlarm);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_mc_alarm")]
        public static extern MMLReturn md3pro6_mc_alarm(uint handle, [In, Out] uint[] alarmNo, [In, Out] byte[] alarmType, [In, Out] byte[] seriosLevel, [In, Out]byte[] poutDisable, [In, Out]byte[] cycleStartDisable, [In, Out] byte[] retryDisable, [In, Out] byte[] failedNcReset, [In, Out] SystemTime[] alarmDate, ref uint arraySize, StringBuilder message);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_mc_alarm_history")]
        public static extern MMLReturn md3pro6_mc_alarm_history(uint handle, [In, Out] uint[] alarmNo, [In, Out] byte[] alarmType, [In, Out] byte[] seriosLevel, [In, Out] byte[] retryDisable, [In, Out] SystemTime[] alarmDate, [In, Out] SystemTime[] resetDate, ref uint arraySize, StringBuilder message);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_max_atc_magazine")]
        public static extern MMLReturn md3pro6_max_atc_magazine(uint handle, out uint actualMgzn, out int outMcMgzn);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_atc_magazine_info")]
        public static extern MMLReturn md3pro6_atc_magazine_info(uint handle, uint mgznNo, out uint maxPot, out int mgznType, out uint emptyPot);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_tool_info")]
        public static extern MMLReturn md3pro6_tool_info(uint handle, out int inchUnit, out uint ftnFig, out uint itnFig, out uint ptnFig, out uint manageType);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_spindle_tool")]
        public static extern MMLReturn md3pro6_spindle_tool(uint handle, out uint mgznNo, out int pot, out uint cutter, out uint ftn, out uint itn, out uint ptn);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_next_tool")]
        public static extern MMLReturn md3pro6_next_tool(uint handle, out uint mgznNo, out int potNo, out uint cutterNo, out uint ftn, out uint itn, out uint ptn);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_tls_potno")]
        public static extern MMLReturn md3pro6_tls_potno(uint handle, uint tlsNo, out uint mgznNo, out int potNo);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_get_tool_data_item")]
        public static extern MMLReturn md3pro6_get_tool_data_item(uint handle, uint item, [In] uint[] mgznNo, [In] int[] potNo, [In, Out] int[] value, uint sumArray);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_set_tool_data_item")]
        public static extern MMLReturn md3pro6_set_tool_data_item(uint handle, uint item, [In] uint[] mgznNo, [In] int[] potNo, [In] int[] value, uint sumArray);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_get_cutter_data_item")]
        public static extern MMLReturn md3pro6_get_cutter_data_item(uint handle, uint item, [In] uint[] mgznNo, [In] int[] potNo, [In] uint[] cutterNo, [In, Out] int[] value, uint sumArray);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_set_cutter_data_item")]
        public static extern MMLReturn md3pro6_set_cutter_data_item(uint handle, uint item, [In] uint[] mgznNo, [In] int[] potNo, [In] uint[] cutterNo, [In] int[] value, uint sumArray);

        [DllImport("Md3Pro6.dll", EntryPoint = "md3pro6_clear_tool_data")]
        public static extern MMLReturn md3pro6_clear_tool_data(uint handle, [In] uint[] mgznNo, [In] int[] potNo, uint sumArray);

        # endregion
    }
}
