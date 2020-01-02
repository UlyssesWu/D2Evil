using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    /// <summary>
    /// ParamDefF
    /// </summary>
    public class Param : ICubSerializable, IStringIndexed
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.ParamDefF;

        public float MinValue { get; private set; }

        public float MaxValue { get; private set; }

        public float DefaultValue { get; private set; }

        public string ID { get; private set; }

        public L2ObjType IdType => L2ObjType.ParamID;

        public Param()
        {
        }

        public Param(string pid, float min, float max, float defaultValue)
        {
            this.ID = pid;
            this.MinValue = min;
            this.MaxValue = max;
            this.DefaultValue = defaultValue;
        }

        internal Param(CubReader br)
        {
            Read(br);
        }

        public void Read(CubReader br)
        {
            MinValue = br.ReadSingle();
            MaxValue = br.ReadSingle();
            DefaultValue = br.ReadSingle();
            ID = br.ReadIdString();
        }
    }
}
