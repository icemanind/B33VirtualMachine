using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace B33VirtualMachineBasicCompiler
{
    [ToolboxBitmap(typeof(RichTextBox))]
    internal class B33RichTextEditor : RichTextBox
    {
        public enum LineCounting
        {
            Crlf,
            AsDisplayed
        }

        private User32.CharFormat _charFormat;
        private readonly IntPtr _lParam1;

        private int _savedScrollLine;
        private int _savedSelectionStart;
        private int _savedSelectionEnd;
        private Pen _borderPen;
        private StringFormat _stringDrawingFormat;

        private string _sformat;
        private int _ndigits;
        private int _lnw = -1;
        public bool LineNumbers;
        private Font _numberFont;
        private LineCounting _numberLineCounting;
        private StringAlignment _numberAlignment;
        private Color _numberColor;
        private bool _numberLeadingZeroes;
        private Color _numberBorder;
        private int _numberPadding;
        public float _NumberBorderThickness;
        private Color _numberBackground1;
        private Color _numberBackground2;
        private bool _paintingDisabled;
        private int[] _charIndexForTextLine;
        private String _text2;

        int _lastWidth;

        private int LineNumberWidth
        {
            get
            {
                if (_lnw > 0) return _lnw;
                if (NumberLineCounting == LineCounting.Crlf)
                {
                    _ndigits = (CharIndexForTextLine.Length == 0)
                        ? 1
                        : (int)(1 + Math.Log(CharIndexForTextLine.Length, 10));
                }
                else
                {
                    int n = GetDisplayLineCount();
                    _ndigits = (n == 0)
                        ? 1
                        : (int)(1 + Math.Log(n, 10));
                }
                var s = new String('0', _ndigits);
                var b = new Bitmap(400, 400);
                var g = Graphics.FromImage(b);
                SizeF size = g.MeasureString(s, NumberFont);
                g.Dispose();
                _lnw = NumberPadding * 2 + 4 + (int)(size.Width + 0.5 + NumberBorderThickness);
                _sformat = "{0:D" + _ndigits + "}";
                return _lnw;
            }
        }

        public bool ShowLineNumbers
        {
            get
            {
                return LineNumbers;
            }
            set
            {
                if (value == LineNumbers) return;
                SetLeftMargin(value ? LineNumberWidth + Margin.Left : Margin.Left);
                LineNumbers = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public Font NumberFont
        {
            get { return _numberFont; }
            set
            {
                if (_numberFont == value) return;
                _lnw = -1;
                _numberFont = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public LineCounting NumberLineCounting
        {
            get { return _numberLineCounting; }
            set
            {
                if (_numberLineCounting == value) return;
                _lnw = -1;
                _numberLineCounting = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public StringAlignment NumberAlignment
        {
            get { return _numberAlignment; }
            set
            {
                if (_numberAlignment == value) return;
                _numberAlignment = value;
                SetStringDrawingFormat();
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public Color NumberColor
        {
            get { return _numberColor; }
            set
            {
                if (_numberColor.ToArgb() == value.ToArgb()) return;
                _numberColor = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public bool NumberLeadingZeroes
        {
            get { return _numberLeadingZeroes; }
            set
            {
                if (_numberLeadingZeroes == value) return;
                _numberLeadingZeroes = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public Color NumberBorder
        {
            get { return _numberBorder; }
            set
            {
                if (_numberBorder.ToArgb() == value.ToArgb()) return;
                _numberBorder = value;
                NewBorderPen();
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public int NumberPadding
        {
            get { return _numberPadding; }
            set
            {
                if (_numberPadding == value) return;
                _lnw = -1;
                _numberPadding = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public float NumberBorderThickness
        {
            get { return _NumberBorderThickness; }
            set
            {
                if (_NumberBorderThickness == value) return;
                _lnw = -1;
                _NumberBorderThickness = value;
                NewBorderPen();
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public Color NumberBackground1
        {
            get { return _numberBackground1; }
            set
            {
                if (_numberBackground1.ToArgb() == value.ToArgb()) return;
                _numberBackground1 = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        public Color NumberBackground2
        {
            get { return _numberBackground2; }
            set
            {
                if (_numberBackground2.ToArgb() == value.ToArgb()) return;
                _numberBackground2 = value;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
            }
        }

        private int FirstVisibleDisplayLine
        {
            get
            {
                return User32.SendMessage(Handle, (int)User32.Msgs.EmGetFirstVisibleLine, 0, 0);
            }
            set
            {
                int current = FirstVisibleDisplayLine;
                int delta = value - current;
                User32.SendMessage(Handle, (int)User32.Msgs.EmLineScroll, 0, delta);
            }
        }

        private int NumberOfVisibleDisplayLines
        {
            get
            {
                int topIndex = GetCharIndexFromPosition(new Point(1, 1));
                int bottomIndex = GetCharIndexFromPosition(new Point(1, Height - 1));
                int topLine = GetLineFromCharIndex(topIndex);
                int bottomLine = GetLineFromCharIndex(bottomIndex);
                int n = bottomLine - topLine + 1;
                return n;
            }
        }

        private int FirstVisibleTextLine
        {
            get
            {
                int c = GetCharIndexFromPos(1, 1);
                for (int i = 0; i < CharIndexForTextLine.Length; i++)
                {
                    if (c <= CharIndexForTextLine[i]) return i;
                }
                return CharIndexForTextLine.Length;
            }
        }

        private int LastVisibleTextLine
        {
            get
            {
                int c = GetCharIndexFromPos(1, Bounds.Y + Bounds.Height);
                for (int i = 0; i < CharIndexForTextLine.Length; i++)
                {
                    if (c < CharIndexForTextLine[i]) return i;
                }
                return CharIndexForTextLine.Length;
            }
        }

        private int NumberOfVisibleTextLines
        {
            get
            {
                return LastVisibleTextLine - FirstVisibleTextLine;
            }
        }


        public int FirstVisibleLine
        {
            get
            {
                if (NumberLineCounting == LineCounting.Crlf)
                    return FirstVisibleTextLine;
                return FirstVisibleDisplayLine;
            }
        }

        public int NumberOfVisibleLines
        {
            get
            {
                if (NumberLineCounting == LineCounting.Crlf)
                    return NumberOfVisibleTextLines;
                return NumberOfVisibleDisplayLines;
            }
        }

        private int[] CharIndexForTextLine
        {
            get
            {
                if (_charIndexForTextLine == null)
                {
                    var list = new List<int>();
                    int ix = 0;
                    foreach (var c in Text2)
                    {
                        if (c == '\n') list.Add(ix);
                        ix++;
                    }
                    _charIndexForTextLine = list.ToArray();
                }
                return _charIndexForTextLine;
            }
        }

        private String Text2
        {
            get { return _text2 ?? (_text2 = Text); }
        }

        public B33RichTextEditor()
        {
            _charFormat = new User32.CharFormat
            {
                cbSize = Marshal.SizeOf(typeof(User32.CharFormat)),
                szFaceName = new char[32]
            };

            _lParam1 = Marshal.AllocCoTaskMem(_charFormat.cbSize);

            NumberFont = new Font("Consolas",
                                9.75F,
                                FontStyle.Regular,
                                GraphicsUnit.Point, 0);

            NumberColor = Color.FromName("DarkGray");
            NumberLineCounting = LineCounting.Crlf;
            NumberAlignment = StringAlignment.Center;
            NumberBorder = SystemColors.ControlDark;
            NumberBorderThickness = 1;
            NumberPadding = 2;
            NumberBackground1 = SystemColors.ControlLight;
            NumberBackground2 = SystemColors.Window;
            SetStringDrawingFormat();

            System.Security.Cryptography.SHA1.Create();
        }

        ~B33RichTextEditor()
        {
            Marshal.FreeCoTaskMem(_lParam1);
        }

        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            if (keyData == Keys.Tab && AcceptsTab)
            {
                int lineNumber = GetLineFromCharIndex(SelectionStart);
                int fc = GetFirstCharIndexFromLine(lineNumber);
                int sc = SelectionStart;
                int numspaces = (sc - fc) % 12;
                numspaces = 12 - numspaces;
                if (numspaces == 0)
                    numspaces = 12;

                SelectionLength = 0;
                SelectedText = new string(' ', numspaces);
                return true;
            }
            return base.ProcessCmdKey(ref m, keyData);
        }

        private void SetStringDrawingFormat()
        {
            _stringDrawingFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = NumberAlignment,
                Trimming = StringTrimming.None,
            };
        }

        protected override void OnTextChanged(EventArgs e)
        {
            NeedRecomputeOfLineNumbers();
            base.OnTextChanged(e);
        }

        public void BeginUpdate()
        {
            User32.SendMessage(Handle, (int)User32.Msgs.WmSetRedraw, 0, IntPtr.Zero);
        }

        public void EndUpdate()
        {
            User32.SendMessage(Handle, (int)User32.Msgs.WmSetRedraw, 1, IntPtr.Zero);
        }

        public IntPtr BeginUpdateAndSuspendEvents()
        {
            User32.SendMessage(Handle, (int)User32.Msgs.WmSetRedraw, 0, IntPtr.Zero);
            IntPtr eventMask = User32.SendMessage(Handle, User32.Msgs.EmGetEventMask, 0, IntPtr.Zero);

            return eventMask;
        }

        public void EndUpdateAndResumeEvents(IntPtr eventMask)
        {
            User32.SendMessage(Handle, User32.Msgs.EmSetEventMask, 0, eventMask);
            User32.SendMessage(Handle, User32.Msgs.WmSetRedraw, 1, IntPtr.Zero);
            NeedRecomputeOfLineNumbers();
            Invalidate();
        }

        public void GetSelection(out int start, out int end)
        {
            User32.SendMessageRef(Handle, (int)User32.Msgs.EmGetSel, out start, out end);
        }

        public void SetSelection(int start, int end)
        {
            User32.SendMessage(Handle, (int)User32.Msgs.EmSetSel, start, end);
        }

        public void BeginUpdateAndSaveState()
        {
            User32.SendMessage(Handle, (int)User32.Msgs.WmSetRedraw, 0, IntPtr.Zero);
            _savedScrollLine = FirstVisibleDisplayLine;

            GetSelection(out _savedSelectionStart, out _savedSelectionEnd);
        }

        public void EndUpdateAndRestoreState()
        {
            int line1 = FirstVisibleDisplayLine;
            Scroll(_savedScrollLine - line1);

            SetSelection(_savedSelectionStart, _savedSelectionEnd);

            User32.SendMessage(Handle, (int)User32.Msgs.WmSetRedraw, 1, IntPtr.Zero);

            Refresh();
        }

        private void NeedRecomputeOfLineNumbers()
        {
            _charIndexForTextLine = null;
            _text2 = null;
            _lnw = -1;

            if (_paintingDisabled) return;

            User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
        }

        public void SuspendLineNumberPainting()
        {
            _paintingDisabled = true;
        }

        public void ResumeLineNumberPainting()
        {
            _paintingDisabled = false;
        }

        private void NewBorderPen()
        {
            _borderPen = new Pen(NumberBorder) { Width = NumberBorderThickness };
            _borderPen.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
        }

        protected override void WndProc(ref Message m)
        {
            bool handled = false;
            switch (m.Msg)
            {
                case (int)User32.Msgs.WmPaint:
                    if (_paintingDisabled) return;
                    if (LineNumbers)
                    {
                        base.WndProc(ref m);
                        PaintLineNumbers();
                        handled = true;
                    }
                    break;

                case (int)User32.Msgs.WmChar:
                    NeedRecomputeOfLineNumbers();
                    break;
            }

            if (!handled)
                base.WndProc(ref m);
        }

        private void PaintLineNumbers()
        {
            if (_paintingDisabled) return;

            int w = LineNumberWidth;
            if (w != _lastWidth)
            {
                SetLeftMargin(w + Margin.Left);
                _lastWidth = w;
                User32.SendMessage(Handle, User32.Msgs.WmPaint, 0, 0);
                return;
            }

            var buffer = new Bitmap(w, Bounds.Height);
            Graphics g = Graphics.FromImage(buffer);

            Brush forebrush = new SolidBrush(NumberColor);
            var rect = new Rectangle(0, 0, w, Bounds.Height);

            bool wantDivider = NumberBackground1.ToArgb() == NumberBackground2.ToArgb();
            Brush backBrush = (wantDivider)
                ? new SolidBrush(NumberBackground2)
                : SystemBrushes.Window;

            g.FillRectangle(backBrush, rect);

            int n = (NumberLineCounting == LineCounting.Crlf)
                ? NumberOfVisibleTextLines
                : NumberOfVisibleDisplayLines;

            int first = (NumberLineCounting == LineCounting.Crlf)
                ? FirstVisibleTextLine
                : FirstVisibleDisplayLine + 1;

            int py = 0;
            int w2 = w - 2 - (int)NumberBorderThickness;
            var dividerPen = new Pen(NumberColor);

            for (int i = 0; i <= n; i++)
            {
                int ix = first + i;
                int c = (NumberLineCounting == LineCounting.Crlf)
                    ? GetCharIndexForTextLine(ix)
                    : GetCharIndexForDisplayLine(ix) - 1;

                var p = GetPosFromCharIndex(c + 1);

                Rectangle r4;

                if (i == n)
                {
                    if (Bounds.Height <= py) continue;
                    r4 = new Rectangle(1, py, w2, Bounds.Height - py);
                }
                else
                {
                    if (p.Y <= py) continue;
                    r4 = new Rectangle(1, py, w2, p.Y - py);
                }

                if (wantDivider)
                {
                    if (i != n)
                        g.DrawLine(dividerPen, 1, p.Y + 1, w2, p.Y + 1);
                }
                else
                {
                    var brush = new LinearGradientBrush(r4,
                        NumberBackground1,
                        NumberBackground2,
                        LinearGradientMode.Vertical);
                    g.FillRectangle(brush, r4);
                }

                if (NumberLineCounting == LineCounting.Crlf) ix++;

                if (NumberAlignment == StringAlignment.Near)
                    rect.Offset(0, 3);

                var s = (NumberLeadingZeroes) ? String.Format(_sformat, ix) : ix.ToString(CultureInfo.InvariantCulture);
                g.DrawString(s, NumberFont, forebrush, r4, _stringDrawingFormat);
                py = p.Y;
            }

            if (NumberBorderThickness != 0.0)
            {
                int t = (int)(w - (NumberBorderThickness + 0.5) / 2) - 1;
                g.DrawLine(_borderPen, t, 0, t, Bounds.Height);
                //g.DrawLine(_borderPen, w-2, 0, w-2, this.Bounds.Height);
            }

            Graphics g1 = CreateGraphics();
            g1.DrawImage(buffer, new Point(0, 0));
            g1.Dispose();
            g.Dispose();
        }

        private int GetCharIndexFromPos(int x, int y)
        {
            var p = new User32.PointL { X = x, Y = y };
            int rawSize = Marshal.SizeOf(typeof(User32.PointL));
            IntPtr lParam = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(p, lParam, false);
            int r = User32.SendMessage(Handle, (int)User32.Msgs.EmCharFromPos, 0, lParam);
            Marshal.FreeHGlobal(lParam);
            return r;
        }


        private Point GetPosFromCharIndex(int ix)
        {
            int rawSize = Marshal.SizeOf(typeof(User32.PointL));
            IntPtr wParam = Marshal.AllocHGlobal(rawSize);
            User32.SendMessage(Handle, (int)User32.Msgs.EmPosFromChar, (int)wParam, ix);

            var p1 = (User32.PointL)Marshal.PtrToStructure(wParam, typeof(User32.PointL));

            Marshal.FreeHGlobal(wParam);
            var p = new Point { X = p1.X, Y = p1.Y };
            return p;
        }

        private int GetCharIndexForDisplayLine(int line)
        {
            return User32.SendMessage(Handle, (int)User32.Msgs.EmLineIndex, line, 0);
        }

        private int GetDisplayLineCount()
        {
            return User32.SendMessage(Handle, (int)User32.Msgs.EmGetLineCount, 0, 0);
        }

        /// <summary>
        ///   Sets the color of the characters in the given range.
        /// </summary>
        ///
        /// <remarks>
        /// Calling this is equivalent to calling
        /// <code>
        ///   richTextBox.Select(start, end-start);
        ///   this.richTextBox1.SelectionColor = color;
        /// </code>
        /// ...but without the error and bounds checking.
        /// </remarks>
        ///
        public void SetSelectionColor(int start, int end, Color color)
        {
            User32.SendMessage(Handle, (int)User32.Msgs.EmSetSel, start, end);

            _charFormat.dwMask = 0x40000000;
            _charFormat.dwEffects = 0;
            _charFormat.crTextColor = ColorTranslator.ToWin32(color);

            Marshal.StructureToPtr(_charFormat, _lParam1, false);
            User32.SendMessage(Handle, (int)User32.Msgs.EmSetCharFormat, User32.ScfSelection, _lParam1);
        }

        private void SetLeftMargin(int widthInPixels)
        {
            User32.SendMessage(Handle, (int)User32.Msgs.EmSetMargins, User32.EcLeftMargin,
                               widthInPixels);
        }

        public Tuple<int, int> GetMargins()
        {
            int r = User32.SendMessage(Handle, (int)User32.Msgs.EmGetMargins, 0, 0);
            return Tuple.New(r & 0x0000FFFF, (r >> 16) & 0x0000FFFF);
        }

        public void Scroll(int delta)
        {
            User32.SendMessage(Handle, (int)User32.Msgs.EmLineScroll, 0, delta);
        }

        private int GetCharIndexForTextLine(int ix)
        {
            if (ix >= CharIndexForTextLine.Length) return 0;
            if (ix < 0) return 0;
            return CharIndexForTextLine[ix];
        }
    }

    internal static class Tuple
    {
        public static Tuple<T1, T2> New<T1, T2>(T1 v1, T2 v2)
        {
            return new Tuple<T1, T2>(v1, v2);
        }
    }

    internal class Tuple<T1, T2>
    {
        public Tuple(T1 v1, T2 v2)
        {
            V1 = v1;
            V2 = v2;
        }

        public T1 V1 { get; set; }
        public T2 V2 { get; set; }
    }
}
