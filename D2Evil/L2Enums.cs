// ReSharper disable InconsistentNaming

namespace D2Evil
{
    public enum L2SdkFormat : byte
    {
        V2_1 = 0,
        V3_0 = 1,
        V3_1 = 2,
    }

    public enum L2CoordType
    {
        BasicCoord,
        DeformerLocal,
        Canvas,
    }

    public enum L2ColorCompositionType
    {
        Normal = 0,

        /// <summary>
        /// Add (Screen)
        /// </summary>
        Add = 1,

        /// <summary>
        /// Multiply
        /// </summary>
        Multiply = 2,
    }

    public enum L2ObjType
    {
        Unknown = -1,
        Null = 0,
        String = 1,
        Color = 10,
        RectD = 11,
        RectF = 12,
        PointD = 13,
        PointF = 14,
        ObjectArray = 15,
        IntArray = 16,
        IntArray2 = 25,
        Matrix2x3 = 17,
        Rect = 21,
        Point = 22,
        Array = 23,
        DoubleArray = 26,
        FloatArray = 27,

        /// <summary>
        /// Object Reference
        /// </summary>
        ObjectRef = 33,
        DrawDataID = 50,
        BaseDataID = 51,
        ParamID = 60,
        PartsDataID = 134,
        ParamDefF = 131,
        PartsData = 133,
        ModelImpl = 136,
        ParamDefList = 137,
        AvatarPartsItem = 142,
        DDTexture = 70,
        Affine = 69,
        RotationDeformer = 68,
        ParamPivots = 67,
        PivotManager = 66,
        CurvedSurfaceDeformer = 65,
    }
}