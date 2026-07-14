using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UndertaleModLib.Util
{
    // Initial implementation was based on DogScepter's implementation
    public class BufferBinaryReader : IBinaryReader
    {
        // A faster implementation of "MemoryStream"
        private class ChunkBuffer
        {
            private readonly byte[] _buffer;

            private int _position, _length;
            public int Position { get => _position; set => _position = value; }
            public int Length { get => _length; }
            public int Capacity { get; }

            public ChunkBuffer(int capacity)
            {
                _buffer = new byte[capacity];
                Capacity = capacity;
            }

            public int Read(byte[] buffer, int count)
            {
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }

                int n = _length - _position;
                if (n > count)
                    n = count;
                if (n <= 0)
                {
                    throw new IOException("Reading out of chunk bounds");
                }

                if (n <= 8)
                {
                    int byteCount = n;
                    while (--byteCount >= 0)
                        buffer[byteCount] = _buffer[_position + byteCount];
                }
                else
                    Buffer.BlockCopy(_buffer, _position, buffer, 0, n);
                _position += n;

                return n;
            }
            public int Read(Span<byte> buffer)
            {
                int n = Math.Min(_length - _position, buffer.Length);
                if (n <= 0)
                {
                    throw new IOException("Reading out of chunk bounds");
                }

                new Span<byte>(_buffer, _position, n).CopyTo(buffer);

                _position += n;
                return n;
            }
            public byte ReadByte()
            {
                int currPos = _position;
                int newPos = _position + 1;
                if (newPos > _length)
                {
                    throw new IOException("Reading out of chunk bounds");
                }

                _position = newPos;
                return _buffer[currPos];
            }

            public void Write(byte[] buffer, int count)
            {
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count));

                int i = _position + count;
                if (i < 0)
                    throw new IOException("Writing out of the chunk buffer bounds.");

                // "MemoryStream" also extends the buffer if 
                // the length becomes greater than the capacity
                _length = i;

                if ((count <= 8) && (buffer != _buffer))
                {
                    int byteCount = count;
                    while (--byteCount >= 0)
                    {
                        _buffer[_position + byteCount] = buffer[byteCount];
                    }
                }
                else
                {
                    Buffer.BlockCopy(buffer, 0, _buffer, _position, count);
                }

                _position = i;
            }
        }


        private readonly byte[] buffer = new byte[16];
        private readonly ChunkBuffer chunkBuffer;
        private readonly byte[] chunkCopyBuffer = new byte[81920];

        private readonly Encoding encoding = new UTF8Encoding(false);
        public Stream Stream { get; set; }

        private readonly long _length;
        public long Length { get => _length; }

        public long Position
        {
            get => chunkBuffer.Position;
            set => chunkBuffer.Position = (int)value;
        }
        public long ChunkStartPosition { get; set; }

        public BufferBinaryReader(Stream stream, Encoding encoding = null)
        {
            _length = stream.Length;
            Stream = stream;

            // Check data file length
            if (Length >= 12 * 1024 * 1024) // 12 MB
                chunkBuffer = new(12 * 1024 * 1024);
            else
                chunkBuffer = new((int)Length);

            if (stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            if (encoding is not null)
                this.encoding = encoding;
        }

        public void CopyChunkToBuffer(uint length)
        {
            Stream.Position -= 8; // Chunk name + length
            chunkBuffer.Position = 0;

            // Source - https://stackoverflow.com/a/13022108/12136394
            int read;
            int remaining = (int)length + 8;
            while (remaining > 0 &&
                   (read = Stream.Read(chunkCopyBuffer, 0, Math.Min(chunkCopyBuffer.Length, remaining))) > 0)
            {
                chunkBuffer.Write(chunkCopyBuffer, read);
                remaining -= read;
            }

            Stream.Position -= length;
            chunkBuffer.Position -= (int)length;

            ChunkStartPosition = Stream.Position;
        }
        private ReadOnlySpan<byte> ReadToBuffer(int count)
        {
            chunkBuffer.Read(buffer, count);
            return buffer;
        }

        public byte ReadByte()
        {
            return chunkBuffer.ReadByte();
        }

        public virtual bool ReadBoolean()
        {
            return ReadByte() != 0;
        }

        public string ReadChars(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (chunkBuffer.Position + count > _length)
            {
                throw new IOException("Reading out of chunk bounds");
            }

            if (count > 1024)
            {
                byte[] buf = new byte[count];
                chunkBuffer.Read(buf, count);

                return encoding.GetString(buf);
            }
            else
            {
                Span<byte> buf = stackalloc byte[count];
                if (count > 0)
                    chunkBuffer.Read(buf);

                return encoding.GetString(buf);
            }
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (chunkBuffer.Position + count > _length)
            {
                throw new IOException("Reading out of chunk bounds");
            }

            byte[] val = new byte[count];
            if (count > 0)
                chunkBuffer.Read(val, count);
            return val;
        }

        public short ReadInt16()
        {
            return BinaryPrimitives.ReadInt16LittleEndian(ReadToBuffer(2));
        }

        public ushort ReadUInt16()
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(ReadToBuffer(2));
        }

        public int ReadInt24()
        {
            ReadToBuffer(3);
            return buffer[0] | buffer[1] << 8 | (sbyte)buffer[2] << 16;
        }

        public uint ReadUInt24()
        {
            ReadToBuffer(3);
            return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16);
        }

        public int ReadInt32()
        {
            return BinaryPrimitives.ReadInt32LittleEndian(ReadToBuffer(4));
        }

        public uint ReadUInt32()
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(ReadToBuffer(4));
        }

        public float ReadSingle()
        {
            return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(ReadToBuffer(4)));
        }

        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(ReadToBuffer(8)));
        }

        public long ReadInt64()
        {
            return BinaryPrimitives.ReadInt64LittleEndian(ReadToBuffer(8));
        }

        public ulong ReadUInt64()
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(ReadToBuffer(8));
        }

        public string ReadGMString()
        {
            if (chunkBuffer.Position + 4 > _length)
                throw new IOException("Reading out of chunk bounds");

            int length = BinaryPrimitives.ReadInt32LittleEndian(ReadToBuffer(4));

            if (length < 0)
                throw new IOException("Invalid string length");
            if (chunkBuffer.Position + length > _length)
                throw new IOException("Reading out of chunk bounds");

            // Read the length-prefixed content.
            byte[] buf = length > 0 ? new byte[length] : Array.Empty<byte>();
            if (length > 0)
                chunkBuffer.Read(buf, length);

            // The GameMaker runner (and tools such as Butterscotch) treat strings as null
            // terminated and ignore the length prefix for the actual content. Some WADs
            // (e.g. WinPack / TranslaTale) store strings whose declared length prefix is
            // shorter than the null-terminated content, so extend past the declared length up
            // to the null terminator when present instead of truncating the string.
            if (chunkBuffer.Position < _length)
            {
                int b = ReadByte();
                if (b != 0)
                {
                    var extra = new List<byte>(capacity: 16);
                    while (b != 0 && chunkBuffer.Position < _length)
                    {
                        extra.Add((byte)b);
                        b = ReadByte();
                    }
                    if (extra.Count > 0)
                    {
                        var combined = new byte[buf.Length + extra.Count];
                        buf.CopyTo(combined, 0);
                        extra.CopyTo(combined, buf.Length);
                        buf = combined;
                    }
                }
            }

            return encoding.GetString(buf);
        }

        public void SkipGMString()
        {
            int length = BinaryPrimitives.ReadInt32LittleEndian(ReadToBuffer(4));
            if (length < 0)
                throw new IOException("Invalid string length");
            Position += (uint)length + 1;
        }

        public void Dispose()
        {
        }
    }
}