# Simple QuadTree

This is a simple to use quadtree.

## Usage

Have a class that implements the interface `IQTPoint` as the item to store in the quadtree.
Store the item in the quadtree by calling the `Add()` function and get items in an area by calling the `Query()` function.

If your item changes its X and Y position at any point, you must call the `Moved()` function to update the quadtree.

Two structs are provided to help with use of the quadtree.
`XY` is used to define a 2D point. `AABB` is used to define a 2D retangular area.

```C#
using SimpleQT;

class Item : IQTPoint
{
	public float X { get; set; }
	public float Y { get; set; }
}

QuadTree<Item> quadTree = new QuadTree<Item>(0, 0, 20, 20);

Item item1 = new Item() { X = 2, Y = 5 };
quadTree.Add(item1);

IList<Item> others = quadTree.Query(1, 3, 4, 7);

item1.X = 4;
item1.Y = 10;
quadTree.Moved(item1);
```

## Planned changes

These are changes I will make over time. Do not expect these to happen soon.

- [ ] Make thread safe
- [ ] Improve response time of Query
- [ ] Improve Moved efficiency