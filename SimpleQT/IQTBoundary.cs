namespace SimpleQT
{
	/// <summary>
	/// Defines properties and methods for an area that the <see cref="QuadTree{T}"/> can interact with.
	/// </summary>
    public interface IQTBoundary
    {
		IQTPoint Min { get; }
		IQTPoint Max { get; }

		bool ContainsPoint(IQTPoint point);
		bool Intersects(IQTBoundary other);
    }
}
