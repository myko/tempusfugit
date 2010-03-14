using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RaspberryRoad.TempusFugit
{
    public class Door
    {
        public Position Position { get; set; }
        public bool IsOpen { get; set; }

        public Door()
        {
            Position = new Position();
        }

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }

        public bool CanPass(Position objectPosition, float delta)
        {
            return CanPass(objectPosition, new Position() { X = objectPosition.X + delta });
        }

        public bool CanPass(Position currentPosition, Position newPosition)
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

        public Matrix GetMatrix()
        {
            return Matrix.CreateTranslation(Position.X, 2, IsOpen ? -3.1f : 1);
        }
    }
}
