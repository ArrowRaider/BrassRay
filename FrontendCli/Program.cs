﻿using BrassRay.RayTracer.IO;
using CommandLine;
using System;
using System.IO;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using BrassRay.RayTracer;

namespace BrassRay.Frontend.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                // TODO:  Refactor everything here and split into functions

                using var reader = File.OpenText(o.ScenePath);
                var yaml = reader.ReadToEnd();
                var scene = Serialization.ReadScene(yaml);
                
                if (o.Height.HasValue) scene.Camera.PixelHeight = o.Height.Value;
                if (o.Ratio.HasValue) scene.Camera.Ratio = o.Ratio.Value;
                if (o.Width.HasValue) scene.Camera.PixelWidth = o.Width.Value;

                var done = 0;
                var shaded = scene.Camera.Render(scene, o.Samples, (_, __, ___, ____, _____, count) =>
                {
                    Interlocked.Increment(ref done);
                    var p = Volatile.Read(ref done) / (float)count;
                    Console.Write($"{p:P1}\r");
                });
                using var bitmap = new Image<Bgr24>(scene.Camera.PixelWidth, scene.Camera.PixelHeight);
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var pixelRowSpan = bitmap.GetPixelRowSpan(y);
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        var s = shaded[x, y];
                        var c = (ClampedRgb)s;
                        pixelRowSpan[x] = new Bgr24(c.R, c.G, c.B);
                    }
                }
                bitmap.Save(o.OutputPath);
                Console.WriteLine("done!   ");
            });
        }

        public class Options
        {
            [Value(0, MetaName = "input file", HelpText = "Input scene graph yaml file", Required = true)]
            public string ScenePath { get; set; }

            [Value(1, MetaName = "output file", HelpText = "Output image file", Required = true)]
            public string OutputPath { get; set; }

            [Option('s', "samples", Default = 8, HelpText = "Samples per pixel")]
            public int Samples { get; set; }

            [Option('w', "width", Default = null, HelpText = "Override output image width")]
            public int? Width { get; set; }

            [Option('h', "height", Default = null, HelpText = "Override output image height")]
            public int? Height { get; set; }

            [Option('r', "ratio", Default = null, HelpText = "Override output image aspect ratio")]
            public float? Ratio { get; set; }
        }
    }
}
