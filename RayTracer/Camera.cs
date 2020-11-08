﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrassRay.RayTracer
{
    public abstract class Camera
    {
        public delegate void ProgressCallback(Vector3[,] shaded, int x, int y, int width, int height, int blockCount);

        public int BlockWidth { get; set; } = 32;
        public int BlockHeight { get; set; } = 32;
        public int PixelHeight { get; set; } = 200;
        public int PixelWidth => (int)MathF.Round(PixelHeight * Ratio);
        public float Ratio { get; set; } = 1.0f;

        protected abstract CoordinateSystem GetCoordinateSystem();

        protected abstract Ray GetCameraRay(Vector3 target, CoordinateSystem cs);

        public Vector3[,] Render(Scene scene, int samples, ProgressCallback callback = null)
        {
            var cs = GetCoordinateSystem();
            var xBlocks = (int)MathF.Ceiling((float)PixelWidth / BlockWidth);
            var yBlocks = (int)MathF.Ceiling((float)PixelHeight / BlockHeight);
            var blockCount = xBlocks * yBlocks;
            var shaded = new Vector3[PixelWidth, PixelHeight];
            
            Parallel.For(0, blockCount, i =>
            {
                RandomProvider.InitRandom();
                var x = i % xBlocks * BlockWidth;
                var y = i / xBlocks * BlockHeight;
                var width = Math.Min(PixelWidth, x + BlockWidth) - x;
                var height = Math.Min(PixelHeight, y + BlockHeight) - y;
                RenderBlock(shaded, x, y, width, height, cs, scene, samples);
                callback?.Invoke(shaded, x, y, width, height, blockCount);
            });

            return shaded;
        }

        private void RenderBlock(Vector3[,] shaded, int x, int y, int width, int height, CoordinateSystem cs, Scene scene, int samples)
        {
            var random = RandomProvider.Random;
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var acc = Vector3.Zero;
                    for (var k = 0; k < samples; k++)
                    {
                        var p = cs.Origin
                                - cs.U * (cs.Interval * (j + x) +
                                                        cs.Interval / 2 *
                                                        (2 * (float)random.NextDouble() - 1))
                                - cs.V * (cs.Interval * (i + y) +
                                                        cs.Interval / 2 *
                                                        (2 * (float)random.NextDouble() - 1));
                        acc += scene.Shade(GetCameraRay(p, cs)) / samples;
                    }
                    shaded[x + j, y + i] = acc;
                }
            }
        }

        protected readonly struct CoordinateSystem
        {
            public CoordinateSystem(Vector3 origin, Vector3 u, Vector3 v, float interval)
            {
                Origin = origin;
                U = u;
                V = v;
                Interval = interval;
            }

            public Vector3 Origin { get; }
            public Vector3 U { get; }
            public Vector3 V { get; }
            public float Interval { get; }
        }
    }
}
