using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RaspberryRoad.Xna.Collision;

namespace RaspberryRoad.TempusFugit
{
    public class Door: Entity
    {
        public Vector2 Position { get; set; }
        public bool IsOpen { get; set; }

        public Door(Model model)
            : base(model)
        {
            Position = new Vector2();
        }

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }

        public override Matrix GetMatrix()
        {
            return Matrix.CreateTranslation(Position.X, 2, IsOpen ? -3.1f : 1);
        }

        public override IEnumerable<Line2> GetCollisionGeometry()
        {
            if (!IsOpen)
            {
                yield return new Line2(new Vector2(Position.X - 0.5f, 0), new Vector2(Position.X - 0.5f, 4));
                yield return new Line2(new Vector2(Position.X + 0.5f, 4), new Vector2(Position.X + 0.5f, 0));
                yield return new Line2(new Vector2(Position.X - 0.5f, 4), new Vector2(Position.X + 0.5f, 4));
                yield return new Line2(new Vector2(Position.X + 0.5f, 0), new Vector2(Position.X - 0.5f, 0));
            }
        }
    }
}
