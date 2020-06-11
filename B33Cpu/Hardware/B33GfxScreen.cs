using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace B33Cpu.Hardware
{
    public partial class B33GfxScreen : UserControl, IB33Hardware
    {
        public ushort MemoryLocation { get; set; }
        public ushort RequiredMemory { get; private set; }

        private readonly byte[] _gfxMemory;
        private Bitmap _bitmap;
        private readonly Point _zeroPoint;

        private delegate void PokeCallBack(ushort addr, byte value);

        public B33GfxScreen()
        {
            InitializeComponent();

            _zeroPoint = new Point(0, 0);
            MemoryLocation = 0x3200;
            RequiredMemory = 51020;
            _gfxMemory = new byte[51000];
            _bitmap = new Bitmap(255, 200);
            Graphics.FromImage(_bitmap).Clear(Color.FromArgb(255, 255, 255));

            Reset();
        }

        public void Poke(ushort address, byte value)
        {
            ushort memLoc;

            try
            {
                memLoc = (ushort)(address - MemoryLocation);
            }
            catch (Exception)
            {
                return;
            }

            if (memLoc >= 51000)
                return;

            if (InvokeRequired)
            {
                var pcb = new PokeCallBack(PokeAddress);
                Invoke(pcb, new object[] { memLoc, value });
            }
            else
            {
                PokeAddress(address, value);
            }
        }

        private void PokeAddress(ushort memLoc, byte value)
        {
            lock (_gfxMemory)
            {
                _gfxMemory[memLoc] = value;
            }

            using (Graphics gfx = Graphics.FromImage(_bitmap))
            {
                var r = (byte)(value >> 4);
                var g = (byte)((value >> 2) & 0x3);
                var b = (byte)(value & 0x3);

                Color c = Color.FromArgb(255, r*85, g*85, b*85);
                int y = memLoc/255;
                int x = memLoc%255;

                _bitmap.SetPixel(x, y, c);
            }

            Refresh();
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
                memLoc = (ushort)(address - MemoryLocation);
            }
            catch (Exception)
            {
                return 0;
            }

            if (memLoc >= 51000)
                return 0;

            return _gfxMemory[memLoc];
        }
        
        public void Reset()
        {
            for (int i = 0; i < 51000; i++)
            {
                _gfxMemory[i] = 0xff;
            }

            Refresh();
        }

        private void B33GfxScreen_SizeChanged(object sender, EventArgs e)
        {
            Width = 255;
            Height = 200;
        }

        private void B33GfxScreen_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_bitmap, _zeroPoint);
        }
    }
}
