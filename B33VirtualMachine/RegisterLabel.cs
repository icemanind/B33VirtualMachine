using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace B33VirtualMachine
{
    public sealed partial class RegisterLabel : UserControl
    {
        private readonly Bitmap _bmp;
        private string _text;
        private readonly Font _renderFont;
        private bool _drawing;

        public override string Text { get { return _text; } set { _text = value; UpdateBitmap(); } }

        public RegisterLabel()
        {
            InitializeComponent();

            _drawing = false;

            _bmp = new Bitmap(Width, Height);
            Text = "";
            using (Graphics bmpGraphics = Graphics.FromImage(_bmp))
            {
                bmpGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                bmpGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                bmpGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                bmpGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                bmpGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                bmpGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            }

            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.ContainerControl |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            _renderFont = new Font("Courier New", 12, FontStyle.Bold);
            UpdateBitmap();
        }

        private void RegisterLabel_Load(object sender, EventArgs e)
        {

        }

        private void RegisterLabel_SizeChanged(object sender, EventArgs e)
        {
            Width = 1407;
            Height = 33;
        }

        private void UpdateBitmap()
        {
            if (_drawing)
                return;
            _drawing = true;
            using (Graphics g = Graphics.FromImage(_bmp))
            {
                g.Clear(Color.LightYellow);

                g.DrawString(Text, _renderFont, Brushes.Black, new PointF(0, 0));
            }
            _drawing = false;
        }

        private void RegisterLabel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_bmp, new Point(0, 0));
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            Refresh();
        }
    }
}
