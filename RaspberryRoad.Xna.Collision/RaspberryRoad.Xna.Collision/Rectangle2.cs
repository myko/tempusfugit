using Microsoft.Xna.Framework;

namespace RaspberryRoad.Xna.Collision
{
    public struct Rectangle2
    {
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomLeft;
        public Vector2 BottomRight;

        public Line2 Top;
        public Line2 Left;
        public Line2 Right;
        public Line2 Bottom;

        public Vector2 Center;

        public Rectangle2(float x1, float y1, float x2, float y2)
        {
            TopLeft = new Vector2(x1, y1);
            TopRight = new Vector2(x2, y1);
            BottomLeft = new Vector2(x1, y2);
            BottomRight = new Vector2(x2, y2);

            Top = new Line2(TopLeft, TopRight);
            Left = new Line2(BottomLeft, TopLeft);
            Right = new Line2(TopRight, BottomRight);
            Bottom = new Line2(BottomRight, BottomLeft);

            Center = new Vector2((x1 + x2) / 2, (y1 + y2) / 2);
        }

        public Rectangle2(Vector2 topLeft, Vector2 bottomRight)
            : this(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y)
        {
        }

        public static Rectangle2 operator+ (Rectangle2 rectangle, Vector2 v)
        {
            return new Rectangle2(rectangle.TopLeft + v, rectangle.BottomRight + v);
        }
    }
}
