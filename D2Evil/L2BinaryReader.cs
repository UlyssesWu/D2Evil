using System;
using System.IO;
using System.Text;
using Be.IO;

namespace D2Evil
{
    public class L2BinaryReader
    {
        private BinaryReader _reader;

        public BinaryReader BaseReader => _reader;
        public bool IsBigEndian { get; private set; } = false;
        public int FormatVersion { get; set; }
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public L2BinaryReader(Stream s, bool isBigEndian = false)
        {
            IsBigEndian = isBigEndian;
            _reader = isBigEndian ? new BeBinaryReader(s) : new BinaryReader(s);
        }

        public L2BinaryReader(Stream s, bool isBigEndian, Encoding e)
        {
            IsBigEndian = isBigEndian;
            _reader = isBigEndian ? new BeBinaryReader(s, e) : new BinaryReader(s, e);
        }

        public L2BinaryReader(Stream s, bool isBigEndian, Encoding e, bool leaveOpen)
        {
            IsBigEndian = isBigEndian;
            _reader = isBigEndian ? new BeBinaryReader(s, e, leaveOpen) : new BinaryReader(s, e, leaveOpen);
        }

        #region BinaryReader Overrides

        public int ReadInt32() => BaseReader.ReadInt32();
        public short ReadInt16() => BaseReader.ReadInt16();
        public float ReadSingle() => BaseReader.ReadSingle();
        public double ReadDouble() => BaseReader.ReadDouble();
        public byte ReadByte() => BaseReader.ReadByte();
        public bool ReadBoolean() => BaseReader.ReadBoolean();
        public Stream BaseStream => BaseReader.BaseStream;
        public int PeekChar() => BaseReader.PeekChar();
        public int ReadChar() => BaseReader.ReadChar();
        public byte[] ReadBytes(int count) => BaseReader.ReadBytes(count);

        #endregion

        public void Seek(long offset, SeekOrigin origin)
            => BaseStream.Seek(offset, origin);

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public int[] ReadIntArray()
        {
            var len = ReadNumber();
            int[] array = new int[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = _reader.ReadInt32();
            }

            return array;
        }

        public float[] ReadFloatArray()
        {
            var len = ReadNumber();
            float[] array = new float[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = _reader.ReadSingle();
            }

            return array;
        }

        public double[] ReadDoubleArray()
        {
            var len = ReadNumber();
            double[] array = new double[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = _reader.ReadSingle();
            }

            return array;
        }

        public int ReadNumber()
        {
            var b1 = _reader.ReadByte();
            if ((b1 & 0b10000000) == 0)
            {
                return b1 & 0b11111111;
            }

            var b2 = _reader.ReadByte();
            if ((b2 & 0b10000000) == 0)
            {
                return (b1 & 0b01111111) << 7 | (b2 & 0b01111111);
            }

            var b3 = _reader.ReadByte();
            if ((b3 & 0b10000000) == 0)
            {
                return (b1 & 0b01111111) << 14 | (b2 & 0b01111111) << 7 | (b3 & 0b11111111);
            }

            int b4 = _reader.ReadByte();
            if ((b4 & 0b10000000) != 0)
            {
                throw new ArgumentOutOfRangeException("", "Only support 32bit number.");
            }

            return (b1 & 0b01111111) << 21 | (b2 & 0b01111111) << 14 | (b3 & 0b01111111) << 7 | (b4 & 0b11111111);
        }

        public string ReadUTF8String()
        {
            var len = ReadNumber();
            return Encoding.UTF8.GetString(_reader.ReadBytes(len));
        }

        public long ReadLongNumber()
        {
            long result = 0;
            byte b;
            do
            {
                b = _reader.ReadByte();
                result = (result << 7) + (b & 0x7F);
            } while (b >= 0x80);

            return result;
        }
    }
}