using System;
using System.Runtime.InteropServices;

namespace B33VirtualMachineAssembler
{
    internal static class User32
    {
        public enum Msgs
        {
            WmSetRedraw = 0x000B,
            WmPaint = 0x000F,
            EmGetSel = 0x00B0,
            EmSetSel = 0x00B1,
            EmLineScroll = 0x00B6,
            EmGetLineCount = 0x00BA,
            EmLineIndex = 0x00BB,
            EmGetFirstVisibleLine = 0x00CE,
            EmSetMargins = 0x00D3,
            EmGetMargins = 0x00D4,
            EmPosFromChar = 0x00D6,
            EmCharFromPos = 0x00D7,
            WmChar = 0x0102,
            WmUser = 0x0400,
            EmGetEventMask = (WmUser + 59),
            EmSetCharFormat = (WmUser + 68),
            EmSetEventMask = (WmUser + 69),
        }

        public const int ScfSelection = 0x0001;
        public const int EcLeftMargin = 0x0001;
        public const int EcRightMargin = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        public struct CharFormat
        {
            public int cbSize;
            public UInt32 dwMask;
            public UInt32 dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public Int32 crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szFaceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PointL
        {
            public Int32 X;
            public Int32 Y;
        }


        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd,
                                                [MarshalAs(UnmanagedType.I4)] Msgs msg,
                                                int wParam,
                                                IntPtr lParam);


        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd,
                                                [MarshalAs(UnmanagedType.I4)] Msgs msg,
                                                int wParam,
                                                int lParam);


        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wparam, IntPtr lparam);

        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wparam, int lparam);

        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessageRef(IntPtr hWnd, int msg, out int wparam, out int lparam);
    }
}
