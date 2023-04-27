namespace FSHLib
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct FSHEntryHeader
    {
        public int code;
        public short width;
        public short height;
        public short[] misc;
    }
}

