using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    public class RotationDeformer : DeformerData
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.RotationDeformer;

        public PivotList Pivots { get; private set; } = new PivotList();
        public List<Affine> Affines { get; private set; } = new List<Affine>();
        public override C2DeformerType DeformerType => C2DeformerType.Rotation;

        internal RotationDeformer(C2Reader br)
        {
            Read(br);
        }
        public sealed override void Read(C2Reader br)
        {
            base.Read(br);
            Pivots = br.ReadKnownObject<PivotList>();
            if (br.ReadObject() is List<object> list)
            {
                Affines = new List<Affine>(list.Cast<Affine>());
            }
            base.ReadOpacity(br);
        }
    }
}
