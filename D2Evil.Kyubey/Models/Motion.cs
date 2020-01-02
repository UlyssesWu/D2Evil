namespace D2Evil.Kyubey.Models
{
    public enum MotionType
    {
        Param = 0,
        VISIBLE = 1,
        X = 100,
        Y = 101,
        ANCHOR_X = 102,
        ANCHOR_Y = 103,
        SCALE_X = 104,
        SCALE_Y = 105,

    }

    public class Motion : IStringIndexed
    {
        public int FadeInMs { get; set; } = -1;

        public int FadeOutMs { get; set; } = -1;

        public float[] Values { get; set; }

        public string ID { get; internal set; }

        public L2ObjType IdType => L2ObjType.ParamID;

        public MotionType MotionType { get; internal set; }
    }
}
