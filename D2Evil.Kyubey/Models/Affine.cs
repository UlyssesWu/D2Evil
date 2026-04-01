using FreeLive;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    /// <summary>
    /// Rotation Desc
    /// </summary>
    public class Affine : IC2Serializable
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.Affine;

        public float OriginX { get; set; } = 0f;

        public float OriginY { get; set; } = 0f;

        public float ScaleX { get; set; } = 1f;

        public float ScaleY { get; set; } = 1f;

        /// <summary>
        /// Rotate Degree
        /// </summary>
        public float Rotation { get; set; } = 0f;

        public bool ReflectX { get; set; } = false;

        public bool ReflectY { get; set; } = false;

        public Affine() { }

        public Affine(Affine a)
        {
            CopyFrom(a);
        }

        public void CopyFrom(Affine a)
        {
            OriginX = a.OriginX;
            OriginY = a.OriginY;
            ScaleX = a.ScaleX;
            ScaleY = a.ScaleY;
            Rotation = a.Rotation;
            ReflectX = a.ReflectX;
            ReflectY = a.ReflectY;
        }

        internal Affine(C2Reader br)
        {
            Read(br);
        }

        public void Read(C2Reader br)
        {
            OriginX = br.ReadSingle();
            OriginY = br.ReadSingle();
            ScaleX = br.ReadSingle();
            ScaleY = br.ReadSingle();
            Rotation = br.ReadSingle();
            if (br.FormatVersion >= 10)
            {
                ReflectX = br.ReadBoolean();
                ReflectY = br.ReadBoolean();
            }

        }

        //TODO: ToAffineTransform
    }
}
