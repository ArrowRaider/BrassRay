#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace BrassRay.RayTracer.IO
{
    internal class Vector3Converter : IYamlTypeConverter
    {
        private static readonly Regex Exp = new Regex(@"^vec\s*\[\s*(\S+),\s*(\S+),\s*(\S+)\s*\]$");

        public bool Accepts(Type type)
        {
            return type == typeof(Vector3) || type == typeof(Vector3?);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            Match match;
            if (!parser.Accept<Scalar>(out var scalar) ||
                !(match = Exp.Match(scalar.Value)).Success ||
                !float.TryParse(match.Groups[1].Value, out var x) ||
                !float.TryParse(match.Groups[2].Value, out var y) ||
                !float.TryParse(match.Groups[3].Value, out var z))
                throw new InvalidDataException();
            parser.MoveNext();
            return new Vector3(x, y, z);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value is not Vector3 v) return;
            emitter.Emit(new SequenceStart(null, "vec", false, SequenceStyle.Flow));
            emitter.Emit(new Scalar($"{v.X:G3}"));
            emitter.Emit(new Scalar($"{v.Y:G3}"));
            emitter.Emit(new Scalar($"{v.Z:G3}"));
            emitter.Emit(new SequenceEnd());
        }
    }

    internal class RgbConverter : IYamlTypeConverter
    {
        private static readonly Regex Exp = new Regex(@"^rgb\s*\[\s*(\S+),\s*(\S+),\s*(\S+)\s*\]$");

        public bool Accepts(Type type)
        {
            return type == typeof(Rgb) || type == typeof(Rgb?);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            Match match;
            if (!parser.Accept<Scalar>(out var scalar) ||
                !(match = Exp.Match(scalar.Value)).Success ||
                !float.TryParse(match.Groups[1].Value, out var r) ||
                !float.TryParse(match.Groups[2].Value, out var g) ||
                !float.TryParse(match.Groups[3].Value, out var b))
                throw new InvalidDataException();
            parser.MoveNext();
            return new Rgb(r, g, b);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value is not Rgb v) return;
            emitter.Emit(new SequenceStart(null, "rgb", false, SequenceStyle.Flow));
            emitter.Emit(new Scalar($"{v.R:G3}"));
            emitter.Emit(new Scalar($"{v.G:G3}"));
            emitter.Emit(new Scalar($"{v.B:G3}"));
            emitter.Emit(new SequenceEnd());
        }
    }
}
