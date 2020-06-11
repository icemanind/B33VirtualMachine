namespace B33Cpu.Hardware
{
    /// <summary>
    /// Interface IB33Mouse. Any B33 screens that can support a mouse needs to implement this interface
    /// </summary>
    public interface IB33Mouse
    {
        int MaxWidthResolution { get; }
        int MaxHeightResolution { get; }
        void ShowCursor();
        void HideCursor();
    }
}
