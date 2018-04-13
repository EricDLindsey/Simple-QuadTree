using System.Collections.Generic;

namespace SimpleQT
{
	/// <summary>
	/// Represents a QuadTree node that holds a certain number of items and its four child trees.
	/// </summary>
	/// <typeparam name="T">Item type stored.</typeparam>
    public class QuadTreeNode<T>
		where T : class, IQTPoint
    {
		#region Constructors

		/// <summary>
		/// Creates a quadtree at (<paramref name="x"/>,<paramref name="y"/>)
		/// with the <paramref name="width"/> and <paramref name="height"/>.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="width">Width of bounds.</param>
		/// <param name="height">Height of bounds.</param>
		public QuadTreeNode(float x, float y, float width, float height)
			: this(new AABB(x, y, width, height)) { }

		/// <summary>
		/// Creates a quadtree with an upper left corner at <paramref name="min"/>
		/// and a lower right corner at <paramref name="max"/>.
		/// </summary>
		/// <param name="min">Upper left corner.</param>
		/// <param name="max">Lower right corner.</param>
		public QuadTreeNode(IQTPoint min, IQTPoint max)
			: this(new AABB(min, max)) { }

		/// <summary>
		/// Creates a quadtree with a boundary defined by the specified <see cref="IQTBoundary"/>.
		/// </summary>
		/// <param name="boundary">Bounding area.</param>
		public QuadTreeNode(IQTBoundary boundary)
		{
			QT_NODE_CAPACITY = 4;
			Boundary = new AABB(boundary.Min, boundary.Max);
			items = new List<T>();
		}

		#endregion

		#region Public Members

		/// <summary>
		/// The boundary of this quadtree section.
		/// </summary>
		public IQTBoundary Boundary { get; set; }

		#endregion

		#region Protected Members

		/// <summary>
		/// Max items for each section.
		/// </summary>
		protected readonly int QT_NODE_CAPACITY;

		/// <summary>
		/// Collection of local items.
		/// </summary>
		protected List<T> items;

		/// <summary>
		/// The four child trees of this section.
		/// </summary>
		protected QuadTreeNode<T> northWest;
		protected QuadTreeNode<T> northEast;
		protected QuadTreeNode<T> southWest;
		protected QuadTreeNode<T> southEast;

		#endregion

		#region Public Methods

		/// <summary>
		/// Recursively adds an item to the quadtree.
		/// </summary>
		/// <param name="item">Item to add.</param>
		/// <returns>True if successful, false if not.</returns>
		public virtual bool Add(T item)
		{
			if(!Boundary.ContainsPoint(item))
				return false;

			if(items.Count < QT_NODE_CAPACITY)
			{
				items.Add(item);
				return true;
			}

			if(northWest == null)
				subdivide();

			if(northWest.Add(item)) return true;
			if(northEast.Add(item)) return true;
			if(southWest.Add(item)) return true;
			if(southEast.Add(item)) return true;

			return false; // This shouldn't happen
		}

		/// <summary>
		/// Recursively removes an item.
		/// </summary>
		/// <param name="item">Item to be removed.</param>
		/// <returns>True if successful, false if not.</returns>
		public virtual bool Remove(T item)
		{
			if(!Boundary.ContainsPoint(item))
				return false;

			if(items.Remove(item))
				return true;

			if(northWest == null)
				return false;

			bool removed = false;
			if(northWest.Remove(item)) removed = true;
			else if(northEast.Remove(item)) removed = true;
			else if(southWest.Remove(item)) removed = true;
			else if(southEast.Remove(item)) removed = true;

			// If a change was made, check if child trees are empty, if so then remove them.
			if(removed)
			{
				clean();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Recursively removes an item that changed position.
		/// </summary>
		/// <param name="item">Item that has changed position</param>
		/// <param name="x">Old X coordinate.</param>
		/// <param name="y">Old Y coordinate.</param>
		/// <returns>Item that was removed.</returns>
		public virtual bool RemoveAt(T item, float x, float y)
		{
			if(!Boundary.ContainsPoint(new XY(x, y)))
				return false;

			if(items.Remove(item))
				return true;

			if(northWest == null)
				return false;

			bool removed = false;
			if(northWest.RemoveAt(item, x, y)) removed = true;
			else if(northEast.RemoveAt(item, x, y)) removed = true;
			else if(southWest.RemoveAt(item, x, y)) removed = true;
			else if(southEast.RemoveAt(item, x, y)) removed = true;

			// If a change was made, check if child trees are empty, if so then remove them.
			if(removed)
			{
				clean();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Removes all items.
		/// </summary>
		public virtual void Clear()
		{
			items.Clear();

			// Let the garbage collector have all the children.
			northWest = null;
			northEast = null;
			southWest = null;
			southEast = null;
		}

		/// <summary>
		/// Gets all items within the boundary.
		/// </summary>
		/// <param name="boundary">Boundary to check.</param>
		/// <returns>Collection of all items within the boundary.</returns>
		public virtual IList<T> Query(IQTBoundary boundary)
		{
			List<T> itemsInRange = new List<T>(4);

			if(Boundary == null)
				return itemsInRange;

			if(!Boundary.Intersects(boundary))
				return itemsInRange;

			foreach(T item in items)
			{
				if(boundary.ContainsPoint(item))
					itemsInRange.Add(item);
			}

			if(northWest == null)
				return itemsInRange;

			itemsInRange.AddRange(northWest.Query(boundary));
			itemsInRange.AddRange(northEast.Query(boundary));
			itemsInRange.AddRange(southWest.Query(boundary));
			itemsInRange.AddRange(southEast.Query(boundary));

			return itemsInRange;
		}

		/// <summary>
		/// Gets all bounding boxes of each section.
		/// </summary>
		/// <returns>Collection of boundaries.</returns>
		public virtual IList<IQTBoundary> GetAllBounds()
		{
			List<IQTBoundary> bounds = new List<IQTBoundary>();

			if(Boundary == null)
				return bounds;

			bounds.Add(Boundary);

			if(northWest == null)
				return bounds;

			bounds.AddRange(northWest.GetAllBounds());
			bounds.AddRange(northEast.GetAllBounds());
			bounds.AddRange(southWest.GetAllBounds());
			bounds.AddRange(southEast.GetAllBounds());

			return bounds;
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets number of items plus number of child items.
		/// </summary>
		/// <returns>Number of all items.</returns>
		protected int count()
		{
			int total = items.Count;

			if(northWest == null)
				return total;

			total += northWest.count();
			total += northEast.count();
			total += southWest.count();
			total += southEast.count();

			return total;
		}

		/// <summary>
		/// Checks if children are empty and removes them if so.
		/// </summary>
		protected void clean()
		{
			if(northWest.count() == 0 &&
				northEast.count() == 0 &&
				southWest.count() == 0 &&
				southEast.count() == 0)
			{
				// Let the garbage collector have all the children.
				northWest = null;
				northEast = null;
				southWest = null;
				southEast = null;
			}
		}

		/// <summary>
		/// Creates the four sub trees.
		/// </summary>
		protected void subdivide()
		{
			float halfX = (Boundary.Max.X - Boundary.Min.X) / 2;
			float halfY = (Boundary.Max.Y - Boundary.Min.Y) / 2;

			northWest = new QuadTreeNode<T>(new XY(Boundary.Min.X, Boundary.Min.Y),
											new XY(Boundary.Max.X - halfX, Boundary.Max.Y - halfY));

			northEast = new QuadTreeNode<T>(new XY(Boundary.Min.X + halfX, Boundary.Min.Y),
											new XY(Boundary.Max.X, Boundary.Max.Y - halfY));

			southWest = new QuadTreeNode<T>(new XY(Boundary.Min.X, Boundary.Min.Y + halfY),
											new XY(Boundary.Max.X - halfX, Boundary.Max.Y));

			southEast = new QuadTreeNode<T>(new XY(Boundary.Min.X + halfX, Boundary.Min.Y + halfY),
											new XY(Boundary.Max.X, Boundary.Max.Y));
		}

		#endregion
	}
}
