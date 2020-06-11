using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace B33VirtualMachine
{
    public sealed partial class B33Screen : UserControl, IB33Hardware
    {
        private ushort _memoryLocation;
        private readonly byte[] _screenMemory;
        private readonly Font _renderFont;
        private ushort _lastPokedLocation;
        private bool _resetInvoked;
        private readonly Bitmap _screenBitmap;
        private readonly List<string> _graphicsList;

        public string GraphicsList
        {
            set { _graphicsList.Add(value); }
        }

        public ushort MemoryLocation
        {
            get
            {
                return _memoryLocation;
            }
            set
            {
                _memoryLocation = value;
            }
        }

        public bool ThreadInvokeRequired
        {
            get { return InvokeRequired; }
        }

        public ushort RequiredMemory
        {
            get { return 4000; }
        }

        public B33Screen()
        {
            InitializeComponent();

            _graphicsList = new List<string>();
            _screenBitmap = new Bitmap(Width, Height);
            _lastPokedLocation = 0;
            _resetInvoked = false;
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.ContainerControl |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor
                     , true);

            _memoryLocation = 0xA000;
            _screenMemory = new byte[4000];
            _renderFont = new Font("Courier New", 10f, FontStyle.Bold);
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < 4000; i += 2)
            {
                _screenMemory[i] = 32;
                _screenMemory[i + 1] = 7;
            }
            _resetInvoked = true;
            _graphicsList.Clear();
            Refresh();
        }

        public void Poke(ushort address, byte value)
        {
            ushort memLoc;

            try
            {
                memLoc = (ushort)(address - _memoryLocation);
            }
            catch (Exception)
            {
                return;
            }

            if (memLoc > 3999)
                return;

            _screenMemory[memLoc] = value;
            _lastPokedLocation = memLoc;
            Refresh();
        }

        public byte Peek(ushort address)
        {
            ushort memLoc;

            try
            {
                memLoc = (ushort)(address - _memoryLocation);
            }
            catch (Exception)
            {
                return 0;
            }

            if (memLoc > 3999)
                return 0;

            return _screenMemory[memLoc];
        }

        public void ForceRefresh()
        {
            Refresh();
        }

        private void B33ScreenSizeChanged(object sender, EventArgs e)
        {
            Width = 644;
            Height = 280;
        }

        private SolidBrush GetBackgroundBrush(byte attr)
        {
            var bgBit = (byte)(attr & 112);
            SolidBrush bgBrush = null;

            switch (bgBit)
            {
                case 0:
                    bgBrush = new SolidBrush(Color.Black);
                    break;
                case 16:
                    bgBrush = new SolidBrush(Color.Blue);
                    break;
                case 32:
                    bgBrush = new SolidBrush(Color.Green);
                    break;
                case 48:
                    bgBrush = new SolidBrush(Color.Cyan);
                    break;
                case 64:
                    bgBrush = new SolidBrush(Color.Red);
                    break;
                case 80:
                    bgBrush = new SolidBrush(Color.Magenta);
                    break;
                case 96:
                    bgBrush = new SolidBrush(Color.Brown);
                    break;
                case 112:
                    bgBrush = new SolidBrush(Color.Gray);
                    break;
            }

            return bgBrush ?? (new SolidBrush(Color.Black));
        }

        private SolidBrush GetForegroundBrush(byte attr)
        {
            var fgBit = (byte)(attr & 15);
            SolidBrush fgBrush = null;

            switch (fgBit)
            {
                case 0:
                    fgBrush = new SolidBrush(Color.Black);
                    break;
                case 1:
                    fgBrush = new SolidBrush(Color.Blue);
                    break;
                case 2:
                    fgBrush = new SolidBrush(Color.Green);
                    break;
                case 3:
                    fgBrush = new SolidBrush(Color.Cyan);
                    break;
                case 4:
                    fgBrush = new SolidBrush(Color.Red);
                    break;
                case 5:
                    fgBrush = new SolidBrush(Color.Magenta);
                    break;
                case 6:
                    fgBrush = new SolidBrush(Color.Brown);
                    break;
                case 7:
                    fgBrush = new SolidBrush(Color.Gray);
                    break;
                case 8:
                    fgBrush = new SolidBrush(Color.Gray);
                    break;
                case 9:
                    fgBrush = new SolidBrush(Color.LightBlue);
                    break;
                case 10:
                    fgBrush = new SolidBrush(Color.LightGreen);
                    break;
                case 11:
                    fgBrush = new SolidBrush(Color.LightCyan);
                    break;
                case 12:
                    fgBrush = new SolidBrush(Color.Pink);
                    break;
                case 13:
                    fgBrush = new SolidBrush(Color.Fuchsia);
                    break;
                case 14:
                    fgBrush = new SolidBrush(Color.Yellow);
                    break;
                case 15:
                    fgBrush = new SolidBrush(Color.White);
                    break;
            }

            return fgBrush ?? (new SolidBrush(Color.Gray));
        }

        private void B33ScreenPaint(object sender, PaintEventArgs e)
        {
            Graphics bmpGraphics = Graphics.FromImage(_screenBitmap);
            int xLoc = 0;
            int yLoc = 0;

            bmpGraphics.Clear(Color.Black);
            _resetInvoked = true;
            for (int i = 0; i < 4000; i += 2)
            {
                SolidBrush bgBrush = GetBackgroundBrush(_screenMemory[i + 1]);
                SolidBrush fgBrush = GetForegroundBrush(_screenMemory[i + 1]);

                if (((xLoc % 640) == 0) && (xLoc != 0))
                {
                    yLoc += 12;
                    xLoc = 0;
                }
                if ((_resetInvoked) || (_lastPokedLocation == i || _lastPokedLocation == i + 1))
                {
                    string s = System.Text.Encoding.ASCII.GetString(_screenMemory, i, 1);
                    var pf = new PointF(xLoc, yLoc);

                    bmpGraphics.FillRectangle(bgBrush, xLoc + 2, yLoc + 3, 8f, 11f);
                    if (s == "_")
                    {
                        bmpGraphics.DrawLine(new Pen(fgBrush), pf.X + 2, pf.Y + 10, pf.X + 6f, pf.Y + 10);
                        bmpGraphics.DrawLine(new Pen(fgBrush), pf.X + 2, pf.Y + 11, pf.X + 6f, pf.Y + 11);
                        bmpGraphics.DrawLine(new Pen(fgBrush), pf.X + 2, pf.Y + 12, pf.X + 6f, pf.Y + 12);
                    }
                    else
                    {
                        bmpGraphics.DrawString(s, _renderFont, fgBrush, pf);
                    }
                }
                xLoc += 8;
            }

            var graphicsPen = new Pen(Color.FromArgb(0, 0, 0));
            var graphicsCoords = new Point(0, 0);

            foreach (string cmd in _graphicsList)
            {
                if (cmd.StartsWith("COLOR"))
                {
                    int red = int.Parse(cmd.Replace("COLOR ", "").Split(',')[0]);
                    int green = int.Parse(cmd.Replace("COLOR ", "").Split(',')[1]);
                    int blue = int.Parse(cmd.Replace("COLOR ", "").Split(',')[2]);

                    graphicsPen = new Pen(Color.FromArgb(red, green, blue));
                }
                if (cmd.StartsWith("MOVETO"))
                {
                    int x = int.Parse(cmd.Replace("MOVETO ", "").Split(',')[0]);
                    int y = int.Parse(cmd.Replace("MOVETO ", "").Split(',')[1]);

                    graphicsCoords = new Point(x, y);
                }
                if (cmd.StartsWith("LINETO"))
                {
                    int x = int.Parse(cmd.Replace("LINETO ", "").Split(',')[0]);
                    int y = int.Parse(cmd.Replace("LINETO ", "").Split(',')[1]);


                    bmpGraphics.DrawLine(graphicsPen, graphicsCoords, new Point(x, y));
                }
            }
            _resetInvoked = false;

            e.Graphics.DrawImage(_screenBitmap, new Point(0, 0));
            bmpGraphics.Dispose();
            //bmp.Dispose();
        }
    }
}
