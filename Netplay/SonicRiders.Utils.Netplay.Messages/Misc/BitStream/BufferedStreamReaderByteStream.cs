using System.IO;
using Reloaded.Memory.Streams;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Misc.BitStream
{
    public struct BufferedStreamReaderByteStream : IByteStream
    {
        public BufferedStreamReader Reader { get; private set; }
        public BufferedStreamReaderByteStream(BufferedStreamReader reader) => Reader = reader;

        /// <inheritdoc />
        public byte Read(int index)
        {
            Reader.Seek(index, SeekOrigin.Begin);
            return Reader.Read<byte>();
        }

        /// <inheritdoc />
        public void Write(byte value, int index) => throw new System.NotImplementedException();
    }
}