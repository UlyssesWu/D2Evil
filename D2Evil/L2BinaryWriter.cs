using System.IO;
using System.Text;
using Be.IO;

namespace D2Evil
{
    public class L2BinaryWriter
    {
        private BinaryWriter _writer;
        public BinaryWriter BaseWriter => _writer;

        public bool IsBigEndian { get; private set; } = false;
        public int FormatVersion { get; set; }
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public L2BinaryWriter(Stream s, bool isBigEndian = false)
        {
            IsBigEndian = isBigEndian;
            _writer = isBigEndian ? new BeBinaryWriter(s) : new BinaryWriter(s);
        }

        public L2BinaryWriter(Stream s, bool isBigEndian, Encoding e)
        {
            IsBigEndian = isBigEndian;
            _writer = isBigEndian ? new BeBinaryWriter(s, e) : new BinaryWriter(s, e);
        }

        public L2BinaryWriter(Stream s, bool isBigEndian, Encoding e, bool leaveOpen)
        {
            IsBigEndian = isBigEndian;
            _writer = isBigEndian ? new BeBinaryWriter(s, e, leaveOpen) : new BinaryWriter(s, e, leaveOpen);
        }

        #region BinaryWriter Overrides

        public void Write(int val) => BaseWriter.Write(val);
        public void Write(short val) => BaseWriter.Write(val);
        public void Write(float val) => BaseWriter.Write(val);
        public void Write(double val) => BaseWriter.Write(val);
        public void Write(bool val) => BaseWriter.Write(val);
        public void Write(byte val) => BaseWriter.Write(val);
        public void Write(char val) => BaseWriter.Write(val);
        public void Write(byte[] val) => BaseWriter.Write(val);

        #endregion
    }
}