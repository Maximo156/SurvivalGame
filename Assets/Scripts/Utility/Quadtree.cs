using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//https://github.com/BadEcho/core/blob/master/src/Game/Quadtree.cs

public interface Trackable
{
    public abstract Vector2 Location();
}

/// <summary>
/// Provides a tree data structure in which each internal (non-leaf) node has exactly four children, useful
/// for 2D spatial information queries.
/// </summary>
public sealed class Quadtree <T> where T : Trackable
{
    private readonly List<T> _elements = new List<T>();

    private readonly int _bucketCapacity;
    private readonly int _maxDepth;

    private Quadtree<T> _topLeft, _topRight, _bottomLeft, _bottomRight;

    /// <summary>
    /// Initializes a new instance of the <see cref="Quadtree"/> class.
    /// </summary>
    /// <param name="Rect">The bounding rectangle of the region that this quadtree's contents occupy.</param>
    /// <remarks>
    /// This initializes the <see cref="Quadtree"/> instance with a default bucket capacity of <c>32</c> and a default maximum
    /// node depth of <c>5</c>, which are reasonable general-purpose values for these settings.
    /// </remarks>
    public Quadtree(Rect Rect)
        : this(Rect, 32, 5)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Quadtree"/> class.
    /// </summary>
    /// <param name="Rect">The bounding rectangle of the region that this quadtree's contents occupy.</param>
    /// <param name="bucketCapacity">The maximum number of items allowed in this node before it gets quartered.</param>
    /// <param name="maxDepth">The maximum number of levels of child nodes allowed across the entire quad tree.</param>
    public Quadtree(Rect Rect, int bucketCapacity, int maxDepth)
    {
        _bucketCapacity = bucketCapacity;
        _maxDepth = maxDepth;

        this.Rect = Rect;
    }

    /// <summary>
    /// Gets the bounding rectangle of the region that this quadtree's contents occupy.
    /// </summary>
    public Rect Rect
    { get; }

    /// <summary>
    /// Gets the level of this node within the overall quadtree; that is, the number of edges on the path from this node to the
    /// root node.
    /// </summary>
    public int Level
    { get; set; }

    /// <summary>
    /// Gets a value indicating if this node is a terminal node for the tree (i.e., it has no children).
    /// </summary>
    /// <remarks>
    /// The majority of spatial elements reside in leaf nodes, the only exceptions being elements whose Rect overlap multiple
    /// split node boundaries.
    /// </remarks>
    public bool IsLeaf
        => _topLeft == null || _topRight == null || _bottomLeft == null || _bottomRight == null;

    /// <summary>
    /// Inserts the specified spatial element into the quadtree at the appropriate node.
    /// </summary>
    /// <param name="element">The spatial element to insert into the quadtree.</param>
    public void Insert(T element, bool debug = false)
    {
        if (!Rect.Contains(element.Location()))
            throw new ArgumentException("Element not inside quadtree Rect");

        // A node exceeding its allotted number of items will get split (if it hasn't been already) into four equal quadrants.
        if (_elements.Count >= _bucketCapacity)
            Split();

        Quadtree<T> containingChild = GetContainingChild(element.Location());

        if (containingChild != null)
        {
            containingChild.Insert(element, debug);
        }
        else
        {   // If no child was returned, then this is either a leaf node, or the element's boundaries overlap multiple children.
            // Either way, the element gets assigned to this node.
            _elements.Add(element);
            if(debug)
                Debug.Log("adding element at: " + element.Location());
        }
    }

    /// <summary>
    /// Removes the specified spatial element from the quadtree and whichever node it's been assigned to.
    /// </summary>
    /// <param name="element">The spatial element to remove from the quadtree.</param>
    /// <returns>True if <c>element</c> is successfully removed; otherwise, false.</returns>
    public bool Remove(Vector2 location)
    {
        Quadtree<T> containingChild = GetContainingChild(location);

        // If no child was returned, then this is the leaf node (or potentially non-leaf node, if the element's boundaries overlap
        // multiple children) containing the element.

        bool removed = containingChild?.Remove(location) ?? _elements.RemoveAll(e => e.Location() == location) > 0;

        // If the total descendant element count is less than the bucket capacity, we ensure the node is in a non-split state.
        if (removed && CountElements() <= _bucketCapacity)
            Merge();

        return removed;
    }

    public bool Remove(T element)
    {
        Quadtree<T> containingChild = GetContainingChild(element.Location());

        // If no child was returned, then this is the leaf node (or potentially non-leaf node, if the element's boundaries overlap
        // multiple children) containing the element.

        bool removed = containingChild?.Remove(element) ?? _elements.Remove(element);

        // If the total descendant element count is less than the bucket capacity, we ensure the node is in a non-split state.
        if (removed && CountElements() <= _bucketCapacity)
            Merge();

        return removed;
    }

