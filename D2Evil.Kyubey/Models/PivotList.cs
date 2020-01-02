using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    /// <summary>
    /// Pivot List (Data)
    /// </summary>
    public class PivotList : List<Pivot>, ICubSerializable
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.PivotManager;

        public PivotList()
        { }

        internal PivotList(CubReader br)
        {
            Read(br);
        }

        public void Read(CubReader br)
        {
            Clear();
            if (br.ReadObject() is List<object> list)
            {
                AddRange(list.Cast<Pivot>());
            }
        }
    }
}
