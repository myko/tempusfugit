using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public bool CanPass(Vector2 objectPosition, float delta)
        {
            return CanPass(objectPosition, new Vector2() { X = objectPosition.X + delta });
        }

        public bool CanPass(Vector2 currentPosition, Vector2 newPosition)
        {
            if (!IsOpen)
            {
                if ((currentPosition.X < this.Position.X) && (newPosition.X > this.Position.X))
                    return false;

                if ((currentPosition.X > this.Position.X) && (newPosition.X < this.Position.X))
                    return false;
            }

            return true;
        }

        public override Matrix GetMatrix()
        {
            return Matrix.CreateTranslation(Position.X, 2, IsOpen ? -3.1f : 1);
        }
    }
}
