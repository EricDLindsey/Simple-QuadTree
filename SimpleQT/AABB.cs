namespace SimpleQT
{
	/// <summary>
	/// Simple (x,y) structure for defining 2D positions.
	/// </summary>
	public struct XY : IQTPoint
	{
		public XY(float x, float y)
		{
			X = x;
			Y = y;
		}

		public float X { get; set; }
		public float Y { get; set; }
	}

	/// <summary>
	/// An axis-aligned minimum bounding box that represents a 2D area.
	/// </summary>
	public struct AABB : IQTBoundary
    {
		#region Constructors

		/// <summary>
		/// Creates boundary at (<paramref name="x"/>,<paramref name="y"/>)
		/// with the <paramref name="width"/> and <paramref name="height"/>.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="width">Width of boundary.</param>
		/// <param name="height">Height of boundary.</param>
		public AABB(float x, float y, float width, float height)
		{
			Min = new XY(x, y);
			Max = new XY(x + width, y + height);
		}

		/// <summary>
		/// Sets the minimum and maximum corners with points.
		/// </summary>
		/// <param name="min">Upper left corner.</param>
		/// <param name="max">Lower right corner.</param>
		public AABB(IQTPoint min, IQTPoint max)
		{
			Min = new XY(min.X, min.Y);
			Max = new XY(max.X, max.Y);
		}

		#endregion

		#region Public Members

		/// <summary>
		/// Upper left point of the boundary.
		/// </summary>
		public IQTPoint Min { get; private set; }

		/// <summary>
		/// Lower right point of the boundary.
		/// </summary>
		public IQTPoint Max { get; private set; }

		/// <summary>
		/// Returns the center of the boundary.
		/// </summary>
		public IQTPoint Center { get { return new XY((Max.X + Min.X) / 2, (Max.Y + Min.Y) / 2); } }

		#endregion

		#region Public Methods

		/// <summary>
		/// Checks if a point is within the boundary.
		/// </summary>
		/// <param name="point">Point to check.</param>
		/// <returns>True if contained, false if not.</returns>
		public bool ContainsPoint(IQTPoint point)
		{
			return (point.X >= Min.X &&
					point.X <= Max.X &&
					point.Y >= Min.Y &&
					point.Y <= Max.Y);
		}

		/// <summary>
		/// Checks if another boundary intersects this one.
		/// </summary>
		/// <param name="other">Boundary to check.</param>
		/// <returns>True if intersects, false if not.</returns>
		public bool Intersects(IQTBoundary other)
		{
			if(other.Max.X < Min.X) return false;
			if(other.Min.X > Max.X) return false;
			if(other.Max.Y < Min.Y) return false;
			if(other.Min.Y > Max.Y) return false;

			return true;
		}

		#endregion
	}
}