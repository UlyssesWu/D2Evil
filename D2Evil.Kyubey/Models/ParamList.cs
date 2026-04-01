using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    /// <summary>
    /// ParameterSamples
    /// </summary>
    public class ParamList : List<Param>, IC2Serializable
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.ParamDefList;

        public ParamList()
        { }

        internal ParamList(C2Reader br)
        {
            Read(br);
        }

        public void Read(C2Reader br)
        {
            Clear();
            if (br.ReadObject() is List<object> list)
            {
                AddRange(list.Cast<Param>());
            }
        }
    }
}
