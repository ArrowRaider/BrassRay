using System;
using System.Numerics;

namespace BrassRay.RayTracer
{
    public class Torus : Drawable
    {
        public float OuterRadius { get; set; }
        public float InnerRadius { get; set; }

        public override BoundingBox ObjectBounds => new(Vector3.Zero, (InnerRadius + OuterRadius) * 2, InnerRadius * 2,
            (InnerRadius + OuterRadius) * 2);

        protected override Intersection? IntersectCore(in Ray ray)
        {
            var g = 4 * OuterRadius * OuterRadius *
                    (ray.Direction.X * ray.Direction.X + ray.Direction.Z * ray.Direction.Z);
            var h = 8 * OuterRadius * OuterRadius *
                    (ray.Direction.X * ray.Position.X + ray.Direction.Z * ray.Position.Z);
            var i = 4 * OuterRadius * OuterRadius * (ray.Position.X * ray.Position.X + ray.Position.Z * ray.Position.Z);
            var j = ray.Direction.LengthSquared();
            var k = 2 * (Vector3.Dot(ray.Position, ray.Direction));
            var l = ray.Position.LengthSquared() + (OuterRadius * OuterRadius - InnerRadius * InnerRadius);

            // quartic terms
            var a = j * j;
            var b = 2 * k * j;
            var c = 2 * j * l + k * k - g;
            var d = 2 * k * l - h;
            var e = l * l - i;

            Span<Complex> roots = stackalloc Complex[4];
            SolveQuarticEquation(a, b, c, d, e, roots);
            Span<float> realRoots = stackalloc float[4];
            var count = 0;
            for (var m = 0; m < 4; m++)
            {
                if (MathF.Abs((float)roots[m].Imaginary) < 1.0e-2)
                    realRoots[count++] = (float)roots[m].Real;
            }
            realRoots = realRoots.Slice(0, count);
            
            if (count == 0)
                return null;
            
            realRoots.Sort();

            
            var inside = false;
            var t = realRoots[0];
            if (t < Utils.Epsilon && count > 1)
            {
                t = realRoots[1];
                inside = true;
            }
            if (t < Utils.Epsilon && count > 2)
            {
                t = realRoots[2];
                inside = false;
            }
            if (t < Utils.Epsilon && count > 3)
            {
                t = realRoots[3];
                inside = true;
            }
            if (t < Utils.Epsilon) return null;

            var p = ray.Position + ray.Direction * t;
            var alpha = OuterRadius / MathF.Sqrt(p.X * p.X + p.Z * p.Z);
            var normal = new Vector3((1 - alpha) * p.X, p.Y, (1 - alpha) * p.Z);
            return new Intersection(t, p, inside ? -normal : normal, p, inside, this);
        }

        private static void SolveQuarticEquation(Complex a, Complex b, Complex c, Complex d, Complex e, in Span<Complex> roots)
        {
            const double epsilon = 1e-9;
            b /= a;
            c /= a;
            d /= a;
            e /= a;

            var b2 = b * b;
            var b3 = b * b2;
            var b4 = b2 * b2;
            var alpha = -3.0 / 8.0 * b2 + c;
            var beta = b3 / 8.0 - b * c / 2.0 + d;
            var gamma = -3.0 / 256.0 * b4 + b2 * c / 16.0 - b * d / 4.0 + e;
            var alpha2 = alpha * alpha;
            var t = -b / 4.0;

            if (Complex.Abs(beta) < epsilon)
            {
                var rad = Complex.Sqrt(alpha2 - 4.0 * gamma);
                var r1 = Complex.Sqrt((-alpha + rad) / 2.0);
                var r2 = Complex.Sqrt((-alpha - rad) / 2.0);

                roots[0] = t + r1;
                roots[1] = t - r1;
                roots[2] = t + r2;
                roots[3] = t - r2;
            }
            else
            {
                var alpha3 = alpha * alpha2;
                var p = -(alpha2 / 12.0 + gamma);
                var q = -alpha3 / 108.0 + alpha * gamma / 3.0 - beta * beta / 8.0;
                var r = -q / 2.0 + Complex.Sqrt(q * q / 4.0 + p * p * p / 27.0);
                var u = Complex.Pow(r, 1.0 / 3.0);
                var y = (-5.0 / 6.0) * alpha + u;

                if (Complex.Abs(beta) < epsilon)
                    y -= Complex.Pow(q, 1.0 / 3.0);
                else
                    y -= p / (3.0 * u);
                var w = Complex.Sqrt(alpha + 2.0 * y);
                var r1 = Complex.Sqrt(-(3.0 * alpha + 2.0 * y + 2.0 * beta / w));
                var r2 = Complex.Sqrt(-(3.0 * alpha + 2.0 * y - 2.0 * beta / w));
                roots[0] = t + (w - r1) / 2.0;
                roots[1] = t + (w + r1) / 2.0;
                roots[2] = t + (-w - r2) / 2.0;
                roots[3] = t + (-w + r2) / 2.0;
            }
        }
    }
}
