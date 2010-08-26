using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RaspberryRoad.Xna.Collision;

namespace RaspberryRoad.TempusFugit
{
    public abstract class Entity
    {
        public Model Model { get; set; }

        public abstract Matrix GetMatrix();

        public Entity(Model model)
        {
            this.Model = model;
        }

        public abstract IEnumerable<Line2> GetCollisionGeometry();
    }
}
