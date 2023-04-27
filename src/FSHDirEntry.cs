namespace FSHLib
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct FSHDirEntry
    {
        public byte[] name;
        public int offset;
    }
}

