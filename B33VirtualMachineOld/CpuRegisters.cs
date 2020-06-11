namespace B33VirtualMachine
{
    public class CpuRegisters
    {
        private byte _a;
        private byte _b;
        private ushort _d;

        public byte A
        {
            get { return _a; }
            set
            {
                _a = value;
                Normalize(1);
            }
        }

        public byte B
        {
            get { return _b; }
            set
            {
                _b = value;
                Normalize(2);
            }
        }

        public ushort D
        {
            get { return _d; }
            set
            {
                _d = value;
                Normalize(3);
            }
        }

        public ushort X
        {
            get;
            set;
        }

        public ushort Y
        {
            get;
            set;
        }

        public ushort Pc
        {
            get;
            set;
        }

        public byte Cc
        {
            get;
            set;
        }

        private void Normalize(int reg)
        {
            switch (reg)
            {
                case 1:
                case 2:
                    _d = (ushort)((_a << 8) + _b);
                    break;
                default:
                    _a = (byte) (_d >> 8);
                    _b = (byte) (_d & 0xff);
                    break;
            }
        }
    }
}
