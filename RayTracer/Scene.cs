using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public class Scene
    {
        private const int BspDepth = 7;
        private const int MinBspLeafSize = 3;

        public List<Drawable> Drawables { get; } = new();
        public Environment Environment { get; set; }
        public Camera Camera { get; set; }
        public ColorModel ColorModel { get; set; } = new();

        // updated at Prepare(), hierarchical partitioned representation of scene drawables
        private BspBase _bsp;
        // updated at Prepare(), drawables that are not in the BSP
        private Drawable[] _nonBsp;

        public Intersection? ClosestIntersection(in Ray ray)
        {
            Intersection? nonBspMin = null;
            foreach (var drawable in _nonBsp)
            {
                var m = drawable.Intersect(ray);
                if (!nonBspMin.HasValue || m < nonBspMin)
                    nonBspMin = m;
            }

            Span<int> visited = stackalloc int[Drawables.Count - _nonBsp.Length];
            var bspMin = ClosestIntersection(ray, _bsp, visited, Drawables);
            if (nonBspMin.HasValue && bspMin.HasValue)
                return nonBspMin < bspMin ? nonBspMin : bspMin;
            return nonBspMin ?? bspMin;
        }

        public Vector3 Shade(in Ray ray, ShadeState state)
        {
            if (state.Depth <= 0)
                return Vector3.Zero;

            var m = ClosestIntersection(ray);
            return m?.Drawable.Material.Shade(ray, this, m.Value, state) ?? (state.Depth >= Utils.DefaultDepth
                ? ColorModel.InFactor * ColorModel.EnvironmentBackgroundFactor * Environment.Shade(ray)
                : Environment.Shade(ray));
        }

        /// <summary>
        /// Builds the scene BSP.  Must be called after scene graph modifications and before Shade or ClosestInterseciton.  Camera takes care of the call.
        /// </summary>
        public void Prepare()
        {
            // join the drawables and bounds as one entity
            var tuples = Drawables.Where(d => d.ObjectBounds != BoundingBox.Zero)
                .Select(d => (d, d.GetBounds())).ToArray();

            // calculate scene-wide bounding box
            var left = tuples.Select(t => t.Item2.Position.X - t.Item2.Width / 2.0f).Min();
            var right = tuples.Select(t => t.Item2.Position.X + t.Item2.Width / 2.0f).Max();
            var bottom = tuples.Select(t => t.Item2.Position.Y - t.Item2.Height / 2.0f).Min();
            var top = tuples.Select(t => t.Item2.Position.Y + t.Item2.Height / 2.0f).Max();
            var back = tuples.Select(t => t.Item2.Position.Z - t.Item2.Depth / 2.0f).Min();
            var front = tuples.Select(t => t.Item2.Position.Z + t.Item2.Depth / 2.0f).Max();
            var position = new Vector3((right + left) / 2.0f, (top + bottom) / 2.0f, (front + back) / 2.0f);
            var bounds = new BoundingBox(position, right - left, top - bottom, front - back);
            
            _bsp = BuildBsp(tuples, bounds);
            _nonBsp = Drawables.Where(d => d.ObjectBounds == BoundingBox.Zero).ToArray();
        }

        // using the BSP, find the closest intersection
        private static Intersection? ClosestIntersection(in Ray ray, BspBase bsp, in Span<int> visited,
            IList<Drawable> drawables, int visitedIndex = 0, Intersection? min = null)
        {
            switch (bsp)
            {
                case BspLeaf leaf:
                {
                    // grab a snapshot of the visited indices before appending to it
                    var visitedSlice = visited.Slice(0, visitedIndex);
                    foreach (var content in leaf.Contents)
                    {
                        // need to check if this drawable has already been intersected with this ray
                        var index = drawables.IndexOf(content);
                        if (visitedSlice.BinarySearch(index) >= 0) continue;

                        var m = content.Intersect(ray);
                        if (!min.HasValue || m < min) min = m;

                        // append this to mark that it has been intersected
                        visited[visitedIndex] = index;
                        visitedIndex++;
                    }

                    // visited indices need to be in sorted order for search to work
                    visitedSlice = visited.Slice(0, visitedIndex);
                    visitedSlice.Sort();

                    return min;
                }
                case BspTree tree:
                {
                    var leftT = BoundingBox.Intersect(tree.Left.Bounds, ray);
                    var rightT = BoundingBox.Intersect(tree.Right.Bounds, ray);
                    if (float.IsInfinity(leftT) && float.IsInfinity(rightT)) return null;
                    BspBase first;
                    BspBase second;
                    float secondT;
                    // determine the closer branch and descend through it
                    if (leftT <= rightT)
                    {
                        first = tree.Left;
                        second = tree.Right;
                        secondT = rightT;
                    }
                    else
                    {
                        first = tree.Right;
                        second = tree.Left;
                        secondT = leftT;
                    }

                    min = ClosestIntersection(ray, first, visited, drawables, visitedIndex, min);
                    // descend through the further branch (if it exists) in the following cases:
                    // the first branch resulted in no intersection
                    // there was an intersection but the actual point of the intersection is not in the first's bounding box
                    // the ray grazed the splitting plane (the T values will be very close)
                    if (!float.IsInfinity(secondT) && (!min.HasValue ||
                                                       Utils.ScalarComparer.Compare(leftT, rightT) == 0 ||
                                                       !BoundingBox.Intersect(first.Bounds, min.Value.Position)))
                        min = ClosestIntersection(ray, second, visited, drawables, visitedIndex, min);
                    return min;
                }
                default:
                    throw new InvalidOperationException();
            }
        }

        // recursively binary-divide the scene into pieces
        // this current algorithm is greedy and somewhat optimizing
        private static BspBase BuildBsp(IReadOnlyCollection<(Drawable Drawable, BoundingBox Bounds)> tuples, in BoundingBox bounds, int depth = BspDepth)
        {
            while (true)
            {
                if (depth == 0 || tuples.Count <= MinBspLeafSize)
                    return new BspLeaf { Bounds = bounds, Contents = tuples.Select(t => t.Drawable).ToList() };

                // TODO:  experiment with moving the partition plane and comparing more than 3 partitions
                // TODO:  make this not favor axis 0 over all and 1 over 2
                // partition along the 3 axes and find the lowest cost partition
                var minPartition = Partition(tuples, bounds, 0);
                var partition1 = Partition(tuples, bounds, 1);
                var partition2 = Partition(tuples, bounds, 2);
                if (partition1.Cost < minPartition.Cost)
                    minPartition = partition1;
                if (partition2.Cost < minPartition.Cost)
                    minPartition = partition2;

                var sharedCount = minPartition.LeftTuples.Intersect(minPartition.RightTuples).Count();
                // make a leaf node when there is no suitable partition:
                // the left side or right side have the same number of objects as prior to partitioning
                // the left side or right side have fewer exclusive objects than shared
                if (tuples.Count == minPartition.LeftTuples.Count ||
                    tuples.Count == minPartition.RightTuples.Count ||
                    sharedCount > minPartition.LeftTuples.Except(minPartition.RightTuples).Count() ||
                    sharedCount > minPartition.RightTuples.Except(minPartition.LeftTuples).Count())
                {
                    depth = 0;
                    continue;
                }

                var left = BuildBsp(minPartition.LeftTuples, minPartition.LeftBounds, depth - 1);
                var right = BuildBsp(minPartition.RightTuples, minPartition.RightBounds, depth - 1);
                return new BspTree { Bounds = bounds, Left = left, Right = right };
            }
        }
        
        // binary partition tuples and bounds at the specified axis
        private static BinaryPartition Partition(IReadOnlyCollection<(Drawable Drawable, BoundingBox Bounds)> tuples,
            in BoundingBox bounds, int axis)
        {
            var mid = axis switch
            {
                0 => tuples.Select(t => t.Bounds.Position.X).Average(),
                1 => tuples.Select(t => t.Bounds.Position.Y).Average(),
                2 => tuples.Select(t => t.Bounds.Position.Z).Average(),
                _ => throw new InvalidOperationException()
            };
            var (leftBounds, rightBounds) = DivideBounds(bounds, axis, mid);
            var leftTuples = tuples.Where(t => BoundingBox.Intersect(leftBounds, t.Bounds)).ToArray();
            var rightTuples = tuples.Where(t => BoundingBox.Intersect(rightBounds, t.Bounds)).ToArray();
            var cost = Math.Abs(leftTuples.Length - rightTuples.Length) + leftTuples.Intersect(rightTuples).Count();
            return new BinaryPartition(leftTuples, rightTuples, leftBounds, rightBounds, cost);
        }

        // split bounds at the specified mid of the specified axis
        private static (BoundingBox Left, BoundingBox Right) DivideBounds(in BoundingBox bounds, int axis, float mid)
        {
            BoundingBox left, right;
            float start, end;
            switch (axis)
            {
                case 0:
                    start = bounds.Position.X - bounds.Width / 2.0f;
                    end = bounds.Position.X + bounds.Width / 2.0f;
                    left = new BoundingBox(new Vector3((mid + start) / 2.0f, bounds.Position.Y, bounds.Position.Z),
                        mid - start, bounds.Height, bounds.Depth);
                    right = new BoundingBox(new Vector3((end + mid) / 2.0f, bounds.Position.Y, bounds.Position.Z),
                        end - mid, bounds.Height, bounds.Depth);
                    break;
                case 1:
                    start = bounds.Position.Y - bounds.Height / 2.0f;
                    end = bounds.Position.Y + bounds.Height / 2.0f;
                    left = new BoundingBox(new Vector3(bounds.Position.X, (mid + start) / 2.0f, bounds.Position.Z),
                        bounds.Width, mid - start, bounds.Depth);
                    right = new BoundingBox(new Vector3(bounds.Position.X, (end + mid) / 2.0f, bounds.Position.Z),
                        bounds.Width, end - mid, bounds.Depth);
                    break;
                case 2:
                    start = bounds.Position.Z - bounds.Depth / 2.0f;
                    end = bounds.Position.Z + bounds.Depth / 2.0f;
                    left = new BoundingBox(new Vector3(bounds.Position.X, bounds.Position.Y, (mid + start) / 2.0f),
                        bounds.Width, bounds.Height, mid - start);
                    right = new BoundingBox(new Vector3(bounds.Position.X, bounds.Position.Y, (end + mid) / 2.0f),
                        bounds.Width, bounds.Height, end - mid);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return (left, right);
        }

        private readonly ref struct BinaryPartition
        {
            public BinaryPartition(IReadOnlyCollection<(Drawable Drawable, BoundingBox Bounds)> leftTuples,
                IReadOnlyCollection<(Drawable Drawable, BoundingBox Bounds)> rightTuples, BoundingBox leftBounds,
                BoundingBox rightBounds, int cost)
            {
                LeftTuples = leftTuples;
                RightTuples = rightTuples;
                LeftBounds = leftBounds;
                RightBounds = rightBounds;
                Cost = cost;
            }

            public IReadOnlyCollection<(Drawable Drawable, BoundingBox Bounds)> LeftTuples { get; }
            public IReadOnlyCollection<(Drawable Drawable, BoundingBox Bounds)> RightTuples { get; }
            public BoundingBox LeftBounds { get; }
            public BoundingBox RightBounds { get; }
            public int Cost { get; }
        }

        private record BspBase
        {
            public BoundingBox Bounds { get; init; }
        }

        private sealed record BspTree : BspBase
        {
            public BspBase Left { get; init; }
            public BspBase Right { get; init; }
        }

        private sealed record BspLeaf : BspBase
        {
            public IReadOnlyList<Drawable> Contents { get; init; }
        }
    }
}
