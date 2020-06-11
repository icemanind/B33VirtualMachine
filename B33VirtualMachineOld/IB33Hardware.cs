namespace B33VirtualMachine
{
    public interface IB33Hardware
    {
        void ForceRefresh();
        void Poke(ushort address, byte value);
        byte Peek(ushort address);
        ushort MemoryLocation { get; set; }
        ushort RequiredMemory { get; }
        bool ThreadInvokeRequired { get; }
        void Reset();
    }
}
