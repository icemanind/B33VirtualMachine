using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace B33Cpu.Hardware
{
    /// <summary>
    /// Interaction logic for B33ScreenWpf.xaml
    /// </summary>
    public partial class B33ScreenWpf : UserControl, IB33Hardware
    {
        private readonly byte[] _screenMemory;
        private ushort _memoryLocation;
        private ushort _lastPokedLocation;
        private readonly SolidColorBrush[] _backgroundBrushesArray;
        private readonly SolidColorBrush[] _foregroundBrushesArray;

        public B33ScreenWpf()
        {
            InitializeComponent();

            _backgroundBrushesArray = new SolidColorBrush[8];
            _backgroundBrushesArray[0] = new SolidColorBrush(Colors.Black);
            _backgroundBrushesArray[1] = new SolidColorBrush(Colors.Blue);
            _backgroundBrushesArray[2] = new SolidColorBrush(Colors.Green);
            _backgroundBrushesArray[3] = new SolidColorBrush(Colors.Cyan);
            _backgroundBrushesArray[4] = new SolidColorBrush(Colors.Red);
            _backgroundBrushesArray[5] = new SolidColorBrush(Colors.Magenta);
            _backgroundBrushesArray[6] = new SolidColorBrush(Colors.Brown);
            _backgroundBrushesArray[7] = new SolidColorBrush(Colors.Gray);

            _foregroundBrushesArray = new SolidColorBrush[16];
            _foregroundBrushesArray[0] = new SolidColorBrush(Colors.Black);
            _foregroundBrushesArray[1] = new SolidColorBrush(Colors.Blue);
            _foregroundBrushesArray[2] = new SolidColorBrush(Colors.Green);
            _foregroundBrushesArray[3] = new SolidColorBrush(Colors.Cyan);
            _foregroundBrushesArray[4] = new SolidColorBrush(Colors.Red);
            _foregroundBrushesArray[5] = new SolidColorBrush(Colors.Magenta);
            _foregroundBrushesArray[6] = new SolidColorBrush(Colors.Brown);
            _foregroundBrushesArray[7] = new SolidColorBrush(Colors.LightGray);
            _foregroundBrushesArray[8] = new SolidColorBrush(Colors.LightGray);
            _foregroundBrushesArray[9] = new SolidColorBrush(Colors.LightBlue);
            _foregroundBrushesArray[10] = new SolidColorBrush(Colors.LightGreen);
            _foregroundBrushesArray[11] = new SolidColorBrush(Colors.LightCyan);
            _foregroundBrushesArray[12] = new SolidColorBrush(Colors.Pink);
            _foregroundBrushesArray[13] = new SolidColorBrush(Colors.Fuchsia);
            _foregroundBrushesArray[14] = new SolidColorBrush(Colors.Yellow);
            _foregroundBrushesArray[15] = new SolidColorBrush(Colors.White);

            FontFamily = new FontFamily("Courier New");
            FontSize = 14;
            FontStyle = FontStyles.Normal;
            FontWeight = FontWeights.Bold;
            _memoryLocation = 0xA000;
            _lastPokedLocation = 0xffff;
            
            string myString = new string('A', 80);
            Width = MeasureString(myString).Width;
            Height = MeasureString(myString).Height * 27;

            _screenMemory = new byte[RequiredMemory];

            for (int i = 0; i < RequiredMemory; i+=2)
            {
                _screenMemory[i] = 32;
                _screenMemory[i + 1] = 7;
            }

            int xLoc = 0;
            int yLoc = 0;

            for (int i = 0; i < 4000; i += 2)
            {
                if (((xLoc % 640) == 0) && (xLoc != 0))
                {
                    yLoc += 13;
                    xLoc = 0;
                }

                var tb = new TextBlock();
                tb.FontFamily = FontFamily;
                tb.FontSize = FontSize;
                tb.FontStretch = FontStretch;
                tb.FontStyle = FontStyle;
                tb.Width = 10;
                tb.Height = 13;
                tb.Text = " ";

                MainCanvas.Children.Add(tb);
                Canvas.SetLeft(tb, xLoc);
                Canvas.SetTop(tb, yLoc);
                xLoc += 8;
            }

            Reset();
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

        public ushort RequiredMemory
        {
            get { return 4001 + 3; }
        }

        private Size MeasureString(string candidate)
        {
            TextBlock tb = new TextBlock { FontStretch = FontStretches.Normal };

            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                tb.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
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

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<ushort, byte>(PokeAddress), memLoc, value);
            }
            else
            {
                PokeAddress(memLoc, value);
            }
        }

        private void PokeAddress(ushort memLoc, byte value)
        {
            lock (_screenMemory)
            {
                _screenMemory[memLoc] = value;
                _lastPokedLocation = memLoc;
            }

            UpdateText(memLoc);
        }

        private void UpdateChar(ushort loc)
        {
            int ndx = loc / 2;
            
            if (loc%2 != 0)
                loc--;

            SolidColorBrush fg = _screenMemory[loc + 1] == 1 ? new SolidColorBrush(Colors.Blue) : _foregroundBrushesArray[(byte)(_screenMemory[loc + 1] & 15)];
            SolidColorBrush bg = _backgroundBrushesArray[(byte)(_screenMemory[loc + 1] & 112) / 16];

            (MainCanvas.Children[ndx] as TextBlock).Text = Convert.ToChar(_screenMemory[loc]).ToString();
            (MainCanvas.Children[ndx] as TextBlock).Foreground = fg;
            (MainCanvas.Children[ndx] as TextBlock).Background = bg;
        }

        private void UpdateText(ushort loc)
        {
            if (loc != 0xffff)
            {
                UpdateChar(loc);
                return;
            }
            int ndx = 0;
            
            for (int i = 0; i < 4000; i += 2)
            {
                if (i == loc || i + 1 == loc || loc == 0xffff)
                {
                    lock (_screenMemory)
                    {
                        SolidColorBrush fg = _screenMemory[i + 1] == 1 ? new SolidColorBrush(Colors.Blue) : _foregroundBrushesArray[((byte)((_screenMemory[i + 1]) & 15))];
                        SolidColorBrush bg = _backgroundBrushesArray[((byte) ((_screenMemory[i + 1]) & 112))/16];

                        (MainCanvas.Children[ndx] as TextBlock).Text = Convert.ToChar(_screenMemory[i]).ToString();
                        (MainCanvas.Children[ndx] as TextBlock).Foreground = fg;
                        (MainCanvas.Children[ndx] as TextBlock).Background = bg;
                    }
                    if (loc != 0xffff)
                        break;
                }
                ndx++;
            }
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

            UpdateText(0xffff);
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
            _lastPokedLocation = 0xffff;

            UpdateText(0xffff);
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

            _lastPokedLocation = 0xffff;

            UpdateText(0xffff);
        }
    }
}
