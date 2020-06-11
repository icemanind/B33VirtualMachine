namespace B33Cpu.Hardware
{
    /// <summary>
    /// Interface IB33Hardware. All B33 hardware devices must implement this interface
    /// </summary>
    public interface IB33Hardware
    {
        /// <summary>
        /// Stores a value at the specified address
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="value">The value to store.</param>
        void Poke(ushort address, byte value);
        /// <summary>
        /// Returns the value the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>System.Byte.</returns>
        byte Peek(ushort address);
        /// <summary>
        /// Returns the value of the specified address.
        /// </summary>
        /// <param name="address">The Address.</param>
        /// <param name="memoryViewer">True if the memory viewer is peeking.</param>
        /// <returns>System.Byte.</returns>
        byte Peek(ushort address, bool memoryViewer);
        
        /// <summary>
        /// Gets or sets the start of the memory location that this hardware will use.
        /// </summary>
        /// <value>The memory location that this hardware will use.</value>
        ushort MemoryLocation { get; set; }
        /// <summary>
        /// Gets the the number of bytes this hardware uses in memory.
        /// </summary>
        /// <example>
        /// If MemoryLocation = $E000 and RequiredMemory = 5, then this hardware will use memory address $E000, $E001, $E002, $E003, and $E004
        /// </example>
        /// <value>The number of bytes this hardware uses in memory.</value>
        ushort RequiredMemory { get; }
        void Reset();
    }
}
