using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace B33Cpu.Hardware
{
    public sealed partial class B33Screen : UserControl, IB33Hardware/*, IB33Mouse*/
    {
        private ushort _memoryLocation;
        private ushort _lastPokedLocation;
        private readonly byte[] _screenMemory;
        private readonly Font _renderFont;
        private readonly Bitmap _screenBitmap;
        private readonly Point _zeroPoint;
        private bool _drawing;
        private bool _mouseCursorOn;
        private bool _cursorOn;
        private bool _shouldRedraw;
        private readonly System.Timers.Timer _cursorThread;
        private readonly System.Timers.Timer _refreshThread;

        private readonly SolidBrush[] _backgroundBrushesArray;
        private readonly SolidBrush[] _foregroundBrushesArray;

        private delegate void PokeCallBack(ushort addr, byte value);

        private delegate void RefreshCallBack();

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

        public ushort RequiredMemory
        {
            get { return 4001 + 3; }
        }

        public int MaxWidthResolution
        {
            get { return 10; }
        }

        public int MaxHeightResolution
        {
            get { return 10; }
        }

        public B33Screen()
        {
            InitializeComponent();

            _drawing = false;
            _shouldRedraw = false;
            _cursorThread = new System.Timers.Timer
            {
                AutoReset = true,
                Enabled = false,
                Interval = 500
            };
            _cursorThread.Elapsed += FlashCursor;
            
            _cursorOn = false;
            _zeroPoint = new Point(0, 0);
            _backgroundBrushesArray = new SolidBrush[8];
            _backgroundBrushesArray[0] = new SolidBrush(Color.Black);
            _backgroundBrushesArray[1] = new SolidBrush(Color.Blue);
            _backgroundBrushesArray[2] = new SolidBrush(Color.Green);
            _backgroundBrushesArray[3] = new SolidBrush(Color.Cyan);
            _backgroundBrushesArray[4] = new SolidBrush(Color.Red);
            _backgroundBrushesArray[5] = new SolidBrush(Color.Magenta);
            _backgroundBrushesArray[6] = new SolidBrush(Color.Brown);
            _backgroundBrushesArray[7] = new SolidBrush(Color.Gray);

            _foregroundBrushesArray = new SolidBrush[16];
            _foregroundBrushesArray[0] = new SolidBrush(Color.Black);
            _foregroundBrushesArray[1] = new SolidBrush(Color.Blue);
            _foregroundBrushesArray[2] = new SolidBrush(Color.Green);
            _foregroundBrushesArray[3] = new SolidBrush(Color.Cyan);
            _foregroundBrushesArray[4] = new SolidBrush(Color.Red);
            _foregroundBrushesArray[5] = new SolidBrush(Color.Magenta);
            _foregroundBrushesArray[6] = new SolidBrush(Color.Brown);
            _foregroundBrushesArray[7] = new SolidBrush(Color.Gray);
            _foregroundBrushesArray[8] = new SolidBrush(Color.Gray);
            _foregroundBrushesArray[9] = new SolidBrush(Color.LightBlue);
            _foregroundBrushesArray[10] = new SolidBrush(Color.LightGreen);
            _foregroundBrushesArray[11] = new SolidBrush(Color.LightCyan);
            _foregroundBrushesArray[12] = new SolidBrush(Color.Pink);
            _foregroundBrushesArray[13] = new SolidBrush(Color.Fuchsia);
            _foregroundBrushesArray[14] = new SolidBrush(Color.Yellow);
            _foregroundBrushesArray[15] = new SolidBrush(Color.White);

            _screenBitmap = new Bitmap(Width, Height);

            using (Graphics bmpGraphics = Graphics.FromImage(_screenBitmap))
            {
                bmpGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                bmpGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                bmpGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                bmpGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                bmpGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            }

            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.ContainerControl |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor
                     , true);

            _memoryLocation = 0xA000;
            _screenMemory = new byte[RequiredMemory];
            _renderFont = new Font("Courier New", 10f, FontStyle.Bold);
            _lastPokedLocation = 0xffff;
            _mouseCursorOn = false;
            Reset();

            _refreshThread = new System.Timers.Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 40
            };
            _refreshThread.Elapsed += RefreshSync;
        }

        private void RefreshSync(object sender, System.Timers.ElapsedEventArgs e)
        {
            _shouldRedraw = true;
            if (InvokeRequired)
            {
                var rcb = new RefreshCallBack(Invalidate);
                Invoke(rcb);
            }
            else
            {
                Invalidate();
            }
        }

        private void FlashCursor(object sender, System.Timers.ElapsedEventArgs e)
        {
            _cursorOn = !_cursorOn;
            Poke(MemoryLocation, Peek(MemoryLocation));
        }

        public void Reset()
        {
            lock (_screenMemory)
            {
                for (int i = 0; i < 4000; i += 2)
                {
                    _screenMemory[i] = 32;
                    _screenMemory[i + 1] = 7;
                }
                for (int i = 4001; i < 4003; i++)
                {
                    _screenMemory[i] = 0;
                }
            }
            _lastPokedLocation = 0xffff;

            using (Graphics bmpGraphics = Graphics.FromImage(_screenBitmap))
            {
                bmpGraphics.Clear(Color.Black);
            }
            //Refresh();
        }

        public void ScrollUp()
        {
            lock (_screenMemory)
            {
                for (int i = 0; i < 4000 - 160; i += 2)
                {
                    _screenMemory[i] = _screenMemory[i + 160];
                    _screenMemory[i + 1] = _screenMemory[i + 160 + 1];
                }
                for (int i = 4000 - 160; i < 4000; i += 2)
                {
                    _screenMemory[i] = 32;
                    _screenMemory[i + 1] = 7;
                }
            }
        }

        public void Clear()
        {
            lock (_screenMemory)
            {
                for (int i = 0; i < 4000; i += 2)
                {
                    _screenMemory[i] = 32;
                    _screenMemory[i + 1] = 7;
                }
            }

            Poke(MemoryLocation, 32);
            _lastPokedLocation = 0xffff;
            using (Graphics bmpGraphics = Graphics.FromImage(_screenBitmap))
            {
                bmpGraphics.Clear(Color.Black);
            }
            //if (InvokeRequired)
            //{
            //    var rcb = new RefreshCallBack(Refresh);
            //    Invoke(rcb);
            //}
            //else
            //{
            //    //Refresh();
            //}
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

            if (memLoc > (3999 + 3))
                return;

            //if (memLoc == 4000)
            //{
            //    if (value == 1)
            //        ShowCursor();
            //    else HideCursor();

            //    Poke((ushort) (address - 1), Peek((ushort) (address - 1)));
            //    return;
            //}

            if (memLoc == 4000)
            {
                if (value == 1)
                {
                    _cursorThread.Enabled = true;
                    _cursorOn = true;
                }
                if (value == 0)
                {
                    _cursorThread.Enabled = false;
                    _cursorOn = false;
                }
                Poke(MemoryLocation, Peek(MemoryLocation));
                return;
            }
            if (InvokeRequired)
            {
                var pcb = new PokeCallBack(PokeAddress);
                Invoke(pcb, memLoc, value);
            }
            else
            {
                PokeAddress(memLoc, value);
            }
        }

        private void PokeAddress(ushort memLoc, byte value)
        {
            if (_drawing)
                return;
            _drawing = true;
            lock (_screenMemory)
            {
                _screenMemory[memLoc] = value;
                _lastPokedLocation = memLoc;
            }

            int invertedRow = 0;
            int invertedColumn = 0;

            if (_mouseCursorOn)
            {
                Point p = PointToClient(Cursor.Position);
                invertedRow = p.Y / 12;
                invertedColumn = p.X / 8;
            }

            using (Graphics bmpGraphics = Graphics.FromImage(_screenBitmap))
            {
                //bmpGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                //bmpGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                //bmpGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                int xLoc = 0;
                int yLoc = 0;

                if (_lastPokedLocation == 0xffff)
                    bmpGraphics.Clear(Color.Black);

                for (int i = 0; i < 4000; i += 2)
                {
                    //if (_lastPokedLocation != 0xffff && i != _lastPokedLocation)
                    //{
                    //    if (((xLoc % 640) == 0) && (xLoc != 0))
                    //    {
                    //        yLoc += 12;
                    //        xLoc = 0;
                    //    }
                    //    xLoc += 8;
                    //    continue;
                    //}
                    if (((xLoc % 640) == 0) && (xLoc != 0))
                    {
                        yLoc += 12;
                        xLoc = 0;
                    }

                    //string s = Convert.ToChar(_screenMemory[i]).ToString();
                    var pf = new Point(xLoc, yLoc);

                    //if (_mouseCursorOn && (xLoc >= invertedColumn && xLoc <= (invertedColumn + 8)) && (yLoc >= invertedRow && yLoc <= (invertedRow + 12)))
                    //{
                    //    bmpGraphics.FillRectangle(Brushes.Gray, xLoc + 2, yLoc + 3, 8f, 11f);
                    //}
                    //else
                    //{
                    bmpGraphics.FillRectangle(_backgroundBrushesArray[((byte)((_screenMemory[i + 1]) & 112)) / 16],
                        xLoc + 2, yLoc + 3, 8f, 11f);
                    //}

                    //if (s == "_")
                    //{
                    //    bmpGraphics.DrawLine(new Pen(_foregroundBrushesArray[((byte)((_screenMemory[i + 1]) & 15))]),
                    //        pf.X + 2, pf.Y + 10, pf.X + 6f, pf.Y + 10);
                    //    bmpGraphics.DrawLine(new Pen(_foregroundBrushesArray[((byte)((_screenMemory[i + 1]) & 15))]),
                    //        pf.X + 2, pf.Y + 11, pf.X + 6f, pf.Y + 11);
                    //    bmpGraphics.DrawLine(new Pen(_foregroundBrushesArray[((byte)((_screenMemory[i + 1]) & 15))]),
                    //        pf.X + 2, pf.Y + 12, pf.X + 6f, pf.Y + 12);
                    //}
                    //else
                    //{
                    Brush b = _screenMemory[i + 1] == 1
                        ? Brushes.Blue
                        : _foregroundBrushesArray[((byte)((_screenMemory[i + 1]) & 15))];
                    bmpGraphics.DrawString(Convert.ToChar(_screenMemory[i]).ToString(), _renderFont,
                        b, pf);
                    
                    ushort loc = BitConverter.ToUInt16(new[] { _screenMemory[4001], _screenMemory[4002] }, 0);
                    
                    if (_cursorOn && loc * 2 == i)
                    {
                        bmpGraphics.DrawLine(new Pen(_foregroundBrushesArray[((byte)((_screenMemory[i + 1]) & 15))]),
                            pf.X + 3, pf.Y + 12, pf.X + 9f, pf.Y + 12);
                        bmpGraphics.DrawLine(new Pen(_foregroundBrushesArray[((byte)((_screenMemory[i + 1]) & 15))]),
                            pf.X + 3, pf.Y + 11, pf.X + 9f, pf.Y + 11);
                    }
                    //}

                    xLoc += 8;
                }
            }
            _drawing = false;
            //Invalidate();
            //Update();
        }

        public byte Peek(ushort address)
        {
            return Peek(address, false);
        }

        public byte Peek(ushort address, bool memoryViewer)
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

        private void B33Screen_Load(object sender, EventArgs e)
        {

        }

        private void B33Screen_SizeChanged(object sender, EventArgs e)
        {
            Width = 644;
            Height = 304;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_shouldRedraw)
                return;
            var compMode = e.Graphics.CompositingMode;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            e.Graphics.DrawImage(_screenBitmap, _zeroPoint);
            e.Graphics.CompositingMode = compMode;

            base.OnPaint(e);
            _shouldRedraw = false;
        }

        public void ShowCursor()
        {
            _mouseCursorOn = true;
            Poke(_memoryLocation, Peek(_memoryLocation));
        }

        public void HideCursor()
        {
            _mouseCursorOn = false;
            Poke(_memoryLocation, Peek(_memoryLocation));
        }

        private void B33Screen_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseCursorOn)
                Poke(_memoryLocation, Peek(_memoryLocation));
        }
    }
}
