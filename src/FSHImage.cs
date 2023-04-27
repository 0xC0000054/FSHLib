namespace FSHLib
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;

    public class FSHImage
    {
        private ArrayList bitmapItems;
        private FSHDirEntry[] directory;
        private FSHHeader fshHead;
        private bool isDirty;
        private bool isFSHComp;
        private byte[] rawData;
        private bool saveGlobPal;

        public FSHImage()
        {
            this.saveGlobPal = false;
            this.rawData = null;
            this.isFSHComp = false;
            this.isDirty = false;
            this.bitmapItems = new ArrayList();
            this.fshHead = new FSHHeader();
            this.directory = new FSHDirEntry[0];
        }

        public FSHImage(Stream stream)
        {
            this.saveGlobPal = false;
            this.rawData = null;
            this.isFSHComp = false;
            this.isDirty = false;
            this.bitmapItems = new ArrayList();
            this.Load(stream);
        }

        public byte[] Comp(byte[] data, bool incLen)
        {
            int num = 0x20000;
            int num2 = num - 1;
            int num3 = 50;
            int destinationIndex = 0;
            int[,] numArray = new int[0x100, 0x100];
            int[] numArray2 = new int[num];
            int num9 = 0;
            int num10 = 0;
            int sourceIndex = 0;
            int num12 = 0;
            int length = 0;
            for (int i = 0; i < 0x100; i++)
            {
                for (int k = 0; k < 0x100; k++)
                {
                    numArray[i, k] = -1;
                }
            }
            for (int j = 0; j < num; j++)
            {
                numArray2[j] = -1;
            }
            int num5 = data.Length;
            byte[] destinationArray = new byte[num5 + 0x404];
            Array.Copy(data, 0, destinationArray, 0, num5);
            byte[] buffer = new byte[num5];
            buffer[0] = 0x10;
            buffer[1] = 0xfb;
            buffer[2] = (byte) (num5 >> 0x10);
            buffer[3] = (byte) ((num5 >> 8) & 0xff);
            buffer[4] = (byte) (num5 & 0xff);
            destinationIndex = 5;
            int index = 0;
            sourceIndex = 0;
            while (index < num5)
            {
                int num8 = numArray[destinationArray[index], destinationArray[index + 1]];
                num12 = numArray2[index & num2] = num8;
                numArray[destinationArray[index], destinationArray[index + 1]] = index;
                if (index < sourceIndex)
                {
                    index++;
                }
                else
                {
                    num9 = 0;
                    int num7 = 0;
                    while (((num12 >= 0) && ((index - num12) < num)) && (num7++ < num3))
                    {
                        length = 2;
                        while ((destinationArray[index + length] == destinationArray[num12 + length]) && (length < 0x404))
                        {
                            length++;
                        }
                        if (length > num9)
                        {
                            num9 = length;
                            num10 = index - num12;
                        }
                        num12 = numArray2[num12 & num2];
                    }
                    if (num9 > (num5 - index))
                    {
                        num9 = index - num5;
                    }
                    if (num9 <= 2)
                    {
                        num9 = 0;
                    }
                    if ((num9 == 3) && (num10 > 0x400))
                    {
                        num9 = 0;
                    }
                    if ((num9 == 4) && (num10 > 0x4000))
                    {
                        num9 = 0;
                    }
                    if (num9 > 0)
                    {
                        while ((index - sourceIndex) >= 4)
                        {
                            length = ((index - sourceIndex) / 4) - 1;
                            if (length > 0x1b)
                            {
                                length = 0x1b;
                            }
                            buffer[destinationIndex++] = (byte) (0xe0 + length);
                            length = (4 * length) + 4;
                            Array.Copy(destinationArray, sourceIndex, buffer, destinationIndex, length);
                            sourceIndex += length;
                            destinationIndex += length;
                        }
                        length = index - sourceIndex;
                        if ((num9 <= 10) && (num10 <= 0x400))
                        {
                            buffer[destinationIndex++] = (byte) (((((num10 - 1) >> 8) << 5) + ((num9 - 3) << 2)) + length);
                            buffer[destinationIndex++] = (byte) ((num10 - 1) & 0xff);
                            while (length-- > 0)
                            {
                                buffer[destinationIndex++] = destinationArray[sourceIndex++];
                            }
                            sourceIndex += num9;
                        }
                        else if ((num9 <= 0x43) && (num10 <= 0x4000))
                        {
                            buffer[destinationIndex++] = (byte) (0x80 + (num9 - 4));
                            buffer[destinationIndex++] = (byte) ((length << 6) + ((num10 - 1) >> 8));
                            buffer[destinationIndex++] = (byte) ((num10 - 1) & 0xff);
                            while (length-- > 0)
                            {
                                buffer[destinationIndex++] = destinationArray[sourceIndex++];
                            }
                            sourceIndex += num9;
                        }
                        else if ((num9 <= 0x404) && (num10 < num))
                        {
                            num10--;
                            buffer[destinationIndex++] = (byte) (((0xc0 + ((num10 >> 0x10) << 4)) + (((num9 - 5) >> 8) << 2)) + length);
                            buffer[destinationIndex++] = (byte) ((num10 >> 8) & 0xff);
                            buffer[destinationIndex++] = (byte) (num10 & 0xff);
                            buffer[destinationIndex++] = (byte) ((num9 - 5) & 0xff);
                            while (length-- > 0)
                            {
                                buffer[destinationIndex++] = destinationArray[sourceIndex++];
                            }
                            sourceIndex += num9;
                        }
                    }
                    index++;
                }
            }
            index = num5;
            while ((index - sourceIndex) >= 4)
            {
                length = ((index - sourceIndex) / 4) - 1;
                if (length > 0x1b)
                {
                    length = 0x1b;
                }
                buffer[destinationIndex++] = (byte) (0xe0 + length);
                length = (4 * length) + 4;
                Array.Copy(destinationArray, sourceIndex, buffer, destinationIndex, length);
                sourceIndex += length;
                destinationIndex += length;
            }
            length = index - sourceIndex;
            buffer[destinationIndex++] = (byte) (0xfc + length);
            while (length-- > 0)
            {
                buffer[destinationIndex++] = destinationArray[sourceIndex++];
            }
            byte[] buffer3 = new byte[destinationIndex];
            Array.Copy(buffer, 0, buffer3, 0, destinationIndex);
            buffer = buffer3;
            if (incLen)
            {
                buffer3 = new byte[buffer.Length + 4];
                Array.Copy(buffer, 0, buffer3, 4, buffer.Length);
                byte[] bytes = BitConverter.GetBytes(buffer3.Length);
                buffer3[0] = bytes[0];
                buffer3[1] = bytes[1];
                buffer3[2] = bytes[2];
                buffer3[3] = bytes[3];
                buffer = buffer3;
            }
            return buffer;
        }

        private Color[] CreatePalette(byte[] data, int pOfs)
        {
            ushort num4;
            FSHEntryHeader header = new FSHEntryHeader();
            header.code = this.GetInt(data, pOfs);
            header.width = this.GetShort(data, pOfs + 4);
            header.height = this.GetShort(data, pOfs + 6);
            header.misc = new short[4];
            Array.Copy(data, pOfs + 8, header.misc, 0, 4);
            int num = header.code & 0xff;
            int width = header.width;
            int index = pOfs + 0x10;
            Color[] colorArray = new Color[width];
            for (int i = 0; i < width; i++)
            {
                colorArray[i] = Color.FromArgb(0);
            }
            switch (num)
            {
                case 0x24:
                {
                    int num6 = 0;
                    while (num6 < width)
                    {
                        colorArray[num6] = Color.FromArgb(0xff, data[index], data[index + 1], data[index + 2]);
                        num6++;
                        index += 3;
                    }
                    return colorArray;
                }
                case 0x22:
                {
                    int num7 = 0;
                    while (num7 < width)
                    {
                        colorArray[num7] = Color.FromArgb((((0x10000 * data[index]) + (0x100 * data[index + 1])) + data[index + 2]) << 2);
                        num7++;
                        index += 3;
                    }
                    return colorArray;
                }
                case 0x2d:
                {
                    num4 = (ushort) index;
                    int num8 = 0;
                    while (num8 < width)
                    {
                        colorArray[num8] = Color.FromArgb((((data[(int) num4] & 0x1f) + (0x100 * (data[num4 >> 5] & 0x1f))) + (0x10000 * (data[num4 >> 10] & 0x1f))) << 3);
                        if ((data[(int) num4] & 0x8000) > 0)
                        {
                            uint num9 = (uint) (colorArray[num8].ToArgb() + -16777216);
                            colorArray[num8] = Color.FromArgb((int) num9);
                        }
                        num8++;
                        num4 = (ushort) (num4 + 1);
                    }
                    return colorArray;
                }
                case 0x29:
                {
                    num4 = (ushort) index;
                    int num10 = 0;
                    while (num10 < width)
                    {
                        colorArray[num10] = Color.FromArgb((((data[(int) num4] & 0x1f) + (0x100 * (data[num4 >> 5] & 0x3f))) + (0x10000 * (data[num4 >> 10] & 0x1f))) << 3);
                        num10++;
                        num4 = (ushort) (num4 + 1);
                    }
                    return colorArray;
                }
            }
            if (num == 0x2a)
            {
                int num11 = 0;
                while (num11 < width)
                {
                    colorArray[num11] = Color.FromArgb(data[index]);
                    num11++;
                    index += 4;
                }
            }
            return colorArray;
        }

        public byte[] Decomp(byte[] data)
        {
            byte[] buffer;
            int num2 = 0;
            int index = 0;
            int num4 = 0;
            byte num5 = 0;
            int sourceIndex = 0;
            int destinationIndex = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int length = 0;
            int num12 = 0;
            int num13 = 0;
            int num = ((data[0] & 0xfe) * 0x100) + data[1];
            if (num != 0x10fb)
            {
                num = ((data[4] & 0xfe) * 0x100) + data[5];
                if (num != 0x10fb)
                {
                    throw new NotSupportedException("The pack code is incorrect. This is either not a QFS file, or is of an unknown type.");
                }
                num13 = 4;
            }
            num2 = ((data[2 + num13] << 0x10) + (data[3 + num13] << 8)) + data[4 + num13];
            index = 5 + num13;
            num4 = 0;
            if ((data[num13] & 1) > 0)
            {
                index = 8;
            }
            try
            {
                buffer = new byte[num2];
            Label_02BE:
                while ((index < data.Length) && (data[index] < 0xfc))
                {
                    num5 = data[index];
                    num8 = data[index + 1];
                    num9 = data[index + 2];
                    if ((num5 & 0x80) == 0)
                    {
                        length = num5 & 3;
                        destinationIndex = num4;
                        sourceIndex = index + 2;
                        num4 += length;
                        index += length + 2;
                        while (length-- > 0)
                        {
                            buffer[destinationIndex++] = data[sourceIndex++];
                        }
                        length = ((num5 & 0x1c) >> 2) + 3;
                        num12 = (((num5 >> 5) << 8) + num8) + 1;
                        destinationIndex = num4;
                        sourceIndex = destinationIndex - num12;
                        num4 += length;
                        while (length-- > 0)
                        {
                            buffer[destinationIndex++] = buffer[sourceIndex++];
                        }
                    }
                    else
                    {
                        if ((num5 & 0x40) == 0)
                        {
                            length = (num8 >> 6) & 3;
                            destinationIndex = num4;
                            sourceIndex = index + 3;
                            index += length + 3;
                            num4 += length;
                            while (length-- > 0)
                            {
                                buffer[destinationIndex++] = data[sourceIndex++];
                            }
                            length = (num5 & 0x3f) + 4;
                            num12 = (((num8 & 0x3f) * 0x100) + num9) + 1;
                            destinationIndex = num4;
                            sourceIndex = destinationIndex - num12;
                            num4 += length;
                            while (length-- > 0)
                            {
                                buffer[destinationIndex++] = buffer[sourceIndex++];
                            }
                            goto Label_02BE;
                        }
                        if ((num5 & 0x20) == 0)
                        {
                            num10 = data[index + 3];
                            length = num5 & 3;
                            destinationIndex = num4;
                            sourceIndex = index + 4;
                            index += length + 4;
                            num4 += length;
                            while (length-- > 0)
                            {
                                buffer[destinationIndex++] = data[sourceIndex++];
                            }
                            length = ((((num5 >> 2) & 3) * 0x100) + num10) + 5;
                            num12 = ((((num5 & 0x10) << 12) + (0x100 * num8)) + num9) + 1;
                            destinationIndex = num4;
                            sourceIndex = destinationIndex - num12;
                            num4 += length;
                            while (length-- > 0)
                            {
                                buffer[destinationIndex++] = buffer[sourceIndex++];
                            }
                            goto Label_02BE;
                        }
                        length = ((num5 & 0x1f) * 4) + 4;
                        destinationIndex = num4;
                        sourceIndex = index + 1;
                        index += length + 1;
                        num4 += length;
                        while (length-- > 0)
                        {
                            buffer[destinationIndex++] = data[sourceIndex++];
                        }
                    }
                }
                if ((index < data.Length) && (num4 < num2))
                {
                    destinationIndex = num4;
                    sourceIndex = index + 1;
                    length = data[index] & 3;
                    Array.Copy(data, sourceIndex, buffer, destinationIndex, length);
                }
            }
            catch (OutOfMemoryException)
            {
                buffer = null;
            }
            return buffer;
        }

        public FSHEntryHeader GetEntryHeader(int offset)
        {
            FSHEntryHeader header = new FSHEntryHeader();
            header.code = -1;
            if ((this.rawData != null) && (this.rawData.Length > (offset + 0x10)))
            {
                header.code = this.GetInt(this.rawData, offset);
                header.width = this.GetShort(this.rawData, offset + 4);
                header.height = this.GetShort(this.rawData, offset + 6);
                header.misc = new short[4];
                Array.Copy(this.rawData, offset + 8, header.misc, 0, 4);
            }
            return header;
        }

        private int GetInt(byte[] data, int offset)
        {
            return (((data[offset] + (data[offset + 1] << 8)) + (data[offset + 2] << 0x10)) + (data[offset + 3] << 0x18));
        }

        private short GetShort(byte[] data, int offset)
        {
            return (short) (data[offset] + (data[offset + 1] << 8));
        }

        public unsafe void Load(Stream s)
        {
            this.fshHead = new FSHHeader();
            int offset = 0;
            int size = this.fshHead.size;
            bool flag = false;
            bool flag2 = false;
            ArrayList list = new ArrayList(2);
            FSHEntryHeader header3 = new FSHEntryHeader();
            int num3 = 0;
            int num4 = 0;
            int pOfs = 0;
            int num10 = 0;
            byte[] buffer = new byte[2];
            s.Read(buffer, 0, 2);
            if ((buffer[0] == 0x10) && (buffer[1] == 0xfb))
            {
                s.Position = 0L;
                buffer = new byte[(uint) s.Length];
                s.Read(buffer, 0, (int) s.Length);
                this.rawData = this.Decomp(buffer);
                this.isFSHComp = true;
            }
            else
            {
                s.Position = 4L;
                s.Read(buffer, 0, 2);
                if ((buffer[0] == 0x10) && (buffer[1] == 0xfb))
                {
                    s.Position = 0L;
                    buffer = new byte[(uint) s.Length];
                    s.Read(buffer, 0, (int) s.Length);
                    this.rawData = this.Decomp(buffer);
                    this.isFSHComp = true;
                }
            }
            if (this.rawData == null)
            {
                s.Position = 0L;
                this.rawData = new byte[(uint) s.Length];
                s.Read(this.rawData, 0, (int) s.Length);
            }
            if (this.rawData.Length <= 4)
            {
                throw new Exception("FSHImage: The file is truncated and invalid.");
            }
            this.fshHead.SHPI = new byte[4];
            Array.Copy(this.rawData, 0, this.fshHead.SHPI, 0, 4);
            if (Encoding.ASCII.GetString(this.fshHead.SHPI) != "SHPI")
            {
                throw new Exception("FSHImage: An invalid header was read.");
            }
            this.fshHead.size = this.GetInt(this.rawData, 4);
            this.fshHead.numBmps = this.GetInt(this.rawData, 8);
            this.fshHead.dirID = new byte[4];
            Array.Copy(this.rawData, 12, this.fshHead.dirID, 0, 4);
            int num14 = 0x10;
            int numBmps = this.fshHead.numBmps;
            FSHDirEntry[] entryArray = new FSHDirEntry[numBmps];
            for (int i = 0; i < numBmps; i++)
            {
                FSHDirEntry entry = entryArray[i];
                entry.name = new byte[4];
                Array.Copy(this.rawData, num14 + (8 * i), entry.name, 0, 4);
                entry.offset = this.GetInt(this.rawData, (num14 + (8 * i)) + 4);
                entryArray[i] = entry;
            }
            this.directory = entryArray;
            for (int j = 0; j < numBmps; j++)
            {
                int num6;
                int num7;
                int num8;
                int num9;
                int width;
                FSHDirEntry entry2 = entryArray[j];
                offset = entry2.offset;
                size = this.fshHead.size;
                for (int k = 0; k < numBmps; k++)
                {
                    if ((entryArray[k].offset < size) && (entryArray[k].offset > offset))
                    {
                        size = entryArray[k].offset;
                    }
                }
                if (((j == (numBmps - 1)) && (size != this.fshHead.size)) || ((j < (numBmps - 1)) && (size != entryArray[j + 1].offset)))
                {
                    Debug.WriteLine("WARNING: FSH bitmaps are not correctly ordered.\nThe reverse conversion from .BMP to .FSH/.QFS may give a corrupted file.\n");
                }
                FSHEntryHeader header = new FSHEntryHeader();
                header.code = this.GetInt(this.rawData, offset);
                header.width = this.GetShort(this.rawData, offset + 4);
                header.height = this.GetShort(this.rawData, offset + 6);
                header.misc = new short[4];
                Array.Copy(this.rawData, offset + 8, header.misc, 0, 4);
                int num19 = header.code & 0x7f;
                flag = ((((num19 == 120) || (num19 == 0x7b)) || ((num19 == 0x7d) || (num19 == 0x7e))) || (((num19 == 0x7f) || (num19 == 0x6d)) || (num19 == 0x61))) || (num19 == 0x60);
                flag2 = (header.code & 0x80) > 0;
                if (!flag)
                {
                    continue;
                }
                FSHEntryHeader header2 = header;
                num3 = offset;
                num4 = 0;
                pOfs = 0;
                while ((header2.code >> 8) > 0)
                {
                    num4++;
                    num3 += header2.code >> 8;
                    if (num3 > size)
                    {
                        Debug.WriteLine("FSHImage: Incorrect attachemnt structure, image may not load properly.");
                    }
                    if (num3 == size)
                    {
                        break;
                    }
                    header2 = new FSHEntryHeader();
                    header2.code = this.GetInt(this.rawData, num3);
                    header2.width = this.GetShort(this.rawData, num3 + 4);
                    header2.height = this.GetShort(this.rawData, num3 + 6);
                    header2.misc = new short[4];
                    Array.Copy(this.rawData, num3 + 8, header2.misc, 0, 4);
                    int num20 = header2.code & 0xff;
                    if (((header.code & 0x7f) == 0x7b) && (((num20 == 0x22) || (num20 == 0x24)) || (((num20 == 0x2d) || (num20 == 0x2a)) || (num20 == 0x29))))
                    {
                        header3 = header2;
                        pOfs = num3;
                    }
                }
                int num21 = 0;
                if (flag2)
                {
                    goto Label_069C;
                }
                if ((header.misc[3] & 0xffff) == 0)
                {
                    num21 = (header.misc[3] >> 12) & 15;
                }
                if (((header.width % (((int) 1) << num21)) > 0) || ((header.height % (((int) 1) << num21)) > 0))
                {
                    num21 = 0;
                }
                if (num21 <= 0)
                {
                    goto Label_069C;
                }
                if ((num19 == 0x7b) || (num19 == 0x61))
                {
                    num6 = 2;
                }
                else
                {
                    switch (num19)
                    {
                        case 0x7d:
                            num6 = 8;
                            goto Label_057A;

                        case 0x7f:
                            num6 = 6;
                            goto Label_057A;

                        case 0x60:
                            num6 = 1;
                            goto Label_057A;
                    }
                    num6 = 4;
                }
            Label_057A:
                num8 = num9 = 0;
                int num13 = 0;
                while (num13 <= num21)
                {
                    width = header.width >> num13;
                    if ((header.code & 0x7e) == 0x60)
                    {
                        width += (4 - width) & 3;
                    }
                    num7 = header.height >> num13;
                    if ((header.code & 0x7e) == 0x60)
                    {
                        num7 += (4 - num7) & 3;
                    }
                    num8 += ((width * num7) * num6) / 2;
                    num9 += ((width * num7) * num6) / 2;
                    if ((header.code & 0x7e) != 0x60)
                    {
                        num8 += (0x10 - num8) & 15;
                        if (num13 == num21)
                        {
                            num9 += (0x10 - num9) & 15;
                        }
                    }
                    num13++;
                }
                num10 = 0;
                if ((((header.code >> 8) != (num8 + 0x10)) && ((header.code >> 8) != 0)) || (((header.code >> 8) == 0) && (((num8 + offset) + 0x10) != size)))
                {
                    num10 = 1;
                    if ((((header.code >> 8) != (num9 + 0x10)) && ((header.code >> 8) != 0)) || (((header.code >> 8) == 0) && (((num9 + offset) + 0x10) != size)))
                    {
                        num21 = 0;
                    }
                }
            Label_069C:
                if (flag)
                {
                    num13 = num21;
                    int num12 = offset + 0x10;
                    int num22 = size - num12;
                    if (!flag2)
                    {
                    }
                    while (num13 >= 0)
                    {
                        byte* numPtr;
                        Color[] colorArray = new Color[0];
                        int[,] numArray = new int[(uint) header.height, (uint) header.width];
                        int[,] numArray2 = new int[(uint) header.height, (uint) header.width];
                        PixelFormat undefined = PixelFormat.Format32bppArgb;
                        FSHBmpType thirtyTwoBit = FSHBmpType.ThirtyTwoBit;
                        switch (num19)
                        {
                            case 0x7b:
                            {
                                undefined = PixelFormat.Format8bppIndexed;
                                thirtyTwoBit = FSHBmpType.EightBit;
                                if (pOfs > 0)
                                {
                                    colorArray = this.CreatePalette(this.rawData, pOfs);
                                }
                                width = header.width;
                                while ((width & 3) > 0)
                                {
                                    width++;
                                }
                                int num23 = 0;
                                for (int num24 = header.height - 1; (num23 < header.height) && (num24 >= 0); num24--)
                                {
                                    num22 = 0;
                                    for (int num25 = 0; num25 < header.width; num25++)
                                    {
                                        numArray[num23, num25] = this.rawData[(num12 + (num23 * header.width)) + num22];
                                        num22++;
                                    }
                                    num23++;
                                }
                                num12 += header.width * header.height;
                                break;
                            }
                            case 0x7d:
                            {
                                int num26;
                                undefined = PixelFormat.Format32bppArgb;
                                thirtyTwoBit = FSHBmpType.ThirtyTwoBit;
                                int num27 = 0;
                                for (int num28 = header.height - 1; (num27 < header.height) && (num28 >= 0); num28--)
                                {
                                    num26 = num12 + ((4 * num27) * header.width);
                                    num22 = 0;
                                    while (num22 < header.width)
                                    {
                                        numArray2[num27, num22] = ((this.rawData[(num26 + (4 * num22)) + 3] + (this.rawData[(num26 + (4 * num22)) + 3] << 8)) + (this.rawData[(num26 + (4 * num22)) + 3] << 0x10)) + -16777216;
                                        num22++;
                                    }
                                    num27++;
                                }
                                width = 4 * header.width;
                                while ((width & 4) > 0)
                                {
                                    width++;
                                }
                                num26 = 0;
                                int num29 = 0;
                                for (int num30 = header.height - 1; (num29 < header.height) && (num30 >= 0); num30--)
                                {
                                    num26 = num12 + ((4 * num29) * header.width);
                                    for (int num31 = 0; num31 < header.width; num31++)
                                    {
                                        numArray[num29, num31] = ((this.rawData[num26 + (4 * num31)] + (this.rawData[(num26 + (4 * num31)) + 1] << 8)) + (this.rawData[(num26 + (4 * num31)) + 2] << 0x10)) + (this.rawData[(num26 + (4 * num31)) + 3] << 0x18);
                                    }
                                    num29++;
                                }
                                num12 += (4 * header.width) * header.height;
                                break;
                            }
                            case 0x7f:
                            {
                                undefined = PixelFormat.Format32bppArgb;
                                thirtyTwoBit = FSHBmpType.TwentyFourBit;
                                width = 3 * header.width;
                                while ((width & 3) > 0)
                                {
                                    width++;
                                }
                                int num33 = 0;
                                for (int num34 = header.height - 1; (num33 < header.height) && (num34 >= 0); num34--)
                                {
                                    int num32 = num12 + ((3 * num33) * header.width);
                                    for (int num35 = 0; num35 < header.width; num35++)
                                    {
                                        unchecked
                                        {
                                            numArray[num33, num35] = ((this.rawData[num32 + (3 * num35)] + (this.rawData[(num32 + (3 * num35)) + 1] << 8)) + (this.rawData[(num32 + (3 * num35)) + 2] << 0x10)) + ((int)0xff000000L);
                                        }
                                    }
                                    num33++;
                                }
                                num12 += (3 * header.width) * header.height;
                                break;
                            }
                            case 0x7e:
                            {
                                undefined = PixelFormat.Format32bppArgb;
                                thirtyTwoBit = FSHBmpType.SixteenBitAlpha;
                                width = 2 * header.width;
                                while ((width & 2) > 0)
                                {
                                    width++;
                                }
                                int num37 = 0;
                                for (int num38 = header.height - 1; (num37 < header.height) && (num38 >= 0); num38--)
                                {
                                    int num36 = num12 + ((2 * num38) * header.width);
                                    num22 = 0;
                                    for (int num39 = 0; num39 < header.width; num39++)
                                    {
                                        short num40 = (short) ((this.rawData[num36 + (2 * num22)] << 8) + this.rawData[(num36 + (2 * num22)) + 1]);
                                        numArray[num37, num39] = num40;
                                        num22 += 2;
                                    }
                                    num37++;
                                }
                                num12 += (2 * header.width) * header.height;
                                break;
                            }
                            case 120:
                            {
                                undefined = PixelFormat.Format32bppArgb;
                                thirtyTwoBit = FSHBmpType.SixteenBit;
                                width = 2 * header.width;
                                while ((width & 2) > 0)
                                {
                                    width++;
                                }
                                int num42 = 0;
                                for (int num43 = header.height - 1; (num42 < header.height) && (num43 >= 0); num43--)
                                {
                                    int num41 = num12 + ((2 * num43) * header.width);
                                    num22 = 0;
                                    for (int num44 = 0; num44 < header.width; num44++)
                                    {
                                        try
                                        {
                                            short num45 = (short) ((this.rawData[num41 + (2 * num22)] << 8) + this.rawData[(num41 + (2 * num22)) + 1]);
                                            numArray[num42, num44] = num45;
                                            num22 += 2;
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    num42++;
                                }
                                num12 += (2 * header.width) * header.height;
                                break;
                            }
                            case 0x6d:
                                undefined = PixelFormat.Undefined;
                                thirtyTwoBit = FSHBmpType.SixteenBit4x4;
                                break;

                            case 0x61:
                            {
                                int num46;
                                undefined = PixelFormat.Format32bppArgb;
                                thirtyTwoBit = FSHBmpType.DXT3;
                                int num47 = header.height - 1;
                                int num48 = 0;
                                byte[] target = new byte[(uint) header.width];
                                for (int num49 = (header.height / 4) - 1; num49 >= 0; num49--)
                                {
                                    num46 = num12 + ((4 * num49) * header.width);
                                    num7 = 6;
                                    while (num7 >= 0)
                                    {
                                        num22 = 0;
                                        while (num22 < (header.width / 4))
                                        {
                                            byte[] buffer4;
                                            IntPtr ptr;
                                            byte[] buffer5;
                                            IntPtr ptr2;
                                            byte[] buffer6;
                                            IntPtr ptr3;
                                            byte[] buffer7;
                                            IntPtr ptr4;
                                            target[4 * num22] = (byte) (this.rawData[(num46 + (0x10 * num22)) + num7] & 15);
                                            (buffer4 = target)[(int) (ptr = (IntPtr) (4 * num22))] = (byte) (buffer4[(int) ptr] + ((byte) (target[4 * num22] << 4)));
                                            target[(4 * num22) + 1] = (byte) (this.rawData[(num46 + (0x10 * num22)) + num7] >> 4);
                                            (buffer5 = target)[(int) (ptr2 = (IntPtr) ((4 * num22) + 1))] = (byte) (buffer5[(int) ptr2] + ((byte) (target[(4 * num22) + 1] << 4)));
                                            target[(4 * num22) + 2] = (byte) (this.rawData[((num46 + (0x10 * num22)) + num7) + 1] & 15);
                                            (buffer6 = target)[(int) (ptr3 = (IntPtr) ((4 * num22) + 2))] = (byte) (buffer6[(int) ptr3] + ((byte) (target[(4 * num22) + 2] << 4)));
                                            target[(4 * num22) + 3] = (byte) (this.rawData[((num46 + (0x10 * num22)) + num7) + 1] >> 4);
                                            (buffer7 = target)[(int) (ptr4 = (IntPtr) ((4 * num22) + 3))] = (byte) (buffer7[(int) ptr4] + ((byte) (target[(4 * num22) + 3] << 4)));
                                            if (num48 >= header.width)
                                            {
                                                num48 = 0;
                                                num47--;
                                            }
                                            numArray2[num47, num48] = (((target[4 * num22] << 0x18) + (target[4 * num22] << 0x10)) + (target[4 * num22] << 8)) + target[4 * num22];
                                            num48++;
                                            numArray2[num47, num48] = (((target[(4 * num22) + 1] << 0x18) + (target[(4 * num22) + 1] << 0x10)) + (target[(4 * num22) + 1] << 8)) + target[(4 * num22) + 1];
                                            num48++;
                                            numArray2[num47, num48] = (((target[(4 * num22) + 2] << 0x18) + (target[(4 * num22) + 2] << 0x10)) + (target[(4 * num22) + 2] << 8)) + target[(4 * num22) + 2];
                                            num48++;
                                            numArray2[num47, num48] = (((target[(4 * num22) + 3] << 0x18) + (target[(4 * num22) + 3] << 0x10)) + (target[(4 * num22) + 3] << 8)) + target[(4 * num22) + 3];
                                            num48++;
                                            num22++;
                                        }
                                        num7 -= 2;
                                    }
                                }
                                for (width = 3 * header.width; (width & 3) > 0; width++)
                                {
                                }
                                num47 = header.height - 1;
                                num48 = 0;
                                target = new byte[12 * (header.width / 4)];
                                for (int num51 = (header.height / 4) - 1; num51 >= 0; num51--)
                                {
                                    num46 = num12 + ((4 * num51) * header.width);
                                    num7 = 15;
                                    while (num7 >= 12)
                                    {
                                        num22 = 0;
                                        while (num22 < (header.width / 4))
                                        {
                                            int @int = this.GetInt(this.rawData, (num46 + (0x10 * num22)) + 8);
                                            this.UnpackDXT((byte) (this.rawData[(num46 + (0x10 * num22)) + num7] & 3), (ushort) @int, (ushort) (@int >> 0x10), target, 12 * num22);
                                            this.UnpackDXT((byte) ((this.rawData[(num46 + (0x10 * num22)) + num7] >> 2) & 3), (ushort) @int, (ushort) (@int >> 0x10), target, (12 * num22) + 3);
                                            this.UnpackDXT((byte) ((this.rawData[(num46 + (0x10 * num22)) + num7] >> 4) & 3), (ushort) @int, (ushort) (@int >> 0x10), target, (12 * num22) + 6);
                                            this.UnpackDXT((byte) ((this.rawData[(num46 + (0x10 * num22)) + num7] >> 6) & 3), (ushort) @int, (ushort) (@int >> 0x10), target, (12 * num22) + 9);
                                            if (num48 >= header.width)
                                            {
                                                num48 = 0;
                                                num47--;
                                            }
                                            int num52 = 0;
                                            int num53 = -16777216;
                                            numArray[num47, num48] = ((target[(12 * num22) + num52] + (target[((12 * num22) + 1) + num52] << 8)) + (target[((12 * num22) + 2) + num52] << 0x10)) + num53;
                                            num48++;
                                            num52 += 3;
                                            numArray[num47, num48] = ((target[(12 * num22) + num52] + (target[((12 * num22) + 1) + num52] << 8)) + (target[((12 * num22) + 2) + num52] << 0x10)) + num53;
                                            num48++;
                                            num52 += 3;
                                            numArray[num47, num48] = ((target[(12 * num22) + num52] + (target[((12 * num22) + 1) + num52] << 8)) + (target[((12 * num22) + 2) + num52] << 0x10)) + num53;
                                            num48++;
                                            num52 += 3;
                                            numArray[num47, num48] = ((target[(12 * num22) + num52] + (target[((12 * num22) + 1) + num52] << 8)) + (target[((12 * num22) + 2) + num52] << 0x10)) + num53;
                                            num48++;
                                            num22++;
                                        }
                                        num7--;
                                    }
                                }
                                num12 += header.width * header.height;
                                break;
                            }
                            case 0x60:
                            {
                                undefined = PixelFormat.Format32bppArgb;
                                thirtyTwoBit = FSHBmpType.DXT1;
                                for (int num54 = header.height - 1; num54 >= 0; num54--)
                                {
                                    num22 = 0;
                                    while (num22 < header.width)
                                    {
                                        numArray2[num54, num22] = -1;
                                        num22++;
                                    }
                                }
                                int num56 = header.height - 1;
                                int num58 = 0;
                                byte[] buffer3 = new byte[12 * (header.width / 4)];
                                for (int num59 = (header.height / 4) - 1; num59 >= 0; num59--)
                                {
                                    int num55 = num12 + ((2 * num59) * header.width);
                                    for (num7 = 7; num7 >= 4; num7--)
                                    {
                                        num22 = 0;
                                        while (num22 < (header.width / 4))
                                        {
                                            int num57 = this.GetInt(this.rawData, num55 + (8 * num22));
                                            this.UnpackDXT((byte) (this.rawData[(num55 + (8 * num22)) + num7] & 3), (ushort) num57, (ushort) (num57 >> 0x10), buffer3, 12 * num22);
                                            this.UnpackDXT((byte) ((this.rawData[(num55 + (8 * num22)) + num7] >> 2) & 3), (ushort) num57, (ushort) (num57 >> 0x10), buffer3, (12 * num22) + 3);
                                            this.UnpackDXT((byte) ((this.rawData[(num55 + (8 * num22)) + num7] >> 4) & 3), (ushort) num57, (ushort) (num57 >> 0x10), buffer3, (12 * num22) + 6);
                                            this.UnpackDXT((byte) ((this.rawData[(num55 + (8 * num22)) + num7] >> 6) & 3), (ushort) num57, (ushort) (num57 >> 0x10), buffer3, (12 * num22) + 9);
                                            if (num58 >= header.width)
                                            {
                                                num58 = 0;
                                                num56--;
                                            }
                                            int num60 = 0;
                                            unchecked{
                                                numArray[num56, num58] = ((buffer3[(12 * num22) + num60] + (buffer3[((12 * num22) + 1) + num60] << 8)) + (buffer3[((12 * num22) + 2) + num60] << 0x10)) + ((int)0xff000000L);
                                                num58++;
                                                num60 += 3;
                                                numArray[num56, num58] = ((buffer3[(12 * num22) + num60] + (buffer3[((12 * num22) + 1) + num60] << 8)) + (buffer3[((12 * num22) + 2) + num60] << 0x10)) + ((int)0xff000000L);
                                                num58++;
                                                num60 += 3;
                                                numArray[num56, num58] = ((buffer3[(12 * num22) + num60] + (buffer3[((12 * num22) + 1) + num60] << 8)) + (buffer3[((12 * num22) + 2) + num60] << 0x10)) + ((int)0xff000000L);
                                                num58++;
                                                num60 += 3;
                                                numArray[num56, num58] = ((buffer3[(12 * num22) + num60] + (buffer3[((12 * num22) + 1) + num60] << 8)) + (buffer3[((12 * num22) + 2) + num60] << 0x10)) + ((int)0xff000000L);
                                                num58++;
                                            }
                                            num22++;
                                        }
                                    }
                                }
                                num12 += (header.width * header.height) / 2;
                                break;
                            }
                        }
                        Bitmap bitmap = new Bitmap(header.width, header.height, undefined);

                        // This is the Vista compatibility fix.
                        if (colorArray.Length > 0)
                        {
                            ColorPalette palette = bitmap.Palette;
                            for (int m = 0; m < colorArray.Length; m++)
                            {
                                palette.Entries[m] = Color.FromArgb(0xff, colorArray[m]);
                            }
                            bitmap.Palette = palette;
                        }

                        GraphicsUnit pixel = GraphicsUnit.Pixel;
                        RectangleF bounds = bitmap.GetBounds(ref pixel);
                        Rectangle rect = new Rectangle((int) bounds.X, (int) bounds.Y, (int) bounds.Width, (int) bounds.Height);
                        BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, undefined);
                        switch (undefined)
                        {
                            case PixelFormat.Format8bppIndexed:
                            {
                                int num62 = ((int) bounds.Width) * 1;
                                if ((num62 % 4) != 0)
                                {
                                    num62 = 4 * ((num62 / 4) + 1);
                                }
                                for (int num63 = 0; num63 < header.height; num63++)
                                {
                                    numPtr = this.PixelAt(0, num63, num62, (byte*) bitmapdata.Scan0, 1);
                                    for (int num64 = 0; num64 < header.width; num64++)
                                    {
                                        numPtr[0] = (byte) numArray[num63, num64];
                                        numPtr++;
                                    }
                                }
                                break;
                            }
                            case PixelFormat.Format32bppArgb:
                            {
                                int num65 = ((int) bounds.Width) * 4;
                                if ((num65 % 4) != 0)
                                {
                                    num65 = 4 * ((num65 / 4) + 1);
                                }
                                for (int num66 = 0; num66 < header.height; num66++)
                                {
                                    numPtr = this.PixelAt(0, num66, num65, (byte*) bitmapdata.Scan0, 4);
                                    for (int num67 = 0; num67 < header.width; num67++)
                                    {
                                        numPtr[0] = (byte) (numArray[num66, num67] & 0xff);
                                        numPtr++;
                                        numPtr[0] = (byte) ((numArray[num66, num67] >> 8) & 0xff);
                                        numPtr++;
                                        numPtr[0] = (byte) ((numArray[num66, num67] >> 0x10) & 0xff);
                                        numPtr++;
                                        numPtr[0] = (byte) ((numArray[num66, num67] >> 0x18) & 0xff);
                                        numPtr++;
                                    }
                                }
                                break;
                            }
                        }
                        bitmap.UnlockBits(bitmapdata);
                        Bitmap bitmap2 = new Bitmap(header.width, header.height, PixelFormat.Format32bppRgb);
                        for (int n = 0; n < header.width; n++)
                        {
                            for (int num69 = 0; num69 < header.height; num69++)
                            {
                                bitmap2.SetPixel(n, num69, Color.FromArgb(numArray2[num69, n]));
                            }
                        }
                        BitmapItem item = new BitmapItem();
                        item.BmpType = thirtyTwoBit;
                        item.SetDirName(Encoding.ASCII.GetString(entry2.name));
                        item.IsCompressed = flag2;
                        item.Bitmap = bitmap;
                        item.Alpha = bitmap2;
                        item.Palette = colorArray;
                        this.bitmapItems.Add(item);
                        if (num21 > 0)
                        {
                            header.width = (short) (header.width / 2);
                            header.height = (short) (header.height / 2);
                            if ((header.code & 0x7e) == 0x60)
                            {
                                header.width = (short) (header.width + ((short) ((4 - header.width) & 3)));
                                header.height = (short) (header.height + ((short) ((4 - header.height) & 3)));
                            }
                            else
                            {
                                num22 = (num12 - offset) & 15;
                                if ((num22 > 0) && (num10 != 0))
                                {
                                    num12 += 0x10 - num22;
                                }
                            }
                        }
                        num13--;
                    }
                    if (flag2)
                    {
                        if ((header.code >> 8) > 0)
                        {
                            num12 = offset + (header.code >> 8);
                        }
                        else
                        {
                            num12 = size;
                        }
                    }
                    header2 = header;
                    num3 = offset;
                    if (num4 > 0)
                    {
                    }
                    try
                    {
                        num6 = num4;
                        while (num6 > 0)
                        {
                            num6--;
                            num3 += header2.code >> 8;
                            if (num12 < num3)
                            {
                            }
                            header2 = new FSHEntryHeader();
                            header2.code = this.GetInt(this.rawData, num3);
                            header2.width = this.GetShort(this.rawData, num3 + 4);
                            header2.height = this.GetShort(this.rawData, num3 + 6);
                            header2.misc = new short[4];
                            for (int num70 = 0; num70 < 4; num70++)
                            {
                                header2.misc[num70] = this.GetShort(this.rawData, (num3 + 8) + (2 * j));
                            }
                            num22 = header2.code & 0xff;
                            if (((header.code & 0x7f) == 0x7b) && header2.Equals(header3))
                            {
                                switch (num22)
                                {
                                    case 0x2d:
                                    case 0x29:
                                        num22 = 2;
                                        break;

                                    default:
                                        if (num22 == 0x2a)
                                        {
                                            num22 = 4;
                                        }
                                        else
                                        {
                                            num22 = 3;
                                        }
                                        break;
                                }
                                num12 = (num3 + 0x10) + (header2.width * num22);
                                continue;
                            }
                            if ((header2.code & 0xff) == 0x6f)
                            {
                                num12 = (num3 + 8) + header2.width;
                            }
                            else
                            {
                                if ((header2.code & 0xff) == 0x69)
                                {
                                    num12 = (num3 + 0x10) + header2.width;
                                    continue;
                                }
                                if ((header2.code & 0xff) == 0x70)
                                {
                                    num12 = num3 + 0x10;
                                    continue;
                                }
                                num22 = header2.code >> 8;
                                if (num22 == 0)
                                {
                                    num22 = size - num3;
                                }
                                if (num22 > 0x4000)
                                {
                                    throw new Exception("Data too large in FSH data attachment.");
                                }
                                num12 = num3 + num22;
                            }
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("FSHImage: Error parsing attachments.");
                    }
                    if (num4 > 0)
                    {
                    }
                    if (num12 < size)
                    {
                    }
                    if (num12 > size)
                    {
                    }
                }
                else
                {
                    switch ((header.code & 0xff))
                    {
                        case 0x24:
                            Debug.WriteLine("FSHImage: Palette: 24-bit");
                            break;

                        case 0x22:
                            Debug.WriteLine("FSHImage: Palette: 24-bit DOS");
                            break;

                        case 0x2d:
                            Debug.WriteLine("FSHImage: Palette: 16-bit");
                            break;

                        case 0x29:
                            Debug.WriteLine("FSHImage: Palette: 16-bit NFS5");
                            break;

                        case 0x2a:
                        {
                            Debug.WriteLine("FSHImage: Palette: 32-bit");
                            continue;
                        }
                    }
                }
            }
        }

        private unsafe void PackDXT(ulong* px, byte* dest)
        {
            int num;
            int num2;
            ulong num7;
            ulong num8;
            int nstep = 0;
            int num5 = 0;
            ulong[] numArray = new ulong[0x10];
            ulong num9 = 0L;
            ulong num10 = 0L;
            int num6 = 0;
            for (num = 0; num < 0x10; num++)
            {
                num7 = px[num] & ((ulong) 0xf8fcf8L);
                num2 = 0;
                while (num2 < num6)
                {
                    if (numArray[num2] == num7)
                    {
                        break;
                    }
                    num2++;
                }
                if (num2 == num6)
                {
                    numArray[num6++] = num7;
                }
            }
            if (num6 == 1)
            {
                num9 = numArray[0];
                num10 = numArray[0];
                num5 = 0x3e8;
                nstep = 3;
            }
            else
            {
                num5 = 0x40000000;
                for (num = 0; num < (num6 - 1); num++)
                {
                    for (num2 = num + 1; num2 < num6; num2++)
                    {
                        ulong num11;
                        int num4 = this.ScoreDXT(px, 2, numArray[num], numArray[num2], &num11);
                        if (num4 < num5)
                        {
                            num9 = numArray[num];
                            num10 = numArray[num2];
                            nstep = 2;
                            num5 = num4;
                        }
                        num4 = this.ScoreDXT(px, 3, numArray[num], numArray[num2], &num11);
                        if (num4 < num5)
                        {
                            num9 = numArray[num];
                            num10 = numArray[num2];
                            nstep = 3;
                            num5 = num4;
                        }
                    }
                }
            }
            byte* numPtr = (byte*) &num7;
            byte* numPtr2 = (byte*) &num8;
            num7 = num9;
            num8 = num10;
            ushort* numPtr3 = (ushort*) dest;
            numPtr3[0] = (ushort) (((numPtr[0] >> 3) + ((numPtr[1] >> 2) << 5)) + ((numPtr[2] >> 3) << 11));
            numPtr3[1] = (ushort) (((numPtr2[0] >> 3) + ((numPtr2[1] >> 2) << 5)) + ((numPtr2[2] >> 3) << 11));
            if ((numPtr3[0] > numPtr3[1]) ^ (nstep == 3))
            {
                ushort num12 = numPtr3[0];
                numPtr3[0] = numPtr3[1];
                numPtr3[1] = num12;
                num9 = num8;
                num10 = num7;
            }
            this.ScoreDXT(px, nstep, num9, num10, (ulong*) (dest + 4));
        }

        private unsafe byte* PixelAt(int x, int y, int width, byte* pBase, int strucSize)
        {
            return ((pBase + (y * width)) + (x * strucSize));
        }

        public unsafe void Save(Stream stream)
        {
            try
            {
                int num;
                MemoryStream stream2 = new MemoryStream(0x400);
                FSHHeader header = new FSHHeader();
                header.SHPI = Encoding.ASCII.GetBytes("SHPI");
                header.size = 0;
                header.numBmps = num = this.bitmapItems.Count + (this.saveGlobPal ? 1 : 0);
                header.dirID = Encoding.ASCII.GetBytes("G264");
                stream2.Write(header.SHPI, 0, 4);
                stream2.Write(BitConverter.GetBytes(header.size), 0, 4);
                stream2.Write(BitConverter.GetBytes(header.numBmps), 0, 4);
                stream2.Write(header.dirID, 0, 4);
                FSHDirEntry[] entryArray = new FSHDirEntry[num];
                int index = 0;
                if (this.saveGlobPal)
                {
                    FSHDirEntry entry = entryArray[index];
                    entry.name = Encoding.ASCII.GetBytes("!PAL");
                    entry.offset = 0x10 + (8 * num);
                    index++;
                    stream2.Write(entry.name, 0, 4);
                    stream2.Write(BitConverter.GetBytes(entry.offset), 0, 4);
                }
                index = index;
                int num3 = 0;
                while ((index < num) && (num3 < this.bitmapItems.Count))
                {
                    BitmapItem item = (BitmapItem) this.bitmapItems[num3];
                    entryArray[index].name = item.DirName;
                    entryArray[index].offset = (0x10 + (8 * num)) + index;
                    stream2.Write(entryArray[index].name, 0, 4);
                    stream2.Write(BitConverter.GetBytes(entryArray[index].offset), 0, 4);
                    index++;
                    num3++;
                }
                int num4 = 0;
                if (this.saveGlobPal)
                {
                    num4++;
                }
                num4 = num4;
                for (int i = 0; (num4 < num) && (i < this.bitmapItems.Count); i++)
                {
                    BitmapItem item2 = (BitmapItem) this.bitmapItems[i];
                    entryArray[num4].offset = (int) stream2.Position;
                    entryArray[num4].name = item2.DirName;
                    FSHEntryHeader header2 = new FSHEntryHeader();
                    header2.code = (int) item2.BmpType;
                    header2.height = (short) item2.Bitmap.Height;
                    header2.width = (short) item2.Bitmap.Width;
                    header2.misc = new short[4];
                    stream2.Write(BitConverter.GetBytes(header2.code), 0, 4);
                    stream2.Write(BitConverter.GetBytes(header2.width), 0, 2);
                    stream2.Write(BitConverter.GetBytes(header2.height), 0, 2);
                    for (int j = 0; j < 4; j++)
                    {
                        stream2.Write(BitConverter.GetBytes(header2.misc[j]), 0, 2);
                    }
                    Bitmap bitmap = item2.Bitmap;
                    Bitmap alpha = item2.Alpha;
                    GraphicsUnit pixel = GraphicsUnit.Pixel;
                    RectangleF bounds = bitmap.GetBounds(ref pixel);
                    Rectangle rect = new Rectangle((int) bounds.X, (int) bounds.Y, (int) bounds.Width, (int) bounds.Height);
                    RectangleF ef2 = alpha.GetBounds(ref pixel);
                    Rectangle rectangle2 = new Rectangle((int) ef2.X, (int) ef2.Y, (int) ef2.Width, (int) ef2.Height);
                    if (item2.BmpType == FSHBmpType.EightBit)
                    {
                        BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                        int width = ((int) bounds.Width) * 1;
                        if ((width % 4) != 0)
                        {
                            width = 4 * ((width / 4) + 1);
                        }
                        for (int k = header2.height - 1; k >= 0; k--)
                        {
                            byte* numPtr = this.PixelAt(0, k, width, (byte*) bitmapdata.Scan0, 1);
                            for (int m = 0; m < header2.width; m++)
                            {
                                stream2.WriteByte(numPtr[0]);
                                numPtr++;
                            }
                        }
                        bitmap.UnlockBits(bitmapdata);
                    }
                    else if (item2.BmpType == FSHBmpType.SixteenBit)
                    {
                        BitmapData data2 = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        int num10 = ((int) bounds.Width) * 4;
                        if ((num10 % 4) != 0)
                        {
                            num10 = 4 * ((num10 / 4) + 1);
                        }
                        for (int n = 0; n < header2.height; n++)
                        {
                            byte* numPtr2 = this.PixelAt(0, n, num10, (byte*) data2.Scan0, 4);
                            for (int num12 = 0; num12 < header2.width; num12++)
                            {
                                ushort num13 = (ushort) ((((numPtr2[1] >> 3) + ((numPtr2[2] >> 2) << 5)) + ((numPtr2[3] >> 3) << 11)) & 0xffff);
                                byte[] bytes = BitConverter.GetBytes(num13);
                                stream2.Write(bytes, 0, 2);
                                numPtr2 += 4;
                            }
                        }
                        bitmap.UnlockBits(data2);
                    }
                    else if (item2.BmpType != FSHBmpType.SixteenBitAlpha)
                    {
                        if (item2.BmpType == FSHBmpType.SixteenBit4x4)
                        {
                            BitmapData data3 = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            BitmapData data4 = alpha.LockBits(rectangle2, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            int num14 = ((int) bounds.Width) * 4;
                            if ((num14 % 4) != 0)
                            {
                                num14 = 4 * ((num14 / 4) + 1);
                            }
                            for (int num15 = 0; num15 < header2.height; num15++)
                            {
                                byte* numPtr3 = this.PixelAt(0, num15, num14, (byte*) data3.Scan0, 4);
                                byte* numPtr4 = this.PixelAt(0, num15, num14, (byte*) data4.Scan0, 4);
                                for (int num16 = 0; num16 < header2.width; num16++)
                                {
                                    ushort num17 = (ushort) (((((numPtr4[1] >> 4) << ((12 + (numPtr3[1] >> 4)) & 0x1f)) << ((8 + (numPtr3[2] >> 4)) & 0x1f)) << (4 + (numPtr3[3] >> 4))) & 0xffff);
                                    byte[] buffer3 = BitConverter.GetBytes(num17);
                                    stream2.Write(buffer3, 0, 2);
                                    numPtr3 += 4;
                                    numPtr4 += 4;
                                }
                            }
                            bitmap.UnlockBits(data3);
                            alpha.UnlockBits(data4);
                        }
                        else if (item2.BmpType == FSHBmpType.TwentyFourBit)
                        {
                            BitmapData data5 = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                            int num18 = ((int) bounds.Width) * 3;
                            if ((num18 % 4) != 0)
                            {
                                num18 = 4 * ((num18 / 4) + 1);
                            }
                            for (int num19 = 0; num19 < header2.height; num19++)
                            {
                                byte* numPtr5 = this.PixelAt(0, num19, num18, (byte*) data5.Scan0, 3);
                                for (int num20 = 0; num20 < header2.width; num20++)
                                {
                                    byte[] buffer4 = new byte[] { numPtr5[0], numPtr5[1], numPtr5[2] };
                                    stream2.Write(buffer4, 0, 3);
                                    numPtr5 += 3;
                                }
                            }
                            bitmap.UnlockBits(data5);
                        }
                        else if (item2.BmpType == FSHBmpType.ThirtyTwoBit)
                        {
                            BitmapData data6 = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            BitmapData data7 = alpha.LockBits(rectangle2, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            int num21 = ((int) bounds.Width) * 4;
                            if ((num21 % 4) != 0)
                            {
                                num21 = 4 * ((num21 / 4) + 1);
                            }
                            for (int num22 = 0; num22 < header2.height; num22++)
                            {
                                byte* numPtr6 = this.PixelAt(0, num22, num21, (byte*) data6.Scan0, 4);
                                byte* numPtr7 = this.PixelAt(0, num22, num21, (byte*) data7.Scan0, 4);
                                for (int num23 = 0; num23 < header2.width; num23++)
                                {
                                    byte[] buffer5 = new byte[] { numPtr6[0], numPtr6[1], numPtr6[2], numPtr7[0] };
                                    stream2.Write(buffer5, 0, 4);
                                    numPtr6 += 4;
                                    numPtr7 += 4;
                                }
                            }
                            bitmap.UnlockBits(data6);
                            alpha.UnlockBits(data7);
                        }
                        else if (item2.BmpType == FSHBmpType.DXT3)
                        {
                            int num24 = item2.Bitmap.Width;
                            int height = item2.Bitmap.Height;
                            BitmapData data8 = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            BitmapData data9 = alpha.LockBits(rectangle2, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            int num26 = ((int) bounds.Width) * 4;
                            if ((num26 % 4) != 0)
                            {
                                num26 = 4 * ((num26 / 4) + 1);
                            }
                            if (((num24 & 3) > 0) || ((height & 3) > 0))
                            {
                                Debug.WriteLine("DXT Textures must have size divisible by 4.");
                            }
                            else
                            {
                                int num27 = 4 * num24;
                                while ((num27 & 4) > 0)
                                {
                                    num27++;
                                }
                                ulong[] numArray = new ulong[0x10];
                                byte[] buffer6 = new byte[(num24 * height) + 0x800];
                                fixed (byte* numRef = buffer6)
                                {
                                    byte* numPtr8;
                                    byte* numPtr9 = (byte*) data8.Scan0;
                                    num3 = 0;
                                    while (num3 < (height / 4))
                                    {
                                        for (int num28 = 0; num28 < (num24 / 4); num28++)
                                        {
                                            for (int num29 = 0; num29 < 4; num29++)
                                            {
                                                numPtr8 = (numPtr9 + (((4 * num3) + num29) * num27)) + (0x10 * num28);
                                                for (int num30 = 0; num30 < 4; num30++)
                                                {
                                                    numArray[(4 * num29) + num30] = (ulong) ((numPtr8[4 * num30] + (0x100 * numPtr8[(4 * num30) + 1])) + (0x10000 * numPtr8[(4 * num30) + 2]));
                                                }
                                            }
                                            fixed (ulong* numRef2 = numArray)
                                            {
                                                this.PackDXT(numRef2, ((numRef + ((4 * num3) * num24)) + (0x10 * num28)) + 8);
                                            }
                                        }
                                        num3++;
                                    }
                                    numPtr9 = (byte*) data9.Scan0;
                                    num3 = 0;
                                    while (num3 < (height / 4))
                                    {
                                        for (int num31 = 0; num31 < (num24 / 4); num31++)
                                        {
                                            for (int num32 = 0; num32 < 4; num32++)
                                            {
                                                numPtr8 = (numPtr9 + (((4 * num3) + num32) * num27)) + (0x10 * num31);
                                                byte* numPtr10 = ((numRef + ((4 * num3) * num24)) + (0x10 * num31)) + (2 * num32);
                                                numPtr10[0] = (byte) (((numPtr8[0] & 240) >> 4) + (numPtr8[4] & 240));
                                                numPtr10[1] = (byte) (((numPtr8[8] & 240) >> 4) + (numPtr8[12] & 240));
                                            }
                                        }
                                        num3++;
                                    }
                                }
                                stream2.Write(buffer6, 0, num24 * height);
                                bitmap.UnlockBits(data8);
                                alpha.UnlockBits(data9);
                            }
                        }
                        else if (item2.BmpType == FSHBmpType.DXT1)
                        {
                            int num33 = item2.Bitmap.Width;
                            int num34 = item2.Bitmap.Height;
                            BitmapData data10 = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            int num35 = ((int) bounds.Width) * 4;
                            if ((num35 % 4) != 0)
                            {
                                num35 = 4 * ((num35 / 4) + 1);
                            }
                            if (((num33 & 3) > 0) || ((num34 & 3) > 0))
                            {
                                Debug.WriteLine("DXT Textures must have size divisible by 4.");
                            }
                            else
                            {
                                int num36 = 4 * num33;
                                while ((num36 & 4) > 0)
                                {
                                    num36++;
                                }
                                ulong[] numArray2 = new ulong[0x10];
                                byte[] buffer7 = new byte[((num33 * num34) / 2) + 0x800];
                                fixed (byte* numRef3 = buffer7)
                                {
                                    byte* numPtr12 = (byte*) data10.Scan0;
                                    for (num3 = 0; num3 < (num34 / 4); num3++)
                                    {
                                        for (int num37 = 0; num37 < (num33 / 4); num37++)
                                        {
                                            for (int num38 = 0; num38 < 4; num38++)
                                            {
                                                byte* numPtr11 = (numPtr12 + (((4 * num3) + num38) * num36)) + (0x10 * num37);
                                                for (int num39 = 0; num39 < 4; num39++)
                                                {
                                                    numArray2[(4 * num38) + num39] = (ulong) ((numPtr11[4 * num39] + (0x100 * numPtr11[(4 * num39) + 1])) + (0x10000 * numPtr11[(4 * num39) + 2]));
                                                }
                                            }
                                            fixed (ulong* numRef4 = numArray2)
                                            {
                                                this.PackDXT(numRef4, (numRef3 + ((2 * num3) * num33)) + (8 * num37));
                                            }
                                        }
                                    }
                                }
                                stream2.Write(buffer7, 0, (num33 * num34) / 2);
                                bitmap.UnlockBits(data10);
                            }
                        }
                    }
                    num4++;
                }
                int num40 = 0;
                stream2.Position = 0x10L;
                if (this.saveGlobPal)
                {
                    stream2.Position += 8L;
                    num40++;
                }
                for (num40 = num40; num40 < entryArray.Length; num40++)
                {
                    stream2.Position += 4L;
                    stream2.Write(BitConverter.GetBytes(entryArray[num40].offset), 0, 4);
                }
                header.size = (int) stream2.Length;
                stream2.Position = 4L;
                stream2.Write(BitConverter.GetBytes(header.size), 0, 4);
                byte[] buffer = new byte[(uint) stream2.Length];
                stream2.Position = 0L;
                stream2.Read(buffer, 0, (int) stream2.Length);
                this.rawData = buffer;
                this.directory = entryArray;
                this.fshHead = header;
                if (this.IsCompressed)
                {
                    buffer = this.Comp(buffer, true);
                }
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
                Debug.WriteLine(exception.StackTrace);
            }
        }

        private unsafe int ScoreDXT(ulong* px, int nstep, ulong col1, ulong col2, ulong* pack)
        {
            int[] numArray = new int[3];
            int[] numArray2 = new int[3];
            Debug.Assert(numArray2.Length > 0, "The vdir array did not allocate");
            Debug.Assert(numArray.Length > 0, "The vec array did not allocate");
            byte* numPtr = (byte*) &col1;
            byte* numPtr2 = (byte*) &col2;
            numArray2[0] = numPtr2[0] - numPtr[0];
            numArray2[1] = numPtr2[1] - numPtr[1];
            numArray2[2] = numPtr2[2] - numPtr[2];
            int num = ((numArray2[0] * numArray2[0]) + (numArray2[1] * numArray2[1])) + (numArray2[2] * numArray2[2]);
            int num5 = 0;
            pack[0] = 0L;
            byte* numPtr3 = (byte*) (px + 15);
            int num4 = 15;
            while (num4 >= 0)
            {
                int num6;
                Debug.Assert(numArray2.Length > 0, "The vdir array did not allocate");
                Debug.Assert(numArray.Length > 0, "The vec array did not allocate");
                numArray[0] = numPtr3[0] - numPtr[0];
                numArray[1] = numPtr3[1] - numPtr[1];
                numArray[2] = numPtr3[2] - numPtr[2];
                int num2 = ((numArray[0] * numArray[0]) + (numArray[1] * numArray[1])) + (numArray[2] * numArray[2]);
                int num3 = ((numArray[0] * numArray2[0]) + (numArray[1] * numArray2[1])) + (numArray[2] * numArray2[2]);
                if (num != 0)
                {
                    num6 = ((nstep * num3) + (num >> 1)) / num;
                }
                else
                {
                    num6 = 0;
                }
                if (num6 < 0)
                {
                    num6 = 0;
                }
                if (num6 > nstep)
                {
                    num6 = nstep;
                }
                num5 += (num2 - (((2 * num6) * num3) / nstep)) + (((num6 * num6) * num) / (nstep * nstep));
                pack[0] = pack[0] << 2;
                if (num6 == nstep)
                {
                    pack[0] += (ulong) 1L;
                }
                else if (num6 != 0)
                {
                    pack[0] += (ulong)num6 + 1;
                }
                num4--;
                numPtr3 = (byte*) (px + num4);
            }
            return num5;
        }

        public void SetDirectoryName(string dirName)
        {
            Encoding.ASCII.GetBytes(dirName, 0, 4, this.fshHead.dirID, 0);
            this.isDirty = true;
        }

        private int UnpackDXT(byte mask, ushort c1, ushort c2, byte[] target, int idx)
        {
            ushort num = (ushort) (8 * (c1 & 0x1f));
            ushort num2 = (ushort) (4 * ((c1 >> 5) & 0x3f));
            ushort num3 = (ushort) (8 * (c1 >> 11));
            ushort num4 = (ushort) (8 * (c2 & 0x1f));
            ushort num5 = (ushort) (4 * ((c2 >> 5) & 0x3f));
            ushort num6 = (ushort) (8 * (c2 >> 11));
            switch (mask)
            {
                case 0:
                    target[idx] = (byte) num;
                    target[idx + 1] = (byte) num2;
                    target[idx + 2] = (byte) num3;
                    break;

                case 1:
                    target[idx] = (byte) num4;
                    target[idx + 1] = (byte) num5;
                    target[idx + 2] = (byte) num6;
                    break;

                case 2:
                    if (c1 <= c2)
                    {
                        target[idx] = (byte) ((num + num4) / 2);
                        target[idx + 1] = (byte) ((num2 + num5) / 2);
                        target[idx + 2] = (byte) ((num3 + num6) / 2);
                        break;
                    }
                    target[idx] = (byte) (((2 * num) + num4) / 3);
                    target[idx + 1] = (byte) (((2 * num2) + num5) / 3);
                    target[idx + 2] = (byte) (((2 * num3) + num6) / 3);
                    break;

                case 3:
                    if (c1 <= c2)
                    {
                        target[idx] = 0;
                        target[idx + 1] = 0;
                        target[idx + 2] = 0;
                        break;
                    }
                    target[idx] = (byte) ((num + (2 * num4)) / 3);
                    target[idx + 1] = (byte) ((num2 + (2 * num5)) / 3);
                    target[idx + 2] = (byte) ((num3 + (2 * num6)) / 3);
                    break;
            }
            int num7 = ((target[idx] << 0x10) + (target[idx + 1] << 8)) + target[idx + 2];
            return (num7 |= -16777216);
        }

        public void UpdateDirty()
        {
            this.fshHead.numBmps = this.bitmapItems.Count;
            this.isDirty = true;
        }

        public ArrayList Bitmaps
        {
            get
            {
                return this.bitmapItems;
            }
        }

        public FSHDirEntry[] Directory
        {
            get
            {
                return this.directory;
            }
        }

        public FSHHeader Header
        {
            get
            {
                return this.fshHead;
            }
        }

        public bool IsCompressed
        {
            get
            {
                return this.isFSHComp;
            }
            set
            {
                this.isFSHComp = value;
            }
        }

        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
        }

        public byte[] RawData
        {
            get
            {
                return this.rawData;
            }
        }
    }
}

