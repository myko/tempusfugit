using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RaspberryRoad.Xna.Collision;

namespace RaspberryRoad.TempusFugit
{
    public class Door: Entity
    {
        public Vector2 Position { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool IsOpen { get; set; }

        public Door(Model model)
            : base(model)
        {
            Position = new Vector2();
            Width = 1;
            Height = 4;
        }

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }

        public override Matrix GetMatrix()
        {
            return Matrix.CreateTranslation(Position.X, Position.Y, IsOpen ? -3.1f : 1);
        }

        public override IEnumerable<Line2> GetCollisionGeometry()
        {
            if (!IsOpen)
            {
                var halfWidth = Width / 2f;
                var halfHeight = Height / 2f;
                yield return new Line2(new Vector2(Position.X - halfWidth, Position.Y - halfHeight), new Vector2(Position.X - halfWidth, Position.Y + halfHeight));
                yield return new Line2(new Vector2(Position.X + halfWidth, Position.Y + halfHeight), new Vector2(Position.X + halfWidth, Position.Y - halfHeight));
                yield return new Line2(new Vector2(Position.X - halfWidth, Position.Y + halfHeight), new Vector2(Position.X + halfWidth, Position.Y + halfHeight));
                yield return new Line2(new Vector2(Position.X + halfWidth, Position.Y - halfHeight), new Vector2(Position.X - halfWidth, Position.Y - halfHeight));
            }
        }
    }
}