    /// <summary>
    /// Looks for and returns all spatial elements that exist within this node and its children whose bounds collide with
    /// the specified spatial element.
    /// </summary>
    /// <param name="element">The spatial element to find collisions for.</param>
    /// <returns>All spatial elements that collide with <c>element</c>.</returns>
    public HashSet<T> FindClosest(Vector2 pos, float Distance)
    {
        var playerBounds = new Rect(pos - Vector2.one * Distance, Vector2.one * Distance * 2);
        var nodes = new Queue<Quadtree<T>>();
        var collisions = new HashSet<T>();

        nodes.Enqueue(this);

        while (nodes.Count > 0)
        {
            var node = nodes.Dequeue();

            if (!node.Rect.Overlaps(playerBounds))
                continue;

            foreach(var elem in node._elements.Where(e => Vector2.Distance(pos, e.Location()) < Distance))
            {
                collisions.Add(elem);
            }

            if (!node.IsLeaf)
            {
                if (playerBounds.Overlaps(node._topLeft.Rect))
                    nodes.Enqueue(node._topLeft);

                if (playerBounds.Overlaps(node._topRight.Rect))
                    nodes.Enqueue(node._topRight);

                if (playerBounds.Overlaps(node._bottomLeft.Rect))
                    nodes.Enqueue(node._bottomLeft);

                if (playerBounds.Overlaps(node._bottomRight.Rect))
                    nodes.Enqueue(node._bottomRight);
            }
        }
        return collisions;
    }

    /// <summary>
    /// Gets the total number of elements belonging to this and all descending nodes.
    /// </summary>
    /// <returns>The total number of elements belong to this and all descending nodes.</returns>
    public int CountElements()
    {
        int count = _elements.Count;

        if (!IsLeaf)
        {
            count += _topLeft.CountElements();
            count += _topRight.CountElements();
            count += _bottomLeft.CountElements();
            count += _bottomRight.CountElements();
        }

        return count;
    }

    /// <summary>
    /// Retrieves the elements belonging to this and all descendant nodes.
    /// </summary>
    /// <returns>A sequence of the elements belonging to this and all descendant nodes.</returns>
    public IEnumerable<T> GetElements()
    {
        var children = new List<T>();
        var nodes = new Queue<Quadtree<T>>();

        nodes.Enqueue(this);

        while (nodes.Count > 0)
        {
            var node = nodes.Dequeue();

            if (!node.IsLeaf)
            {
                nodes.Enqueue(node._topLeft);
                nodes.Enqueue(node._topRight);
                nodes.Enqueue(node._bottomLeft);
                nodes.Enqueue(node._bottomRight);
            }

            children.AddRange(node._elements);
        }

        return children;
    }

    private void Split()
    {   // If we're not a leaf node, then we're already split.
        if (!IsLeaf)
            return;

        // Splitting is only allowed if it doesn't cause us to exceed our maximum depth.
        if (Level + 1 > _maxDepth)
            return;

        _topLeft = CreateChild(Rect.min);
        _topRight = CreateChild(new Vector2(Rect.center.x, Rect.min.y));
        _bottomLeft = CreateChild(new Vector2(Rect.min.x, Rect.center.y));
        _bottomRight = CreateChild(new Vector2(Rect.center.x, Rect.center.y));

        var elements = _elements.ToList();

        foreach (var element in elements)
        {
            Quadtree<T> containingChild = GetContainingChild(element.Location());
            // An element is only moved if it completely fits into a child quadrant.
            if (containingChild != null)
            {
                _elements.Remove(element);

                containingChild.Insert(element);
            }
        }
    }
   
    private Quadtree<T> CreateChild(Vector2 location)
    => new Quadtree<T>(new Rect(location, new Vector2(Rect.size.x/2, Rect.size.y/2)), _bucketCapacity, _maxDepth)
    {
        Level = Level + 1
    };

    private void Merge()
    {   // If we're a leaf node, then there is nothing to merge.
        if (IsLeaf)
            return;

        _elements.AddRange(_topLeft._elements);
        _elements.AddRange(_topRight._elements);
        _elements.AddRange(_bottomLeft._elements);
        _elements.AddRange(_bottomRight._elements);

        _topLeft = _topRight = _bottomLeft = _bottomRight = null;
    }

    private Quadtree<T> GetContainingChild(Vector2 Rect)
    {
        if (IsLeaf)
            return null;

        if (_topLeft.Rect.Contains(Rect))
            return _topLeft;

        if (_topRight.Rect.Contains(Rect))
            return _topRight;

        if (_bottomLeft.Rect.Contains(Rect))
            return _bottomLeft;

        return _bottomRight.Rect.Contains(Rect) ? _bottomRight : null;
    }

    public void Display()
    {
        Gizmos.DrawWireCube(new Vector3(Rect.center.x, 0, Rect.center.y) + Vector3.up*100, new Vector3(Rect.size.x, 0, Rect.size.y));
        _topLeft?.Display();
        _topRight?.Display();
        _bottomLeft?.Display();
        _bottomRight?.Display();
    }
}
