using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    class CurvedSurfaceDeformer : DeformerData
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.CurvedSurfaceDeformer;

        public int Row { get; set; }
        public int Column { get; set; }
        public PivotList Pivots { get; private set; } = new PivotList();
        public List<float[]> PivotPoints { get; private set; }
        public override C2DeformerType DeformerType => C2DeformerType.CurvedSurface;

        internal CurvedSurfaceDeformer(CubReader br)
        {
            Read(br);
        }

        public sealed override void Read(CubReader br)
        {
            base.Read(br);
            Row = br.ReadInt32();
            Column = br.ReadInt32();
            Pivots = br.ReadKnownObject<PivotList>();
            if (br.ReadObject() is List<object> list)
            {
                PivotPoints = new List<float[]>(list.Cast<float[]>());
            }
            base.ReadOpacity(br);
        }
    }
}
