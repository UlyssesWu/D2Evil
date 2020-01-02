namespace D2Evil.Kyubey.Models
{
    /// <summary>
    /// Component
    /// </summary>
    public enum ComponentType
    {
        Mesh = 2,
    }

    /// <summary>
    /// Component (IDrawData)
    /// </summary>
    public interface IComponentData : ICubSerializable, ITargetStringIndexed
    {
        ComponentType ComponentType { get; }
    }
}
