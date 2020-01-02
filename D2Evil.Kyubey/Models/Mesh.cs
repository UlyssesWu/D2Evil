using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace D2Evil.Kyubey.Models
{
    internal class UvInfo
    {
        public bool Enabled;

        public int ConvertedTextureNo = -1;

        public float OffsetX;

        public float OffsetY;

        public float ScaleX = 1f;

        public float ScaleY = 1f;

        /// <summary>
        /// XY换位
        /// </summary>
        public bool Transposition;
    }

    class Mesh : DrawableComponent
    {
        [JsonProperty] public const L2ObjType Type = L2ObjType.DDTexture;
        public override ComponentType ComponentType => ComponentType.Mesh;

        public int TextureNo { get; set; } = -1;
        public float[] UvMap { get; private set; }
        public int OptionFlag { get; private set; } = 0;
        public int PointCount { get; private set; }
        public int PolygonCount { get; private set; }

        public Dictionary<string, object> OptionData { get; private set; } = new Dictionary<string, object>();

        public ushort[] IndexArray { get; private set; }

        public List<float[]> PivotPoints { get; private set; }

        public UvInfo UvMapInfo = new UvInfo();

        /// <summary>
        /// <see cref="FreeLive.Kyubey.Models.ColorCompositionType"/>
        /// </summary>
        public int ColorCompositionType { get; private set; }

        public bool Culling { get; set; } = true;

        public int ColorGroupNo { get; private set; } = -1;

        internal Mesh(CubReader br)
        {
            Read(br);
        }

        public sealed override void Read(CubReader br)
        {
            base.Read(br);
            TextureNo = br.ReadInt32();
            PointCount = br.ReadInt32();
            PolygonCount = br.ReadInt32();
            int[] array = (int[])br.ReadObject();
            IndexArray = new ushort[PolygonCount * 3];
            for (int i = PolygonCount * 3 - 1; i >= 0; i--)
            {
                IndexArray[i] = (ushort)array[i];
            }

            if (br.ReadObject() is List<object> list)
            {
                PivotPoints = new List<float[]>(list.Cast<float[]>());
            }

            UvMap = (float[])br.ReadObject();

            if (br.FormatVersion >= 8)
            {
                OptionFlag = br.ReadInt32();
                if (OptionFlag != 0)
                {
                    if ((OptionFlag & 1) != 0)
                    {
                        int num = br.ReadInt32();
                        ColorGroupNo = num;
                        OptionData.Add(L2Consts.BK_OPTION_COLOR, num);
                    }

                    ColorCompositionType = (OptionFlag & 30) != 0 ? (OptionFlag & 30) >> 1 : 0;

                    if ((OptionFlag & 32) != 0)
                    {
                        Culling = false;
                    }
                }
            }
            else
            {
                OptionFlag = 0;
            }
        }
    }
}
