using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace RueHelper
{
    public class AnswersCollection
    {
        private const string DLL_NAME = "AnswersCollection.dll";

        public enum CALLBACK_MSG
        {
            MSG_PULLEDOUT = 1,
            MSG_TEST_DATA = 2,
            MSG_ANSWER_DATA = 3,
            MSG_ATTENCE_DATA = 4,
            MSG_REGISTER_DATA = 5,
            MSG_ERROR = 6,
            MSG_VRITIME = 7,
            MSG_DOUTE_DATA = 9
        };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackDelegate(int device, CALLBACK_MSG msg, int param1, string param2);

        [DllImport(DLL_NAME, EntryPoint = "HX_Init", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_Init();

        [DllImport(DLL_NAME, EntryPoint = "HX_QueryReaderID", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_QueryReaderID(ref int ReaderID);

        [DllImport(DLL_NAME, EntryPoint = "HX_Release", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_Release();

        [DllImport(DLL_NAME, EntryPoint = "HX_EnumDevices", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_EnumDevices(StringBuilder sComs);

        [DllImport(DLL_NAME, EntryPoint = "HX_OpenDevice", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_OpenDevice(string com);

        [DllImport(DLL_NAME, EntryPoint = "HX_CloseDevice", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_CloseDevice();

        [DllImport(DLL_NAME, EntryPoint = "HX_SetCallbackAddr", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_SetCallbackAddr(CallbackDelegate callback_addr);

        [DllImport(DLL_NAME, EntryPoint = "HX_UpdateTime", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_UpdateTime();

        [DllImport(DLL_NAME, EntryPoint = "HX_Start", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_Start();


        [DllImport(DLL_NAME, EntryPoint = "HX_Stop", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_Stop();

        [DllImport(DLL_NAME, EntryPoint = "HX_GetFirmwareVer", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_GetFirmwareVer(int device, out byte major, out byte minor);

        [DllImport(DLL_NAME, EntryPoint = "HX_GetMiddlewareVer", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_GetMiddlewareVer(out byte major, out byte minor);

        [DllImport(DLL_NAME, EntryPoint = "HX_SetWorkMode", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_SetWorkMode(TBModeDef mode, string param);

        [DllImport(DLL_NAME, EntryPoint = "HX_EnableWhitelist", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_EnableWhitelist(int bEnable);

        [DllImport(DLL_NAME, EntryPoint = "HX_AddtoWhitelist", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_AddtoWhitelist(string cardid);

        [DllImport(DLL_NAME, EntryPoint = "HX_RemovefromWhitelist", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_RemovefromWhitelist(string cardid);

        [DllImport(DLL_NAME, EntryPoint = "HX_GetWhitelist", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_GetWhitelist(int[] TagID);

        [DllImport(DLL_NAME, EntryPoint = "HX_UnlockRegister", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_UnlockRegister(string cardid);

        [DllImport(DLL_NAME, EntryPoint = "HX_StartRegister", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_StartRegister();

        [DllImport(DLL_NAME, EntryPoint = "HX_StopRegister", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_StopRegister();

        [DllImport(DLL_NAME, EntryPoint = "HX_ConcurrentTest", CallingConvention = CallingConvention.StdCall)]
        internal static extern int HX_ConcurrentTest(int status);

    }

    public enum TBModeDef
    {
        HX_MODE_NONE = 0, 	//待机模式
        HX_MODE_SINGLE = 1,	//单题模式
        HX_MODE_SINGLE_judge = 0x81, //单题模式判断
        HX_MODE_SINGLE_MUL = 0x41,	//单题模式多选
        HX_MODE_RAISE = 0xC1,	//单题模式抢答（举手）
        HX_MODE_MULTI = 2,	//多题模式
        HX_MODE_PAPER = 3		//套卷模式
    }
}