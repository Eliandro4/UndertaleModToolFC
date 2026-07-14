using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UndertaleModLib.Util
{
    // Reimplemented based on DogScepter's implementation
    public class FileBinaryReader : IBinaryReader
    {
        private readonly byte[] buffer = new byte[16];

        private readonly Encoding encoding = new UTF8Encoding(false);
        public Stream Stream { get; set; }

        private readonly long _length;
        public long Length { get => _length; }

        public long Position
        {
            get => Stream.Position;
            set
            {
                if (value > Length)
                    throw new IOException("Reading out of bounds.");
                Stream.Position = value;
            }
        }

        public FileBinaryReader(Stream stream, Encoding encoding = null)
        {
            _length = stream.Length;
            Stream = stream;

            if (stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            if (encoding is not null)
                this.encoding = encoding;
        }

        private ReadOnlySpan<byte> ReadToBuffer(int count)
        {
            Stream.ReadExactly(buffer, 0, count);
            return buffer;
        }

        public byte ReadByte()
        {
            if (Stream.Position + 1 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            return (byte)Stream.ReadByte();
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
            if (Stream.Position + count > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            if (count > 1024)
            {
                byte[] buf = new byte[count];
                Stream.ReadExactly(buf, 0, count);

                return encoding.GetString(buf);
            }
            else
            {
                Span<byte> buf = stackalloc byte[count];
                Stream.ReadExactly(buf);

                return encoding.GetString(buf);
            }
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (Stream.Position + count > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            byte[] val = new byte[count];
            Stream.ReadExactly(val, 0, count);
            return val;
        }

        public short ReadInt16()
        {
            if (Stream.Position + 2 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            return BinaryPrimitives.ReadInt16LittleEndian(ReadToBuffer(2));
        }

        public ushort ReadUInt16()
        {
            if (Stream.Position + 2 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            return BinaryPrimitives.ReadUInt16LittleEndian(ReadToBuffer(2));
        }

        public int ReadInt24()
        {
            if (Stream.Position + 3 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            ReadToBuffer(3);
            return buffer[0] | buffer[1] << 8 | (sbyte)buffer[2] << 16;
        }

        public uint ReadUInt24()
        {
            if (Stream.Position + 3 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            ReadToBuffer(3);
            return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16);
        }

        public int ReadInt32()
        {
            if (Stream.Position + 4 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            return BinaryPrimitives.ReadInt32LittleEndian(ReadToBuffer(4));
        }

        public uint ReadUInt32()
        {
            if (Stream.Position + 4 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            return BinaryPrimitives.ReadUInt32LittleEndian(ReadToBuffer(4));
        }

        public float ReadSingle()
        {
            if (Stream.Position + 4 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(ReadToBuffer(4)));
        }

        public double ReadDouble()
        {
            if (Stream.Position + 8 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(ReadToBuffer(8)));
        }

        public long ReadInt64()
        {
            if (Stream.Position + 8 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            return BinaryPrimitives.ReadInt64LittleEndian(ReadToBuffer(8));
        }

        public ulong ReadUInt64()
        {
            if (Stream.Position + 8 > _length)
            {
                throw new IOException("Reading out of bounds");
            }

            return BinaryPrimitives.ReadUInt64LittleEndian(ReadToBuffer(8));
        }

        public string ReadGMString()
        {
            if (Stream.Position + 4 > _length)
                throw new IOException("Reading out of bounds");

            int length = BinaryPrimitives.ReadInt32LittleEndian(ReadToBuffer(4));

            if (length < 0)
                throw new IOException("Invalid string length");
            if (Stream.Position + length > _length)
                throw new IOException("Reading out of bounds");

            // Read the length-prefixed content.
            byte[] buf = length > 0 ? new byte[length] : Array.Empty<byte>();
            if (length > 0)
                Stream.ReadExactly(buf, 0, length);

            // The GameMaker runner (and tools such as Butterscotch) treat strings as null
            // terminated and ignore the length prefix for the actual content. Some WADs
            // (e.g. WinPack / TranslaTale) store strings whose declared length prefix is
            // shorter than the null-terminated content, so extend past the declared length up
            // to the null terminator when present instead of truncating the string.
            if (Stream.Position < _length)
            {
                int b = Stream.ReadByte();
                if (b != 0)
                {
                    var extra = new List<byte>(capacity: 16);
                    while (b != 0 && b != -1 && Stream.Position < _length)
                    {
                        extra.Add((byte)b);
                        b = Stream.ReadByte();
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
            if (Stream?.CanRead == true)
            {
                Stream.Close();
                Stream.Dispose();
            }
        }
    }
}