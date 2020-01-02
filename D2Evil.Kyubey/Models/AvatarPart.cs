using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    internal class AvatarTextureInfo
    {
        public int TextureIndex = -1;

        public float ScaleWidth = 1f;

        public float ScaleHeight = 1f;

        public int ColorGroupNo = -1;
    }

    /// <summary>
    /// Paper Doll
    /// </summary>
    class AvatarPart : ICubSerializable, IStringIndexed
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.AvatarPartsItem;

        public string ID { get; private set; }
        public L2ObjType IdType => L2ObjType.PartsDataID;
        public int PartNo { get; set; } = -1;
        public List<AvatarTextureInfo> TextureInfos { get; private set; }
        public int TextureCount => TextureInfos?.Count ?? 0;
        public List<IComponentData> Components { get; private set; }
        public List<DeformerData> Deformers { get; private set; }

        internal AvatarPart(CubReader br)
        {
            Read(br);
        }

        public void Read(CubReader br)
        {
            ID = br.ReadIdString();
            if (br.ReadObject() is List<object> drawDatas)
            {
                Components = new List<IComponentData>(drawDatas.Cast<IComponentData>());
            }

            if (br.ReadObject() is List<object> deformers)
            {
                Deformers = new List<DeformerData>(deformers.Cast<DeformerData>());
            }

        }

        public static AvatarPart Load(Stream input)
        {
            return null; //TODO:
        }
    }
}
