namespace FSHLib
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct FSHHeader
    {
        public byte[] SHPI;
        public int size;
        public int numBmps;
        public byte[] dirID;
    }
}

