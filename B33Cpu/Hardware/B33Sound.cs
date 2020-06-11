using System;
using System.IO;
using System.Threading;

namespace B33Cpu.Hardware
{
    public class B33Sound : IB33Hardware
    {
        private bool _soundPlaying;
        private readonly byte[] _memory;

        public B33Sound()
        {
            // $FE00 - Set to Frequency
            // $FE02 - Set to Duration
            // $FE04 - Poke 1 here to start playing. Check $FE04. It changes to 0 when the sound is done playing.

            MemoryLocation = 65024; // $FE00
            RequiredMemory = 5;
            _soundPlaying = false;

            _memory = new byte[6];
        }

        public void Poke(ushort address, byte value)
        {
            if (((address - MemoryLocation) < 0) || (address - MemoryLocation) > RequiredMemory)
                return;

            if (address == (MemoryLocation + RequiredMemory - 1))
            {
                lock (_memory)
                {
                    ushort freq = BitConverter.ToUInt16(new[] {_memory[0], _memory[1]}, 0);
                    ushort duration = BitConverter.ToUInt16(new[] {_memory[2], _memory[3]}, 0);
                    var t = new Thread(() => PlayTone(freq, duration, Done));
                    _memory[4] = 1;
                    _soundPlaying = true;
                    t.Start();
                }
                return;
            }

            lock (_memory)
            {
                _memory[address - MemoryLocation] = value;
            }
        }

        private void Done(object sender, EventArgs e)
        {
            lock (_memory)
            {
                _memory[4] = 0;
            }
            
            _soundPlaying = false;
        }

        public byte Peek(ushort address, bool memoryViewer)
        {
            if (((address - MemoryLocation) < 0) || (address - MemoryLocation) > RequiredMemory)
                return 0;

            var addr = (ushort) (address - MemoryLocation);
            if (address == (MemoryLocation + RequiredMemory - 1))
            {
                return (byte) (_soundPlaying ? 1 : 0);
            }

            return _memory[addr];
        }

        public byte Peek(ushort address)
        {
            return Peek(address, false);
        }

        public ushort MemoryLocation { get; set; }
        public ushort RequiredMemory { get; private set; }
        public void Reset()
        {
            
        }

        private void PlayTone(UInt16 frequency, int msDuration, EventHandler doneCallback = null, UInt16 volume = 16383)
        {
            using (var mStrm = new MemoryStream())
            {
                using (var writer = new BinaryWriter(mStrm))
                {
                    const double tau = 2 * Math.PI;
                    const int formatChunkSize = 16;
                    const int headerSize = 8;
                    const short formatType = 1;
                    const short tracks = 1;
                    const int samplesPerSecond = 44100;
                    const short bitsPerSample = 16;
                    const short frameSize = (short)(tracks * ((bitsPerSample + 7) / 8));
                    const int bytesPerSecond = samplesPerSecond * frameSize;
                    const int waveSize = 4;
                    var samples = (int)((decimal)samplesPerSecond * msDuration / 1000);
                    int dataChunkSize = samples * frameSize;
                    int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;

                    writer.Write(0x46464952);
                    writer.Write(fileSize);
                    writer.Write(0x45564157);
                    writer.Write(0x20746D66);
                    writer.Write(formatChunkSize);
                    writer.Write(formatType);
                    writer.Write(tracks);
                    writer.Write(samplesPerSecond);
                    writer.Write(bytesPerSecond);
                    writer.Write(frameSize);
                    writer.Write(bitsPerSample);
                    writer.Write(0x61746164);
                    writer.Write(dataChunkSize);

                    double theta = frequency * tau / samplesPerSecond;
                    double amp = volume >> 2;
                    for (int step = 0; step < samples; step++)
                    {
                        writer.Write((short)(amp * Math.Sin(theta * step)));
                    }

                    mStrm.Seek(0, SeekOrigin.Begin);
                    using (var player = new System.Media.SoundPlayer(mStrm))
                    {
                        player.PlaySync();
                    }
                    if (doneCallback != null)
                    {
                        doneCallback(null, new EventArgs());
                    }
                }
            }
        }
    }
}
