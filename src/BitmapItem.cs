namespace FSHLib
{
    using System;
    using System.Drawing;
    using System.Text;

    public class BitmapItem
    {
        private System.Drawing.Bitmap alpha;
        private System.Drawing.Bitmap bitmap;
        private FSHBmpType bmpType;
        private string[] comments;
        private byte[] dirName = new byte[4];
        private bool isComp;
        private Color[] palette;

        public BitmapItem()
        {
            this.dirName = Encoding.ASCII.GetBytes("FiSH");
            this.bmpType = FSHBmpType.DXT1;
            this.isComp = false;
            this.comments = new string[0];
        }

        public void SetDirName(string name)
        {
            this.dirName = new byte[4];
            Encoding.ASCII.GetBytes(name, 0, 4, this.dirName, 0);
        }

        public System.Drawing.Bitmap Alpha
        {
            get
            {
                return this.alpha;
            }
            set
            {
                this.alpha = value;
            }
        }

        public System.Drawing.Bitmap Bitmap
        {
            get
            {
                return this.bitmap;
            }
            set
            {
                this.bitmap = value;
            }
        }

        public FSHBmpType BmpType
        {
            get
            {
                return this.bmpType;
            }
            set
            {
                this.bmpType = value;
            }
        }

        public string[] Comments
        {
            get
            {
                return this.comments;
            }
            set
            {
                this.comments = value;
            }
        }

        public byte[] DirName
        {
            get
            {
                return this.dirName;
            }
        }

        public bool IsCompressed
        {
            get
            {
                return this.isComp;
            }
            set
            {
                this.isComp = value;
            }
        }

        public Color[] Palette
        {
            get
            {
                return this.palette;
            }
            set
            {
                this.palette = value;
            }
        }
    }
}

