using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    /// <summary>
    /// Pivot List (Data)
    /// </summary>
    public class PivotList : List<Pivot>, IC2Serializable
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.PivotManager;

        public PivotList()
        { }

        internal PivotList(C2Reader br)
        {
            Read(br);
        }

        public void Read(C2Reader br)
        {
            Clear();
            if (br.ReadObject() is List<object> list)
            {
                AddRange(list.Cast<Pivot>());
            }
        }
    }
}
