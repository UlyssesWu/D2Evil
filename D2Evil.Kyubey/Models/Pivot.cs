using System.Collections.Generic;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    /// <summary>
    /// ParamPivots
    /// </summary>
    public class Pivot : ICubSerializable, IStringIndexed
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.ParamPivots;

        public string ID { get; private set; }
        public L2ObjType IdType => L2ObjType.ParamID;
        public int Count => Values.Count;
        public List<float> Values { get; private set; } = new List<float>();
        internal int IndexInitVersion { get; set; } = -1;
        public int CurrentPivotIndex { get; set; }
        public float CurrentValue { get; set; }

        public float this[int i]
        {
            get => Values[i];
            set => Values[i] = value;
        }

        internal Pivot(CubReader br)
        {
            Read(br);
        }

        public void Read(CubReader br)
        {
            ID = br.ReadIdString();
            var count = br.ReadInt32();
            Values = new List<float>((float[])br.ReadObject());
        }

        public bool IsCurrentVersion(int version)
        {
            return IndexInitVersion == version;
        }
    }
}
