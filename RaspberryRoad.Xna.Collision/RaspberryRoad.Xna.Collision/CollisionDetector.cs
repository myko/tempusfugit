using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaspberryRoad.Xna.Collision
{
    public struct Collision2
    {
        public float Value;
        public Line2 Line;
    }

    public static class CollisionDetector
    {
        public static bool Collides(Vector2 position, Vector2 movement, Line2 line)
        {
            return FindCollision(position, movement, line).HasValue;
        }

        public static bool Collides(Vector2 position, Vector2 movement, IEnumerable<Line2> lines)
        {
            return lines.Any(line => FindCollision(position, movement, line).HasValue);
        }

        public static float? FindCollision(Vector2 position, Vector2 movement, Line2 line)
        {
            if (Vector2.Dot(movement, line.Normal()) >= 0)
                return null;

            return SegmentsIntersection(position, position + movement, line.Start, line.End);
        }

        public static Vector2 FindMaxMovement(Vector2 position, Vector2 movement, Line2 line)
        {
            float? collision = FindCollision(position, movement, line);

            if (collision.HasValue)
                return movement * collision.Value;
            else
                return movement;
        }

        public static bool Collides(Rectangle2 rectangle, Vector2 movement, Line2 line)
        {
            return FindCollision(rectangle, movement, line).HasValue;
        }

        public static float Distance(Vector2 point, Line2 line)
        {
            var p2 = line.Start + line.Direction();

            return ((line.Start.X - point.X) * (p2.Y - point.Y) - (line.Start.Y - point.Y) * (p2.X - point.X));
        }

        public static bool InFrontOf(Vector2 point, Line2 line)
        {
            return (line.Start.X - point.X) * (line.End.Y - point.Y) - (line.Start.Y - point.Y) * (line.End.X - point.X) >= 0;
        }

        public static Vector2? FindCollision(Rectangle2 rectangle, Line2 line)
        {
            var f1 = SegmentsIntersection(rectangle.Top, line);
            var f2 = SegmentsIntersection(rectangle.Left, line);
            var f3 = SegmentsIntersection(rectangle.Right, line);
            var f4 = SegmentsIntersection(rectangle.Bottom, line);

            if (!f1.HasValue && !f2.HasValue && !f3.HasValue && !f4.HasValue)
                return null;

            var d = Math.Min(Math.Min(Distance(rectangle.TopLeft, line), Distance(rectangle.TopRight, line)), Math.Min(Distance(rectangle.BottomLeft, line), Distance(rectangle.BottomRight, line)));

            return -line.Normal() * d * 1.01f;
        }

        public static Collision2? FindCollision(Rectangle2 rectangle, Vector2 movement, Line2 line)
        {
            if (Vector2.Dot(movement, line.Normal()) >= 0)
                return null;

            var h = InFrontOf(rectangle.Center, line);
            var j = InFrontOf(rectangle.Center + movement, line);
            if (!h && !j)
                return null;

            var f1 = LineSegmentIntersection(line.Start, line.End, rectangle.TopLeft, rectangle.TopLeft + movement);
            var f2 = LineSegmentIntersection(line.Start, line.End, rectangle.TopRight, rectangle.TopRight + movement);
            var f3 = LineSegmentIntersection(line.Start, line.End, rectangle.BottomLeft, rectangle.BottomLeft + movement);
            var f4 = LineSegmentIntersection(line.Start, line.End, rectangle.BottomRight, rectangle.BottomRight + movement);

            if (!f1.HasValue && !f2.HasValue && !f3.HasValue && !f4.HasValue)
                return null;

            f1 = LinesIntersection(line.Start, line.End, rectangle.TopLeft, rectangle.TopLeft + movement);
            f2 = LinesIntersection(line.Start, line.End, rectangle.TopRight, rectangle.TopRight + movement);
            f3 = LinesIntersection(line.Start, line.End, rectangle.BottomLeft, rectangle.BottomLeft + movement);
            f4 = LinesIntersection(line.Start, line.End, rectangle.BottomRight, rectangle.BottomRight + movement);

            List<float> fs = new List<float>();

            if ((Vector2.Dot(rectangle.Top.Normal(), line.Normal()) <= 0) && (Vector2.Dot(rectangle.Left.Normal(), line.Normal()) <= 0))
                fs.Add(f1.Value);

            if ((Vector2.Dot(rectangle.Top.Normal(), line.Normal()) <= 0) && (Vector2.Dot(rectangle.Right.Normal(), line.Normal()) <= 0))
                fs.Add(f2.Value);

            if ((Vector2.Dot(rectangle.Bottom.Normal(), line.Normal()) <= 0) && (Vector2.Dot(rectangle.Left.Normal(), line.Normal()) <= 0))
                fs.Add(f3.Value);

            if ((Vector2.Dot(rectangle.Bottom.Normal(), line.Normal()) <= 0) && (Vector2.Dot(rectangle.Right.Normal(), line.Normal()) <= 0))
                fs.Add(f4.Value);

            var array = fs.OrderBy(x => x);

            if (!array.Any())
                return null;

            if (array.Last() < 0)
                return null;

            if (array.First() > 1)
                return null;

            Collision2 collision = new Collision2();
            
            collision.Value = MinLinesIntersection(rectangle, movement, line);
            collision.Line = line;

            return collision;
        }

        private static float MinLinesIntersection(Rectangle2 rectangle, Vector2 movement, Line2 line)
        {
            return Math.Min(
                Math.Min(
                    LinesIntersection(rectangle.TopLeft, rectangle.TopLeft + movement, line.Start, line.End) ?? 1f,
                    LinesIntersection(rectangle.TopRight, rectangle.TopRight + movement, line.Start, line.End) ?? 1f),
                Math.Min(
                    LinesIntersection(rectangle.BottomLeft, rectangle.BottomLeft + movement, line.Start, line.End) ?? 1f,
                    LinesIntersection(rectangle.BottomRight, rectangle.BottomRight + movement, line.Start, line.End) ?? 1f));
        }

        public static Collision2? FindCollision(Rectangle2 rectangle, Vector2 movement, IEnumerable<Line2> lines)
        {
            var collisions = lines.Select(x => FindCollision(rectangle, movement, x)).Where(x => x.HasValue);

            if (collisions.Any())
                return collisions.OrderBy(x => x.Value.Value).First();
            else
                return null;
        }

        public static Vector2 FindMaxMovement(Rectangle2 rectangle, Vector2 movement, Line2 line)
        {
            var collision = FindCollision(rectangle, movement, line);

            if (collision.HasValue)
                return movement * collision.Value.Value;
            else
                return movement;
        }

        public static Vector2 FindMaxMovement(Rectangle2 rectangle, Vector2 movement, IEnumerable<Line2> lines)
        {
            var collision = FindCollision(rectangle, movement, lines);

            if (collision.HasValue)
                return movement * collision.Value.Value;
            else
                return movement;
        }

        public static Vector2 GetSlidingVector(Vector2 movement, Vector2 lineDirection, float intersection)
        {
            float length = movement.Length() * (1 - intersection);
            return lineDirection * Vector2.Dot(Vector2.Normalize(movement), lineDirection) * length;
        }

        public static Vector2 GetStaticCollisionResolvingVector(Rectangle2 rectangle, IEnumerable<Line2> lines)
        {
            var staticCollisionResolvingVector = Vector2.Zero;
            foreach (var line in lines)
            {
                var partial = CollisionDetector.FindCollision(rectangle, line);
                if (partial.HasValue)
                    staticCollisionResolvingVector = staticCollisionResolvingVector + partial.Value;
            }
            return staticCollisionResolvingVector;
        }

        public static Vector2 GetFinalMovement(Rectangle2 rectangle, Vector2 movement, IEnumerable<Line2> lines)
        {
            float damp = 0.99f;

            var v = movement;
            var v2 = Vector2.Zero;
            var step = rectangle;

            var collision = CollisionDetector.FindCollision(step, v, lines);

            while (collision.HasValue)
            {
                if (collision.Value.Value < 0)
                    break;

                v2 += v * collision.Value.Value * damp;
                step = step + v * collision.Value.Value * damp;
                //step = step + CollisionDetector.GetStaticCollisionResolvingVector(step, lines);

                v = GetSlidingVector(v, collision.Value.Line.Direction(), collision.Value.Value) * damp;

                collision = CollisionDetector.FindCollision(step, v, lines);
            }

            return v2 + v;
        }

        private static float? SegmentsIntersection(Line2 l1, Line2 l2)
        {
            return SegmentsIntersection(l1.Start, l1.End, l2.Start, l2.End);
        }

        private static float? SegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            return SegmentsIntersection(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }

        private static float? SegmentsIntersection(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            // Denominator
            float d = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

            // Lines are parallel
            if (d == 0)
                return null;

            float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / d;
            float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / d;

            // Intersection is not within line segment a
            if (ua < 0 || ua > 1)
                return null;

            // Intersection is not within line segment b
            if (ub < 0 || ub > 1)
                return null;

            return ua;
        }

        private static float? LinesIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            return LinesIntersection(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }

        private static float? LinesIntersection(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            // Denominator
            float d = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

            // Lines are parallel
            if (d == 0)
                return null;

            float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / d;

            return ua;
        }

        private static float? LineSegmentIntersection(Vector2 l1, Vector2 l2, Vector2 s1, Vector2 s2)
        {
            return LineSegmentIntersection(l1.X, l1.Y, l2.X, l2.Y, s1.X, s1.Y, s2.X, s2.Y);
        }

        private static float? LineSegmentIntersection(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            // Denominator
            float d = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

            // Lines are parallel
            if (d == 0)
                return null;

            float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / d;
            float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / d;

            // Intersection is not within line segment b
            if (ub < 0 || ub > 1)
                return null;

            return ua;
        }
    }
}
