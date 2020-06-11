using System.Collections.Generic;
using System.Windows.Forms;

namespace B33Cpu.Hardware
{
    public class B33Keyboard : IB33Hardware
    {
        private readonly Queue<byte> _keyQueue;

        public ushort MemoryLocation { get; set; }
        public ushort RequiredMemory { get; private set; }
        public void Reset()
        {
            _keyQueue.Clear();
        }

        public B33Keyboard(Control control)
        {
            MemoryLocation = 65534; // $FFFE
            RequiredMemory = 2;
            _keyQueue = new Queue<byte>();

            control.KeyPress += KeyPressed;
        }

        public B33Keyboard(System.Windows.Window window)
        {
            MemoryLocation = 65534; // $FFFE
            RequiredMemory = 2;
            _keyQueue = new Queue<byte>();

            window.TextInput += Control_TextInput;
        }

        public B33Keyboard(System.Windows.Controls.UserControl control)
        {
            MemoryLocation = 65534; // $FFFE
            RequiredMemory = 2;
            _keyQueue = new Queue<byte>();

            control.TextInput += Control_TextInput;
        }

        private void Control_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

            _keyQueue.Enqueue((byte) e.Text[0]);
        }

        private void KeyPressed(object sender, KeyPressEventArgs e)
        {
            // Handle Arrow Keys
            // 0x01 = left arrow key
            // 0x02 = up arrow key
            // 0x03 = right arrow key
            // 0x04 = down arrow key

            _keyQueue.Enqueue((byte)e.KeyChar);
        }

        public void Poke(ushort address, byte value)
        {
            // Simulates a keystroke

            if (address != MemoryLocation && address != MemoryLocation + 1)
                return;

            if (address == MemoryLocation + 1)
                _keyQueue.Enqueue(value);
            else
                _keyQueue.Clear();
        }

        public byte Peek(ushort address, bool memoryViewer)
        {
            if ((address != (MemoryLocation + 1)) || _keyQueue.Count == 0)
                return 0;

            if (memoryViewer)
                return _keyQueue.Peek();

            return _keyQueue.Dequeue();
        }

        public byte Peek(ushort address)
        {
            return Peek(address, false);
        }
    }
}
