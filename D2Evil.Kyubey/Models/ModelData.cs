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
    public class ModelData : ICubSerializable
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.ModelImpl;

        public ParamList Params { get; private set; }
        public List<Part> Parts { get; private set; }
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }

        public ModelData()
        {
        }

        internal ModelData(CubReader br)
        {
            Read(br);
        }

        public void Read(CubReader br)
        {
            Params = br.ReadKnownObject<ParamList>();
            if (br.ReadObject() is List<object> partDatas)
            {
                Parts = new List<Part>(partDatas.Cast<Part>());
            }

            CanvasWidth = br.ReadInt32();
            CanvasHeight = br.ReadInt32();
        }

        public static ModelData LoadFromStream(Stream input)
        {
            var sigBytes = new byte[3];
            input.Read(sigBytes, 0, 3);
            if (Encoding.ASCII.GetString(sigBytes) != "moc")
            {
                throw new BadImageFormatException("Not a valid MOC file.");
            }

            CubReader br = new CubReader(input);
            int version = br.ReadByte();
            if (version > CubReader.SupportVersion)
            {
                Debug.WriteLine($"Target version {version} is unsupported.");
            }

            br.FormatVersion = version;
            return br.ReadKnownObject<ModelData>();
        }
    }
}