namespace D2Evil
{
    //public interface IL2Object
    //{ }

    /// <summary>
    /// Indexed by string
    /// </summary>
    public interface IStringIndexed
    {
        string ID { get; }

        //[JsonConverter(typeof(StringEnumConverter))]
        L2ObjType IdType { get; }

        string ToString();
    }

    /// <summary>
    /// Indexed by string and have a target
    /// </summary>
    public interface ITargetStringIndexed : IStringIndexed
    {
        string TargetID { get; }
    }
}
