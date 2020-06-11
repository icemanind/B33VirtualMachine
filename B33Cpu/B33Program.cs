using System.Collections.Generic;

namespace B33Cpu
{
    public class B33Program
    {
        public ushort StartAddress { get; set; }
        public ushort EndAddress { get; set; }
        public ushort ExecAddress { get; set; }
        public ushort DebugInfoAddress { get; set; }
        public byte[] Program { get; set; }
        public bool DualMonitorRequired { get; set; }
        public bool HasDebugInfo { get; set; }
        public string FileName { get; set; }
        public List<DebugData> DebugData { get; set; }

        public B33Program()
        {
            StartAddress = 0;
            EndAddress = 0;
            ExecAddress = 0;
            DebugInfoAddress = 0;
            Program = new byte[B33Cpu.MemorySize];
            DualMonitorRequired = false;
            HasDebugInfo = false;
            FileName = "";
            DebugData = new List<DebugData>();

            ClearProgram();
        }

        private void ClearProgram()
        {
            for (int i = 0; i < B33Cpu.MemorySize; i++)
            {
                Program[i] = 0;
            }
        }

        public void Reset()
        {
            StartAddress = 0;
            EndAddress = 0;
            ExecAddress = 0;
            DebugInfoAddress = 0;
            DualMonitorRequired = false;
            HasDebugInfo = false;
            FileName = "";

            ClearProgram();
        }
    }
}
