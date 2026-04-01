using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    /// <summary>
    /// ModelImpl
    /// <para>Only Data</para>
    /// </summary>
    public class C2ModelData : IC2Serializable
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.ModelImpl;

        public ParamList Params { get; private set; }
        public List<Part> Parts { get; private set; }
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }

        public C2ModelData()
        {
        }

        internal C2ModelData(C2Reader br)
        {
            Read(br);
        }

        public void Read(C2Reader br)
        {
            Params = br.ReadKnownObject<ParamList>();
            if (br.ReadObject() is List<object> partDatas)
            {
                Parts = new List<Part>(partDatas.Cast<Part>());
            }

            CanvasWidth = br.ReadInt32();
            CanvasHeight = br.ReadInt32();
        }

        public static C2ModelData LoadFromStream(Stream input)
        {
            var sigBytes = new byte[3];
            input.Read(sigBytes, 0, 3);
            if (Encoding.ASCII.GetString(sigBytes) != "moc")
            {
                throw new BadImageFormatException("Not a valid MOC file.");
            }

            C2Reader br = new C2Reader(input);
            int version = br.ReadByte();
            if (version > C2Reader.SupportVersion)
            {
                Debug.WriteLine($"Target version {version} is unsupported.");
            }

            br.FormatVersion = version;
            return br.ReadKnownObject<C2ModelData>();
        }
    }
}