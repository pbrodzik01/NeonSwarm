using System;
using UnityEngine;

namespace NeonSwarm.EditorTools
{
    internal delegate float Sdf(Vector2 p);

    internal sealed class Canvas2D
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Color[] Pixels;

        public Canvas2D(int width, int height, Color? clear = null)
        {
            Width = width;
            Height = height;
            Pixels = new Color[width * height];
            var fill = clear ?? new Color(1f, 1f, 1f, 0f);
            for (var i = 0; i < Pixels.Length; i++)
            {
                Pixels[i] = fill;
            }
        }

        public void Draw(Sdf shape, Color fill, Color edge = default, float edgeWidth = 0f, Color glow = default, float glowWidth = 0f)
        {
            var pixel = 2f / Mathf.Min(Width, Height);
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var p = new Vector2((x + 0.5f) / Width * 2f - 1f, (y + 0.5f) / Height * 2f - 1f);
                    var d = shape(p);
                    var index = y * Width + x;
                    if (glowWidth > 0f && glow.a > 0f && d > 0f)
                    {
                        var g = Mathf.Exp(-d / glowWidth);
                        Blend(index, glow, glow.a * g * g);
                    }
                    if (fill.a > 0f)
                    {
                        Blend(index, fill, fill.a * Mathf.Clamp01(0.5f - d / pixel));
                    }
                    if (edgeWidth > 0f && edge.a > 0f)
                    {
                        var ring = Mathf.Abs(d + edgeWidth * 0.5f) - edgeWidth * 0.5f;
                        Blend(index, edge, edge.a * Mathf.Clamp01(0.5f - ring / pixel));
                    }
                }
            }
        }

        public void Grid(Color background, Color line, float lineWidth)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var fx = Mathf.Min(x, Width - 1 - x);
                    var fy = Mathf.Min(y, Height - 1 - y);
                    var edge = Mathf.Min(fx, fy);
                    var t = Mathf.Clamp01(1f - edge / lineWidth);
                    Pixels[y * Width + x] = Color.Lerp(background, line, t * t);
                }
            }
        }

        private void Blend(int index, Color color, float alpha)
        {
            if (alpha <= 0f)
            {
                return;
            }
            var dst = Pixels[index];
            var outA = alpha + dst.a * (1f - alpha);
            if (outA <= 0f)
            {
                return;
            }
            var r = (color.r * alpha + dst.r * dst.a * (1f - alpha)) / outA;
            var g = (color.g * alpha + dst.g * dst.a * (1f - alpha)) / outA;
            var b = (color.b * alpha + dst.b * dst.a * (1f - alpha)) / outA;
            Pixels[index] = new Color(r, g, b, outA);
        }
    }

    internal static class Shapes
    {
        public static Sdf Circle(Vector2 center, float radius)
        {
            return p => (p - center).magnitude - radius;
        }

        public static Sdf Ring(Vector2 center, float radius, float thickness)
        {
            return p => Mathf.Abs((p - center).magnitude - radius) - thickness * 0.5f;
        }

        public static Sdf Box(Vector2 center, Vector2 half, float round = 0f)
        {
            return p =>
            {
                var q = new Vector2(Mathf.Abs(p.x - center.x), Mathf.Abs(p.y - center.y)) - half + new Vector2(round, round);
                var outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude;
                return outside + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - round;
            };
        }

        public static Sdf Capsule(Vector2 a, Vector2 b, float radius)
        {
            return p =>
            {
                var pa = p - a;
                var ba = b - a;
                var h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
                return (pa - ba * h).magnitude - radius;
            };
        }

        public static Sdf Polygon(params Vector2[] v)
        {
            return p =>
            {
                var d = Vector2.Dot(p - v[0], p - v[0]);
                var s = 1f;
                for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
                {
                    var e = v[j] - v[i];
                    var w = p - v[i];
                    var b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                    d = Mathf.Min(d, Vector2.Dot(b, b));
                    var c1 = p.y >= v[i].y;
                    var c2 = p.y < v[j].y;
                    var c3 = e.x * w.y > e.y * w.x;
                    if ((c1 && c2 && c3) || (!c1 && !c2 && !c3))
                    {
                        s = -s;
                    }
                }
                return s * Mathf.Sqrt(d);
            };
        }

        public static Vector2[] Star(int points, float outer, float inner, float rotationDegrees = 90f)
        {
            var result = new Vector2[points * 2];
            for (var i = 0; i < result.Length; i++)
            {
                var angle = (rotationDegrees + i * 180f / points) * Mathf.Deg2Rad;
                var radius = i % 2 == 0 ? outer : inner;
                result[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return result;
        }

        public static Vector2[] Regular(int sides, float radius, float rotationDegrees = 90f)
        {
            var result = new Vector2[sides];
            for (var i = 0; i < sides; i++)
            {
                var angle = (rotationDegrees + i * 360f / sides) * Mathf.Deg2Rad;
                result[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return result;
        }

        public static Sdf Union(params Sdf[] shapes)
        {
            return p =>
            {
                var d = float.MaxValue;
                foreach (var shape in shapes)
                {
                    d = Math.Min(d, shape(p));
                }
                return d;
            };
        }

        public static Sdf Subtract(Sdf a, Sdf b)
        {
            return p => Mathf.Max(a(p), -b(p));
        }

        public static Sdf Inflate(Sdf shape, float amount)
        {
            return p => shape(p) - amount;
        }
    }
}
