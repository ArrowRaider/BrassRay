using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public abstract class Camera
    {
        public delegate void ProgressCallback(ClampedRgb[,] shaded, int x, int y, int width, int height, int blockCount);

        public int BlockWidth { get; set; } = 32;
        public int BlockHeight { get; set; } = 32;
        public int PixelHeight { get; set; } = 200;
        public int PixelWidth => (int)MathF.Round(PixelHeight * Ratio);
        public float Ratio { get; set; } = 1.0f;

        protected abstract CoordinateSystem GetCoordinateSystem();

        protected abstract Ray GetCameraRay(Vector3 target, in CoordinateSystem cs, in Sobol.Sequence sobolSequence);

        // ReSharper disable once AccessToDisposedClosure
        public ClampedRgb[,] Render(Scene scene, int samples, ProgressCallback callback = null)
        {
            scene.Prepare();

            var cs = GetCoordinateSystem();
            var xBlocks = (int)MathF.Ceiling((float)PixelWidth / BlockWidth);
            var yBlocks = (int)MathF.Ceiling((float)PixelHeight / BlockHeight);
            var blockCount = xBlocks * yBlocks;
            var buffer = new ClampedRgb[PixelHeight, PixelWidth];

            using var sobolVectors = Sobol.GetSobolDirectionVectors(5 + 10 * samples);

            Parallel.For(0, blockCount, i =>
            {
                RandomProvider.InitRandom();
                var x = i % xBlocks * BlockWidth;
                var y = i / xBlocks * BlockHeight;
                var width = Math.Min(PixelWidth, x + BlockWidth) - x;
                var height = Math.Min(PixelHeight, y + BlockHeight) - y;
                RenderBlock(buffer, x, y, width, height, cs, scene, samples, sobolVectors);
                callback?.Invoke(buffer, x, y, width, height, blockCount);
            });

            return buffer;
        }

        private void RenderBlock(ClampedRgb[,] buffer, int x, int y, int width, int height, in CoordinateSystem cs, Scene scene, int samples, MemoryHandle sobolVectors)
        {
            var samplesPerPixel = (int)MathF.Ceiling(samples / 9.0f);
            Span<uint> sobolIndices = stackalloc uint[15];
            var blockSequence = new Sobol.Sequence(sobolVectors, 0, sobolIndices.Slice(0, 2));
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var acc = Vector3.Zero;

                    var p = Vector3.Zero;
                    for (var k = 0; k < samples; k++)
                    {
                        if (k % samplesPerPixel == 0)
                        {
                            p = cs.Origin
                                    - cs.U * (cs.Interval.X * (j + x + blockSequence.Get(0) - 0.5f))
                                    - cs.V * (cs.Interval.Y * (i + y + blockSequence.Get(1) - 0.5f));
                        }
                        for (var l = 2; l < 15; l++)
                            sobolIndices[l] = (uint)RandomProvider.Random.Next(0, 100);

                        var raySequence = new Sobol.Sequence(sobolVectors, 2, sobolIndices.Slice(2, 3));
                        var ray = GetCameraRay(p, cs, raySequence);

                        var shadeSequence = new Sobol.Sequence(sobolVectors, k * 10 + 5, sobolIndices.Slice(5, 10));
                        var state = new ShadeState(Utils.DefaultDepth, shadeSequence);
                        
                        acc += scene.Shade(ray, state) / samples;
                    }
                    buffer[y + i, x + j] = (ClampedRgb)scene.ColorModel.VectorToRgb(acc);
                }
            }
        }

        protected readonly struct CoordinateSystem : IEquatable<CoordinateSystem>
        {
            public CoordinateSystem(Vector3 origin, Vector3 u, Vector3 v, Vector2 interval)
            {
                Origin = origin;
                U = u;
                V = v;
                Interval = interval;
            }

            public Vector3 Origin { get; }
            public Vector3 U { get; }
            public Vector3 V { get; }
            public Vector2 Interval { get; }

            public bool Equals(CoordinateSystem other) => Origin.Equals(other.Origin) && U.Equals(other.U) && V.Equals(other.V) && Interval.Equals(other.Interval);

            public override bool Equals(object obj) => obj is CoordinateSystem other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Origin, U, V, Interval);

            public static bool operator ==(in CoordinateSystem left, in CoordinateSystem right) => left.Equals(right);

            public static bool operator !=(in CoordinateSystem left, in CoordinateSystem right) => !left.Equals(right);
        }
    }
}
