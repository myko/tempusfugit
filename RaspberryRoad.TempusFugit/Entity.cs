using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

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
    }
}
