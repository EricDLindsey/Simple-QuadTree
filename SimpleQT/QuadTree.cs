using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SimpleQT
{
	/// <summary>
	/// Represents a quadtree.
	/// </summary>
	/// <typeparam name="T">Item type stored.</typeparam>
    public class QuadTree<T> : ICollection<T>, IEnumerable<T>
		where T : class, IQTPoint
    {
		#region Constructors

		/// <summary>
		/// Creates a <see cref="QuadTree{T}"/> at (<paramref name="x"/>,<paramref name="y"/>)
		/// with the <paramref name="width"/> and <paramref name="height"/>.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="width">Width of bounds.</param>
		/// <param name="height">Height of bounds.</param>
		public QuadTree(float x, float y, float width, float height)
			: this(new AABB(x, y, width, height)) { }

		/// <summary>
		/// Creates a <see cref="QuadTree{T}"/> with an upper left corner at <paramref name="min"/>
		/// and a lower right corner at <paramref name="max"/>.
		/// </summary>
		/// <param name="min">Upper left corner.</param>
		/// <param name="max">Lower right corner.</param>
		public QuadTree(IQTPoint min, IQTPoint max)
			: this(new AABB(min, max)) { }

		/// <summary>
		/// Creates a <see cref="QuadTree{T}"/> with a boundary defined by the specified <see cref="IQTBoundary"/>.
		/// </summary>
		/// <param name="boundary">Bounding area.</param>
		public QuadTree(IQTBoundary boundary)
		{
			rootTree = new QuadTreeNode<T>(boundary);
			itemDictionary = new Dictionary<T, XY>(new ObjectReferenceEqualityComparer<T>());
		}

		#endregion

		#region Public Members

		/// <summary>
		/// Returns the <see cref="IQTBoundary"/> of the <see cref="QuadTree{T}"/>.
		/// </summary>
		public IQTBoundary Boundary { get { return rootTree.Boundary; } }

		/// <summary>
		/// Returns the number of items plus all child items.
		/// </summary>
		public int Count { get { return itemDictionary.Count; } }

		/// <summary>
		/// Returns wether the <see cref="ICollection{T}"/> is read-only.
		/// </summary>
		public bool IsReadOnly { get { return false; } }

		#endregion

		#region Protected Members

		/// <summary>
		/// Collection of all items. Used to find old positions of moving items.
		/// </summary>
		protected Dictionary<T, XY> itemDictionary;

		/// <summary>
		/// The root <see cref="QuadTreeNode{T}"/>.
		/// </summary>
		protected QuadTreeNode<T> rootTree;

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds an item to the <see cref="QuadTree{T}"/>. No duplicates.
		/// </summary>
		/// <param name="item">Item to add.</param>
		public virtual void Add(T item)
		{
			if(itemDictionary.ContainsKey(item))
				return;

			if(rootTree.Add(item))
				itemDictionary.Add(item, new XY(item.X, item.Y));
		}

		/// <summary>
		/// Adds multiple items at once. No duplicates.
		/// </summary>
		/// <param name="items">Collection of items to add.</param>
		public virtual void AddRange(IEnumerable<T> items)
		{
			foreach(T i in items)
				Add(i);
		}

		/// <summary>
		/// Copies all items in the <see cref="QuadTree{T}"/> into the <paramref name="array"/>,
		/// starting at the beginning of the target array.
		/// </summary>
		/// <param name="array">Array that is the destination of the items in the quadtree.</param>
		public virtual void CopyTo(T[] array)
		{
			itemDictionary.Keys.CopyTo(array, 0);
		}

		/// <summary>
		/// Copies all items in the <see cref="QuadTree{T}"/> into the <paramref name="array"/>,
		/// starting at the specified <paramref name="index"/> of the target <paramref name="array"/>.
		/// </summary>
		/// <param name="array">Array that is the destination of the items in the quadtree.</param>
		/// <param name="index">Index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(T[] array, int index)
		{
			itemDictionary.Keys.CopyTo(array, index);
		}

		/// <summary>
		/// Remove an item from the <see cref="QuadTree{T}"/>.
		/// </summary>
		/// <param name="item">Item to add.</param>
		/// <returns>True if successful, false if not.</returns>
		public virtual bool Remove(T item)
		{
			if(!itemDictionary.ContainsKey(item))
				return false;

			if(rootTree.Remove(item))
			{
				itemDictionary.Remove(item);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Removes all items from <see cref="QuadTree{T}"/>.
		/// </summary>
		public virtual void Clear()
		{
			rootTree.Clear();
			itemDictionary.Clear();
		}

		/// <summary>
		/// Checks if item is stored in <see cref="QuadTree{T}"/>.
		/// </summary>
		/// <param name="item">Item to check.</param>
		/// <returns>True is contained, false if not.</returns>
		public virtual bool Contains(T item)
		{
			return itemDictionary.ContainsKey(item);
		}

		/// <summary>
		/// Gets all items in the specified area.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="width">Width of boundary.</param>
		/// <param name="height">Height of boundary.</param>
		/// <returns></returns>
		public virtual IList<T> Query(float x, float y, float width, float height)
		{
			return Query(new AABB(x, y, width, height));
		}

		/// <summary>
		/// Gets all items between min and max.
		/// </summary>
		/// <param name="min">Upper left corner of boundary.</param>
		/// <param name="max">Lower right corner of boundary.</param>
		/// <returns>Collection of all items between min and max.</returns>
		public virtual IList<T> Query(IQTPoint min, IQTPoint max)
		{
			return Query(new AABB(min, max));
		}

		/// <summary>
		/// Gets all items within <see cref="IQTBoundary"/>.
		/// </summary>
		/// <param name="boundary">Boundary to check.</param>
		/// <returns>Collection of all items within <see cref="IQTBoundary"/>.</returns>
		public virtual IList<T> Query(IQTBoundary boundary)
		{
			return rootTree.Query(boundary);
		}

		/// <summary>
		/// Gets all items in <see cref="QuadTree{T}"/>.
		/// </summary>
		/// <returns>List of all items.</returns>
		public virtual List<T> ToList()
		{
			return new List<T>(itemDictionary.Keys);
		}

		/// <summary>
		/// Gets all bounding boxes of each section.
		/// </summary>
		/// <returns>Collection of all boundaries.</returns>
		public virtual IList<IQTBoundary> GetAllBounds()
		{
			return rootTree.GetAllBounds();
		}

		/// <summary>
		/// Updates the position of an item after it has been moved.
		/// Make sure to call this for all moving items before calling <see cref="Query(IQTBoundary)"/>.
		/// </summary>
		/// <param name="item">Item that had moved.</param>
		/// <returns>True if successful, false if not.</returns>
		public virtual bool Moved(T item)
		{
			if(!itemDictionary.ContainsKey(item))
				return false;

			XY qtp = itemDictionary[item];

			// Simply just remove the old one and add it again in the right place.
			// Could be more efficient for small movements if it just "goes up a level" in
			// the QuadTreeNode tree until it reaches a boundary it intersects.
			// Makes no difference in efficiency for large movements.
			if(removeAt(item, qtp.X, qtp.Y))
			{
				Add(item);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Updates the position of multiple items after they have been moved.
		/// Make sure to call this for all moving items before calling <see cref="Query(IQTBoundary)"/>.
		/// </summary>
		/// <param name="items"></param>
		public virtual void Moved(IEnumerable<T> items)
		{
			foreach(T i in items)
				Moved(i);
		}

		/// <summary>
		/// Updates the position for every item that has moved in <see cref="QuadTree{T}"/>.
		/// This method has an efficiency of O(n). It is strongly encouraged to call
		/// <see cref="Moved(T)"/> or <see cref="Moved(IEnumerable{T})"/> for any moving items instead.
		/// </summary>
		public virtual void UpdateAll()
		{
			foreach(T i in itemDictionary.Keys)
			{
				XY qtp = itemDictionary[i];
				if(i.X != qtp.X | i.Y != qtp.Y)
					Moved(i);
			}
		}

		#endregion

		#region Enumerators

		/// <summary>
		/// Gets an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return itemDictionary.Keys.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Equality Comparer

		/// <summary>
		/// A generic object comparer that would only use the objects reference.
		/// </summary>
		protected class ObjectReferenceEqualityComparer<E> : EqualityComparer<E>
			where E : class
		{
			private static IEqualityComparer<E> _defaultComparer;

			public new static IEqualityComparer<E> Default
			{
				get { return _defaultComparer ?? (_defaultComparer = new ObjectReferenceEqualityComparer<E>()); }
			}

			#region IEqualityComparer<T> Members

			public override bool Equals(E x, E y)
			{
				return ReferenceEquals(x, y);
			}

			public override int GetHashCode(E obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}

			#endregion
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Removes an item from <see cref="QuadTree{T}"/> at a specific location.
		/// </summary>
		/// <param name="item">Item that has moved.</param>
		/// <param name="x">Old X coordinate.</param>
		/// <param name="y">Old Y coordinate.</param>
		/// <returns>True if successful, false if not.</returns>
		protected bool removeAt(T item, float x, float y)
		{
			if(rootTree.RemoveAt(item, x, y))
			{
				itemDictionary.Remove(item);
				return true;
			}

			return false;
		}

		#endregion
	}
}
