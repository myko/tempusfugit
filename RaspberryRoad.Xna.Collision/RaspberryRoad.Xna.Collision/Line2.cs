using Microsoft.Xna.Framework;

namespace RaspberryRoad.Xna.Collision
{
    public struct Line2
    {
        public Vector2 Start;
        public Vector2 End;

        public Line2(float x1, float y1, float x2, float y2)
        {
            Start = new Vector2(x1, y1);
            End = new Vector2(x2, y2);
        }

        public Line2(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public Vector2 Normal()
        {
            var direction = Vector2.Normalize(End - Start);
            return new Vector2(-direction.Y, direction.X);
        }

        public Vector2 Direction()
        {
            return Vector2.Normalize(End - Start);
        }
    }
}
