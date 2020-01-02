using System;
using System.Collections.Generic;
using System.Linq;

namespace D2Evil.Kyubey.Models
{
    public abstract class DrawableComponent : IComponentData
    {
        private static int TotalMinOrder = 500;
        private static int TotalMaxOrder = 500;
        public string ID { get; private set; }
        public L2ObjType IdType => L2ObjType.DrawDataID;
        public string TargetID { get; set; }
        public virtual ComponentType ComponentType { get; }
        public bool NeedTransform => TargetID != null && TargetID != L2Consts.DST_BASE_ID;

        public PivotList Pivots { get; private set; }
        public int AverageDrawOrder { get; set; }
        public int[] PivotDrawOrder { get; set; }
        public float[] PivotOpacity { get; set; }
        public List<string> ClipIDs { get; set; }
        public virtual void Read(CubReader br)
        {
            ID = br.ReadIdString();
            TargetID = br.ReadIdString();
            Pivots = br.ReadKnownObject<PivotList>();
            AverageDrawOrder = br.ReadInt32();
            PivotDrawOrder = br.ReadIntArray();
            PivotOpacity = br.ReadFloatArray();
            if (br.FormatVersion >= 11)
            {
                var drawDataId = br.ReadIdString();
                if (string.IsNullOrEmpty(drawDataId))
                {
                    ClipIDs = null;
                }
                else
                {
                    if (drawDataId.Contains(","))
                    {
                        ClipIDs = new List<string>(drawDataId.Split(new[] { "," },
                            StringSplitOptions.RemoveEmptyEntries));
                    }
                    else
                    {
                        ClipIDs = new List<string> { drawDataId };
                    }
                }
            }
            else
            {
                ClipIDs = null;
            }

            var minPivot = PivotDrawOrder.Min();
            if (minPivot < TotalMinOrder)
            {
                TotalMinOrder = minPivot;
            }

            var maxPivot = PivotDrawOrder.Max();
            if (maxPivot > TotalMaxOrder)
            {
                TotalMaxOrder = maxPivot;
            }
        }
    }
}
