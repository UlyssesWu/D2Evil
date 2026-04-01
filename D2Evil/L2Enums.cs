// ReSharper disable InconsistentNaming

namespace D2Evil
{
    public enum L2SdkFormat : byte
    {
        V2_1 = 0,
        V3_0 = 1,
        /// <summary>
        /// 3.03
        /// </summary>
        V3_03 = 2,
        V4_0 = 3,
        V4_02 = 4,
        V5_0 = 5,
    }

    public enum L2CoordType
    {
        BasicCoord,
        DeformerLocal,
        Canvas,
    }

    public enum L2ParameterType
    {
        Normal,
        MorphTarget
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

    public enum L2TextureState
    {
        MODEL_IMAGE = 0,
        TEXTURE_ATLAS = 1,
        NO_TEXTURE_ATLAS = 2,
        NO_TEXTURE_INPUT_EXTENSION_ERROR = 3,
        NO_MODEL_IMAGE_ERROR = 4,
        UNKNOWN = 5
    }
}