using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    /// <summary>
    /// PartsData
    /// </summary>
    public class Part : ICubSerializable, IStringIndexed
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.PartsData;

        public bool Visible { get; set; } = true;

        public bool Locked { get; set; }

        public string ID { get; set; }

        public L2ObjType IdType => L2ObjType.PartsDataID;

        /// <summary>
        /// BaseDataList
        /// </summary>
        public List<DeformerData> Deformers { get; private set; }

        /// <summary>
        /// DrawDataList
        /// </summary>
        public List<IComponentData> Components { get; private set; }

        internal Part(CubReader br)
        {
            Read(br);
        }

        public void Read(CubReader br)
        {
            var state = br.Read8Bit();
            Locked = state[0] == '1';
            Visible = state[1] == '1';
            ID = br.ReadIdString();

            List<object> list = br.ReadObject() as List<object>;
            Deformers = list == null ? new List<DeformerData>() : new List<DeformerData>(list.Cast<DeformerData>());

            list = br.ReadObject() as List<object>;
            Components = list == null ? new List<IComponentData>() : new List<IComponentData>(list.Cast<IComponentData>());
        }
    }
}
